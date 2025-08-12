using System.Collections.ObjectModel;
using System.Text.Json;
using ReleasesFileGenerator.Console.Helpers;
using ReleasesFileGenerator.Console.Models;
using ReleasesFileGenerator.Launchpad.Collections;
using ReleasesFileGenerator.Launchpad.Collections.Options.Archives;
using ReleasesFileGenerator.Launchpad.Types;
using ReleasesFileGenerator.Launchpad.Types.Enums;
using ReleasesFileGenerator.Launchpad.Types.Models;
using ReleasesFileGenerator.Launchpad.Types.Options.Archive;
using ReleasesFileGenerator.Launchpad.Types.Options.BinaryPackagePublishingHistory;
using ReleasesFileGenerator.Launchpad.Types.Options.Distribution;
using ReleasesFileGenerator.Types;
using ReleasesFileGenerator.Types.ReleasesFile;

namespace ReleasesFileGenerator.Console;

public class Program
{
    private const string MicrosoftReleasesUrl =
        "https://builds.dotnet.microsoft.com/dotnet/release-metadata/releases-index.json";

    private static DirectoryInfo WorkingDirectory = Directory.CreateTempSubdirectory("releases-file-generator-");

    private static async Task<int> Main(string[] args)
    {
        var series = args.ElementAtOrDefault(0);
        if (string.IsNullOrWhiteSpace(series))
        {
            await System.Console.Error.WriteLineAsync("No series provided.");
            return -1;
        }

        var manifest = await File.ReadAllTextAsync($"Manifests/{series}.json");
        var currentlyAvailableVersions = JsonSerializer.Deserialize<List<AvailableVersionEntry>>(manifest);

        if (currentlyAvailableVersions is null)
        {
            await System.Console.Error.WriteLineAsync("Failed to deserialize available versions.");
            return -1;
        }

        var ubuntuArchive = await Archives.GetByReference(GetByReferenceOptions.Ubuntu);
        var distribution = await ubuntuArchive.GetDistributionAsync();
        var distroSeries = await distribution.GetSeriesAsync(new GetSeriesOptions(series));

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

            var publishedSources =
                await GetPublishedSourcesForVersion(ubuntuArchive, distroSeries, version);

            // We do not consider packages still in proposed to be generally available in Ubuntu.
            var latestRelease = publishedSources
                .Where(s => s.Pocket is not ArchivePocket.Proposed)
                .Where(s => s.Status is ArchivePublishingStatus.Published)
                .MaxBy(s => s.DatePublished);

            var latestSecurityRelease = publishedSources
                .Where(s => s.Pocket is ArchivePocket.Security)
                .Where(s => s.Status is ArchivePublishingStatus.Published)
                .MaxBy(s => s.DatePublished);

            if (latestRelease is null)
            {
                throw new ApplicationException("Could not determine latest published version.");
            }

            var latestDotNetVersion = DotnetPackageVersion.Create(
                latestRelease.SourcePackageName,
                latestRelease.SourcePackageVersion);

            // Verify if the latest version was a security release by checking if it was published to the
            // security pocket.
            channel.Security = latestSecurityRelease is not null &&
                               latestRelease.SourcePackageVersion == latestSecurityRelease.SourcePackageVersion;

            // .NET versions equal to or higher than 8.0 contain both the Runtime and SDK versions in the source
            // package version. Also, if the .NET version is not a pre-release version, we can also use the versions
            // contained in the source package version.
            // Otherwise, it is necessary to download the runtime and SDK packages from the Ubuntu archive in order
            // to determine the versions from the .version files.
            if (latestDotNetVersion.UpstreamRuntimeVersion is not null &&
                latestDotNetVersion.UpstreamRuntimeVersion.IsStable)
            {
                channel.LatestRelease = latestDotNetVersion.UpstreamRuntimeVersion;
                channel.LatestSdk = latestDotNetVersion.UpstreamSdkVersion;
                channel.LatestRuntime = latestDotNetVersion.UpstreamRuntimeVersion;
            }
            else
            {
                System.Console.WriteLine(
                    $"Analyzing source {latestRelease.SourcePackageName} [{latestRelease.SourcePackageVersion}]");

                var runtimePackageFile = await GetPackageFile(
                    ubuntuArchive,
                    version.RuntimeBinaryPackageName,
                    latestDotNetVersion.GetUbuntuRuntimePackageVersion() ??
                        latestDotNetVersion.GetUbuntuSdkPackageVersion());

                var aspnetCoreRuntimePackageFile = await GetPackageFile(
                    ubuntuArchive,
                    version.AspNetCoreRuntimeBinaryPackageName,
                    latestDotNetVersion.GetUbuntuRuntimePackageVersion() ??
                        latestDotNetVersion.GetUbuntuSdkPackageVersion());

                var sdkPackageFile = await GetPackageFile(
                    ubuntuArchive,
                    version.SdkBinaryPackageName,
                    latestDotNetVersion.GetUbuntuSdkPackageVersion());

                var latestVersions =
                    await ReadVersionFromDotVersionFiles(
                        runtimePackageFile,
                        aspnetCoreRuntimePackageFile,
                        sdkPackageFile);

                channel.LatestRelease = latestVersions.RuntimeVersion;
                channel.LatestRuntime = latestVersions.RuntimeVersion;
                channel.LatestSdk = latestVersions.SdkVersion;
            }

            index.Add(channel);
        }

