using System.Collections.ObjectModel;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ReleasesFileGenerator.Console.Helpers;
using ReleasesFileGenerator.Console.Models;
using ReleasesFileGenerator.Launchpad.Types;
using ReleasesFileGenerator.Launchpad.Types.Enums;
using ReleasesFileGenerator.Types;
using ReleasesFileGenerator.Types.ReleasesFile;

namespace ReleasesFileGenerator.Console;

public static class ReleaseIndexGenerator
{
    /// <summary>
    /// Generate the releases index file for the specified Ubuntu distribution series.
    /// This method retrieves the available versions from the provided archive, determines the latest release,
    /// runtime, and SDK versions for each channel, and writes the results to a JSON file, <c>releases-index.json</c>
    /// in the specified working directory.
    /// </summary>
    /// <param name="workingDirectory">Working directory of the script.</param>
    /// <param name="availableVersions">List of available .NET versions for the specified Ubuntu series.</param>
    /// <param name="ubuntuArchive">Launchpad archive object to query.</param>
    /// <param name="distroSeries">Distro series object to query.</param>
    /// <param name="loggerFactory">Logger factory.</param>
    public static async Task Generate(
        DirectoryInfo workingDirectory,
        ReadOnlyCollection<AvailableVersionEntry> availableVersions,
        Archive ubuntuArchive,
        DistroSeries distroSeries,
        ILoggerFactory? loggerFactory = null)
    {
        var logger = default(ILogger);
        if (loggerFactory is not null)
        {
            logger = loggerFactory.CreateLogger(nameof(ReleaseIndexGenerator));
        }

        var index = new List<Channel>();
        foreach (var version in availableVersions)
        {
            logger?.LogInformation("Processing version {Version} ({SourcePackageName})",
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
                await ArchiveActions.GetPublishedSourcesForVersion(ubuntuArchive, distroSeries, version);

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
                logger?.LogError("Could not determine latest published version.");
            }

            var latestDotNetVersion = DotnetPackageVersion.Create(
                latestRelease!.SourcePackageName,
                latestRelease.SourcePackageVersion);

            logger?.LogInformation("Latest release for version {Version} is {Release}",
                version.ChannelVersion, latestRelease.SourcePackageVersion);

            // Verify if the latest version was a security release by checking if it was published to the
            // security pocket.
            channel.Security = latestSecurityRelease is not null &&
                               latestRelease.SourcePackageVersion == latestSecurityRelease.SourcePackageVersion;

            logger?.LogInformation("Version {Version} security release status: {IsSecurityRelease}",
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
                logger?.LogInformation(
                    "{Name} {Version} is a pre-release version or does not contain the runtime version in the source " +
                    "package version. Downloading packages to determine the versions.",
                    latestRelease.SourcePackageName, latestRelease.SourcePackageVersion);

                var runtimePackageFile = await ArchiveActions.GetPackageFile(
                    ubuntuArchive,
                    version.RuntimeBinaryPackageName,
                    latestDotNetVersion.GetUbuntuRuntimePackageVersion() ??
                        latestDotNetVersion.GetUbuntuSdkPackageVersion());

                var aspnetCoreRuntimePackageFile = await ArchiveActions.GetPackageFile(
                    ubuntuArchive,
                    version.AspNetCoreRuntimeBinaryPackageName,
                    latestDotNetVersion.GetUbuntuRuntimePackageVersion() ??
                        latestDotNetVersion.GetUbuntuSdkPackageVersion());

                var sdkPackageFile = await ArchiveActions.GetPackageFile(
                    ubuntuArchive,
                    version.SdkBinaryPackageName,
                    latestDotNetVersion.GetUbuntuSdkPackageVersion());

                var latestVersions =
                    await ArchiveActions.ReadVersionFromDotVersionFiles(
                        runtimePackageFile,
                        aspnetCoreRuntimePackageFile,
                        sdkPackageFile,
                        workingDirectory,
                        shouldDeleteFiles: false);

                channel.LatestRelease = latestVersions.RuntimeVersion;
                channel.LatestRuntime = latestVersions.RuntimeVersion;
                channel.LatestSdk = latestVersions.SdkVersion;
            }

            logger?.LogInformation("Version {Version} latest versions: Runtime: {Runtime}, SDK: {Sdk}",
                version.ChannelVersion, channel.LatestRuntime, channel.LatestSdk);

            index.Add(channel);

            await ReleaseHistoryGenerator.Generate(
                workingDirectory,
                version,
                channel,
                ubuntuArchive,
                distroSeries,
                loggerFactory);
        }

        var releasesFile = new Types.ReleasesFile.Index
        {
            ReleasesIndex = index
        };

        await File.WriteAllTextAsync(
            $"{workingDirectory.FullName}/releases-index.json",
            JsonSerializer.Serialize(releasesFile));

        logger?.LogInformation("Releases file generated at {Path}",
            $"{workingDirectory.FullName}/releases-index.json");
    }
}
