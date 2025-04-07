using System.Diagnostics;
using System.Text.Json;
using ReleasesFileGenerator.Launchpad.Collections;
using ReleasesFileGenerator.Launchpad.Collections.Options.Archives;
using ReleasesFileGenerator.Launchpad.Types;
using ReleasesFileGenerator.Launchpad.Types.Enums;
using ReleasesFileGenerator.Launchpad.Types.Options.Archive;
using ReleasesFileGenerator.Launchpad.Types.Options.BinaryPackagePublishingHistory;
using ReleasesFileGenerator.Types;
using ReleasesFileGenerator.Types.ReleasesFile;

namespace ReleasesFileGenerator.Console;

public class Program
{
    private const string MicrosoftReleasesUrl =
        "https://builds.dotnet.microsoft.com/dotnet/release-metadata/releases-index.json";

    private static void Main(string[] args)
    {
        var workingDirectory = Directory.CreateTempSubdirectory("releases-file-generator-");
        var currentlyAvailableVersions = AvailableVersions.GetAvailableVersions();

        var ubuntuArchive = Archives.GetByReference(GetByReferenceOptions.Ubuntu).Result;

        var index = new List<Channel>();
        foreach (var version in currentlyAvailableVersions)
        {
            var channel = new Channel
            {
                ChannelVersion = version.ChannelVersion,
                Product = version.Product,
                SupportPhase = version.SupportPhase,
                ReleaseType = version.ReleaseType,
                EolDate = version.EolDate,

                LatestRelease = DotnetVersion.Parse("1.0.0"),
                LatestSdk = DotnetVersion.Parse("1.0.0"),
                LatestRuntime = DotnetVersion.Parse("1.0.0"),
                ReleasesJsonUrl = new Uri("https://not-a-url.com")
            };

            var publishedSources = new List<SourcePackagePublishingHistory>();
            var currentPageOfPublishedSources = ubuntuArchive.GetPublishedSourcesAsync(new GetPublishedSourcesOptions
            {
                SourcePackageName = version.SourcePackageName
            }).Result;

            do
            {
                publishedSources.AddRange(currentPageOfPublishedSources.Entries);
                currentPageOfPublishedSources = currentPageOfPublishedSources.GetNextPageAsync().Result;
            } while (currentPageOfPublishedSources is not null);

            System.Console.WriteLine($"Found {publishedSources.Count} published sources for {version.SourcePackageName}");

            var uniquePublishedSources = publishedSources
                .Where(s => s.Status is ArchivePublishingStatus.Published or ArchivePublishingStatus.Superseded)
                .DistinctBy(s =>
                {
                    var localVersion = DotnetPackageVersion.Create(s.SourcePackageName, s.SourcePackageVersion);
                    return (localVersion.UpstreamRuntimeVersion, localVersion.UpstreamSdkVersion);
                });

            var sourceOfInterest = uniquePublishedSources
                .MaxBy(s => s.DatePublished);

            if (sourceOfInterest is null)
            {
                throw new ApplicationException("Could not determine latest published version.");
            }

            var localVersion = DotnetPackageVersion.Create(sourceOfInterest.SourcePackageName,
                sourceOfInterest.SourcePackageVersion);

            System.Console.WriteLine(
                $"Analyzing source {sourceOfInterest.SourcePackageName} [{sourceOfInterest.SourcePackageVersion}]");

            var runtimePackagesTask = ubuntuArchive.GetPublishedBinariesAsync(new GetPublishedBinariesOptions
            {
                BinaryPackageName = version.RuntimeBinaryPackageName,
                Version = localVersion.GetUbuntuRuntimePackageVersion() ?? localVersion.GetUbuntuSdkPackageVersion(),
                ExactMatch = true
            });

            var sdkPackagesTask = ubuntuArchive.GetPublishedBinariesAsync(new GetPublishedBinariesOptions
            {
                BinaryPackageName = version.SdkBinaryPackageName,
                Version = localVersion.GetUbuntuSdkPackageVersion(),
                ExactMatch = true
            });

            Task.WaitAll(runtimePackagesTask, sdkPackagesTask);

            var runtimePackage = runtimePackagesTask.Result.Entries
                .First(p => p.DisplayName.Contains("amd64"));
            var sdkPackage = sdkPackagesTask.Result.Entries
                .First(p => p.DisplayName.Contains("amd64"));

            var runtimePackageFilesTask = runtimePackage.GetBinaryPackageFiles(new GetBinaryPackageFilesOptions
            {
                IncludeMetadata = true
            });
            var sdkPackageFilesTask = sdkPackage.GetBinaryPackageFiles(new GetBinaryPackageFilesOptions
            {
                IncludeMetadata = true
            });

            Task.WaitAll(runtimePackageFilesTask, sdkPackageFilesTask);

            var runtimePackageFile = runtimePackageFilesTask.Result.First();
            var sdkPackageFile = sdkPackageFilesTask.Result.First();

            var runtimeDownloadProgress = new Progress<double>(percent =>
                System.Console.WriteLine($"[Runtime] Downloaded {percent:F2}%"));
            var runtimeDownloadTask = FileDownloader.DownloadFileAsync(runtimePackageFile.Url,
                workingDirectory.FullName, runtimeDownloadProgress);

            var sdkDownloadProgress = new Progress<double>(percent =>
                System.Console.WriteLine($"[SDK] Downloaded {percent:F2}%"));
            var sdkDownloadTask = FileDownloader.DownloadFileAsync(sdkPackageFile.Url,
                workingDirectory.FullName, sdkDownloadProgress);

            Task.WaitAll(runtimeDownloadTask, sdkDownloadTask);

            ExtractDebFile($"{workingDirectory.FullName}/{runtimePackageFile.Url.Segments.Last()}",
                workingDirectory.FullName);
            ExtractDebFile($"{workingDirectory.FullName}/{sdkPackageFile.Url.Segments.Last()}",
                workingDirectory.FullName);

            channel.LatestRelease =
                DotnetVersion.Parse(
                    DotVersionFile.Parse(
                        FindDotVersionFile($"{workingDirectory.FullName}/usr/lib/dotnet/shared/Microsoft.NETCore.App"))
                        .Version);
            channel.LatestRuntime =
                DotnetVersion.Parse(
                    DotVersionFile.Parse(
                        FindDotVersionFile($"{workingDirectory.FullName}/usr/lib/dotnet/shared/Microsoft.NETCore.App"))
                        .Version);
            channel.LatestSdk =
                DotnetVersion.Parse(
                    DotVersionFile.Parse(
                        FindDotVersionFile($"{workingDirectory.FullName}/usr/lib/dotnet/sdk"))
                        .Version);

            index.Add(channel);

            File.Delete($"{workingDirectory.FullName}/{runtimePackageFile.Url.Segments.Last()}");
            File.Delete($"{workingDirectory.FullName}/{sdkPackageFile.Url.Segments.Last()}");
            Directory.Delete(Path.Join(workingDirectory.FullName, "usr"), recursive: true);
        }

        var releasesFile = new Types.ReleasesFile.Index
        {
            ReleasesIndex = index
        };

        File.WriteAllText($"{workingDirectory.FullName}/releases-index.json", JsonSerializer.Serialize(releasesFile));

        return;

        string FindDotVersionFile(string directory)
        {
            var files = Directory.GetFiles(directory, ".version", SearchOption.AllDirectories);
            if (files.Length == 0)
            {
                throw new FileNotFoundException("No .version file found in the specified directory.");
            }

            return files[0];
        }
    }

    private static void ExtractDebFile(string filePath, string destinationDirectory)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("The specified .deb file does not exist.", filePath);
        }

        var processStartInfo = new ProcessStartInfo
        {
            FileName = "dpkg",
            Arguments = $"--extract {filePath} .",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = destinationDirectory
        };

        using var process = Process.Start(processStartInfo);

        if (process is null)
        {
            throw new ApplicationException("Could not start the dpkg process.");
        }

        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            var errorMessage = process.StandardError.ReadToEnd();
            throw new ApplicationException($"dpkg failed with exit code {process.ExitCode}: {errorMessage}");
        }
    }
}