        var releasesFile = new Types.ReleasesFile.Index
        {
            ReleasesIndex = index
        };

        await File.WriteAllTextAsync(
            $"{WorkingDirectory.FullName}/releases-index.json",
            JsonSerializer.Serialize(releasesFile));

        return 0;
    }

    private static async Task<ReadOnlyCollection<SourcePackagePublishingHistory>> GetPublishedSourcesForVersion(
        Archive ubuntuArchive,
        DistroSeries distroSeries,
        AvailableVersionEntry version)
    {
        var publishedSources = new List<SourcePackagePublishingHistory>();
        var currentPageOfPublishedSources =
            await ubuntuArchive.GetPublishedSourcesAsync(new GetPublishedSourcesOptions
            {
                SourcePackageName = version.SourcePackageName,
                DistroSeriesLink = distroSeries.SelfLink
            });

        do
        {
            publishedSources.AddRange(currentPageOfPublishedSources.Entries);
            currentPageOfPublishedSources = currentPageOfPublishedSources.GetNextPageAsync().Result;
        } while (currentPageOfPublishedSources is not null);

        return publishedSources.AsReadOnly();
    }

    private static async Task<BinaryPackageFile> GetPackageFile(
        Archive ubuntuArchive,
        string binaryPackageName,
        string binaryPackageVersion)
    {
        var publishedBinaries = await ubuntuArchive.GetPublishedBinariesAsync(
            new GetPublishedBinariesOptions
            {
                BinaryPackageName = binaryPackageName,
                Version = binaryPackageVersion,
                ExactMatch = true
            });

        var package = publishedBinaries.Entries
            .First(p => p.DisplayName.Contains("amd64"));

        var packageFiles = await package.GetBinaryPackageFiles(new GetBinaryPackageFilesOptions
        {
            IncludeMetadata = true
        });

        return packageFiles.First();
    }

    private static async Task<(DotnetVersion RuntimeVersion, DotnetVersion AspNetRuntimeVersion, DotnetVersion SdkVersion)> ReadVersionFromDotVersionFiles(
        BinaryPackageFile runtimePackageFile,
        BinaryPackageFile aspnetCoreRuntimePackageFile,
        BinaryPackageFile sdkPackageFile)
    {
        var runtimeDownloadProgress = new Progress<double>(percent =>
            System.Console.WriteLine($"[.NET Runtime] Downloaded {percent:F2}%"));
        var runtimeDownloadTask = FileDownloader.DownloadFileAsync(runtimePackageFile.Url,
            WorkingDirectory.FullName, runtimeDownloadProgress);

        var aspnetCoreRuntimeDownloadProgress = new Progress<double>(percent =>
            System.Console.WriteLine($"[ASP.NET Core Runtime] Downloaded {percent:F2}%"));
        var aspnetCoreRuntimeDownloadTask = FileDownloader.DownloadFileAsync(aspnetCoreRuntimePackageFile.Url,
            WorkingDirectory.FullName, aspnetCoreRuntimeDownloadProgress);

        var sdkDownloadProgress = new Progress<double>(percent =>
            System.Console.WriteLine($"[.NET SDK] Downloaded {percent:F2}%"));
        var sdkDownloadTask = FileDownloader.DownloadFileAsync(sdkPackageFile.Url,
            WorkingDirectory.FullName, sdkDownloadProgress);

        await Task.WhenAll(runtimeDownloadTask, aspnetCoreRuntimeDownloadTask, sdkDownloadTask);

        DebFile.ExtractDebFile($"{WorkingDirectory.FullName}/{runtimePackageFile.Url.Segments.Last()}",
            WorkingDirectory.FullName);
        DebFile.ExtractDebFile($"{WorkingDirectory.FullName}/{aspnetCoreRuntimePackageFile.Url.Segments.Last()}",
            WorkingDirectory.FullName);
        DebFile.ExtractDebFile($"{WorkingDirectory.FullName}/{sdkPackageFile.Url.Segments.Last()}",
            WorkingDirectory.FullName);

        var runtimeVersion =
            DotnetVersion.Parse(
                DotVersionFile.Parse(
                        DotVersionFile.FindDotVersionFile(
                            $"{WorkingDirectory.FullName}/usr/lib/dotnet/shared/Microsoft.NETCore.App"))
                    .Version);
        var aspNetRuntimeVersion =
            DotnetVersion.Parse(
                DotVersionFile.Parse(
                        DotVersionFile.FindDotVersionFile(
                            $"{WorkingDirectory.FullName}/usr/lib/dotnet/shared/Microsoft.AspNetCore.App"))
                    .Version);
        var sdkVersion =
            DotnetVersion.Parse(
                DotVersionFile.Parse(
                        DotVersionFile.FindDotVersionFile($"{WorkingDirectory.FullName}/usr/lib/dotnet/sdk"))
                    .Version);


        File.Delete($"{WorkingDirectory.FullName}/{runtimePackageFile.Url.Segments.Last()}");
        File.Delete($"{WorkingDirectory.FullName}/{aspnetCoreRuntimePackageFile.Url.Segments.Last()}");
        File.Delete($"{WorkingDirectory.FullName}/{sdkPackageFile.Url.Segments.Last()}");
        Directory.Delete(Path.Join(WorkingDirectory.FullName, "usr"), recursive: true);

        return (runtimeVersion, aspNetRuntimeVersion, sdkVersion);
    }
}
