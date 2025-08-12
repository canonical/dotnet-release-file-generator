using System.Collections.ObjectModel;
using System.Text.Json;
using Microsoft.Extensions.Logging;
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
using Spectre.Console;

namespace ReleasesFileGenerator.Console;

public class Program
{
    private const string MicrosoftReleasesUrl =
        "https://builds.dotnet.microsoft.com/dotnet/release-metadata/releases-index.json";

    private static readonly DirectoryInfo WorkingDirectory =
        Directory.CreateTempSubdirectory("releases-file-generator-");

    private static ILogger? _logger;

    private static async Task<int> Main(string[] args)
    {
        #region Logging
        using var factory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole(options =>
            {
                options.LogToStandardErrorThreshold = LogLevel.Error;
            });
        });
        _logger = factory.CreateLogger<Program>();
        #endregion

        var series = args.ElementAtOrDefault(0);
        if (string.IsNullOrWhiteSpace(series))
        {
            _logger.LogError("No series provided.");
            return -1;
        }

        _logger.LogInformation("Generating releases file for series {Series}", series);
        _logger.LogInformation("Using working directory {Path}", WorkingDirectory.FullName);

        var manifest = await File.ReadAllTextAsync($"Manifests/{series}.json");
        var currentlyAvailableVersions = JsonSerializer.Deserialize<List<AvailableVersionEntry>>(manifest);

        if (currentlyAvailableVersions is null)
        {
            _logger.LogError("Failed to deserialize available versions.");
            return -1;
        }

        _logger.LogInformation("Found {Count} available versions for series {Series}",
            currentlyAvailableVersions.Count, series);

        var ubuntuArchive = await Archives.GetByReference(GetByReferenceOptions.Ubuntu);
        var distribution = await ubuntuArchive.GetDistributionAsync();
        var distroSeries = await distribution.GetSeriesAsync(new GetSeriesOptions(series));

        var index = new List<Channel>();
        foreach (var version in currentlyAvailableVersions)
        {
            _logger.LogInformation("Processing version {Version} ({SourcePackageName})",
                version.ChannelVersion, version.SourcePackageName);

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
                _logger.LogError("Could not determine latest published version.");
            }

            var latestDotNetVersion = DotnetPackageVersion.Create(
                latestRelease!.SourcePackageName,
                latestRelease.SourcePackageVersion);

            _logger.LogInformation("Latest release for version {Version} is {Release}",
                version.ChannelVersion, latestRelease.SourcePackageVersion);

            // Verify if the latest version was a security release by checking if it was published to the
            // security pocket.
            channel.Security = latestSecurityRelease is not null &&
                               latestRelease.SourcePackageVersion == latestSecurityRelease.SourcePackageVersion;

            _logger.LogInformation("Version {Version} security release status: {IsSecurityRelease}",
                latestRelease.SourcePackageVersion, channel.Security);

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
                _logger.LogInformation(
                    "{Name} {Version} is a pre-release version or does not contain the runtime version in the source package version. " +
                    "Downloading packages to determine the versions.",
                    latestRelease.SourcePackageName, latestRelease.SourcePackageVersion);

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

            _logger.LogInformation("Version {Version} latest versions: Runtime: {Runtime}, SDK: {Sdk}",
                version.ChannelVersion, channel.LatestRuntime, channel.LatestSdk);

            index.Add(channel);
        }

        var releasesFile = new Types.ReleasesFile.Index
        {
            ReleasesIndex = index
        };

        await File.WriteAllTextAsync(
            $"{WorkingDirectory.FullName}/releases-index.json",
            JsonSerializer.Serialize(releasesFile));

        _logger.LogInformation("Releases file generated at {Path}",
            $"{WorkingDirectory.FullName}/releases-index.json");

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
        await AnsiConsole.Progress()
            .HideCompleted(true)
            .Columns(
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new RemainingTimeColumn(),
                new SpinnerColumn(),
                new DownloadedColumn(),
                new TransferSpeedColumn())
            .StartAsync(async ctx =>
            {
                var runtimeDownloadProgressTask =
                    ctx.AddTask(".NET Runtime", autoStart: true, maxValue: runtimePackageFile.Size!.Value);
                var aspnetCoreRuntimeDownloadProgressTask =
                    ctx.AddTask("ASP.NET Core Runtime", autoStart: true, maxValue: aspnetCoreRuntimePackageFile.Size!.Value);
                var sdkDownloadProgressTask =
                    ctx.AddTask(".NET SDK", autoStart: true, maxValue: sdkPackageFile.Size!.Value);

                var runtimeDownloadTask = FileDownloader.DownloadFileAsync(runtimePackageFile.Url,
                    WorkingDirectory.FullName, new Progress<double>(percent =>
                        runtimeDownloadProgressTask.Increment(percent)));
                var aspnetCoreRuntimeDownloadTask = FileDownloader.DownloadFileAsync(aspnetCoreRuntimePackageFile.Url,
                    WorkingDirectory.FullName, new Progress<double>(percent =>
                        aspnetCoreRuntimeDownloadProgressTask.Increment(percent)));
                var sdkDownloadTask = FileDownloader.DownloadFileAsync(sdkPackageFile.Url,
                    WorkingDirectory.FullName, new Progress<double>(percent =>
                        sdkDownloadProgressTask.Increment(percent)));

                await Task.WhenAll(runtimeDownloadTask, aspnetCoreRuntimeDownloadTask, sdkDownloadTask);
            });

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
