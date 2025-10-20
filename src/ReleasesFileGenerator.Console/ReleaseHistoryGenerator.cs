using System.Text.Json;
using System.Text.RegularExpressions;
using Flamenco.Packaging.Dpkg;
using Microsoft.Extensions.Logging;
using ReleasesFileGenerator.Console.Helpers;
using ReleasesFileGenerator.Console.Models;
using ReleasesFileGenerator.Launchpad.Types;
using ReleasesFileGenerator.Launchpad.Types.Enums;
using ReleasesFileGenerator.Types;
using ReleasesFileGenerator.Types.ReleasesFile;

namespace ReleasesFileGenerator.Console;

public static class ReleaseHistoryGenerator
{
    private static readonly HttpClient HttpClient = new();

    public static async Task Generate(
        DirectoryInfo workingDirectory,
        AvailableVersionEntry versionEntry,
        Channel channel,
        Archive ubuntuArchive,
        DistroSeries distroSeries,
        ILoggerFactory? loggerFactory = null)
    {
        var logger = default(ILogger);
        if (loggerFactory is not null)
        {
            logger = loggerFactory.CreateLogger(nameof(ReleaseIndexGenerator));
        }

        logger?.LogInformation("Creating release history for channel {ChannelVersion}", channel.ChannelVersion);

        var channelDetailsReleases = new List<Release>();
        var channelDetails = new ChannelDetails
        {
            ChannelVersion = channel.ChannelVersion,
            LatestRelease = channel.LatestRelease,
            LatestRuntime = channel.LatestRuntime,
            LatestSdk = channel.LatestSdk,
            SupportPhase = channel.SupportPhase,
            ReleaseType = channel.ReleaseType,
            EolDate = channel.EolDate,
            Releases = channelDetailsReleases
        };

        var sources =
            await ArchiveActions.GetPublishedSourcesForVersion(ubuntuArchive, distroSeries, versionEntry);

        sources = sources
            .Where(s => s.DatePublished is not null && s.Pocket != ArchivePocket.Proposed)
            .OrderByDescending(s => DotnetPackageVersion.Create(s.SourcePackageName, s.SourcePackageVersion))
            .ToList()
            .AsReadOnly();

        var sourceGroup = sources.GroupBy(s =>
        {
            var version = DotnetPackageVersion.Create(s.SourcePackageName, s.SourcePackageVersion);
            return (version.UpstreamRuntimeVersion, version.UpstreamSdkVersion);
        });

        logger?.LogInformation("Found {Count} published sources for version {Version}",
            sources.Count, versionEntry.ChannelVersion);

        foreach (var sourcePackageGroup in sourceGroup)
        {
            var securityRelease = false;
            // Take the first and only source package out of the grouping. If more than one package exists for the same
            // .NET release, e.g. 8.0.100-8.0.1-0ubuntu1~22.04.1 and 8.0.100-8.0.1-0ubuntu1~22.04.2, then
            // consider the earliest one published, i.e., the one with the lowest Ubuntu patch version.
            // If that grouping also contains a source published to the security pocket, take that as priority.
            var sourcePackage = sourcePackageGroup.First();
            if (sourcePackageGroup.Count() > 1)
            {
                var orderedGroup = sourcePackageGroup
                    .OrderByDescending(s => s.DatePublished)
                    .ToList();

                sourcePackage = orderedGroup.LastOrDefault(s => s.Pocket == ArchivePocket.Security)
                                ?? orderedGroup.Last();

                securityRelease = sourcePackage.Pocket == ArchivePocket.Security;
            }

            var sourcePackageVersion =
                DotnetPackageVersion.Create(sourcePackage.SourcePackageName, sourcePackage.SourcePackageVersion);

            logger?.LogInformation("Processing source package version {SourcePackageVersion} from {Pocket} pocket",
                sourcePackage.SourcePackageVersion, sourcePackage.Pocket);

            Release release;

            if (sourcePackageVersion.UpstreamRuntimeVersion is not null &&
                sourcePackageVersion.UpstreamRuntimeVersion.IsStable)
            {
                release = new Release
                {
                    ReleaseVersion = sourcePackageVersion.UpstreamRuntimeVersion,
                    ReleaseDate = DateOnly.FromDateTime(sourcePackage.DatePublished!.Value.UtcDateTime),
                    Security = securityRelease,
                    Runtime = new Runtime
                    {
                        Version = sourcePackageVersion.UpstreamRuntimeVersion,
                        VersionDisplay = sourcePackageVersion.UpstreamRuntimeVersion.ToString()
                    },
                    Sdk = new Sdk
                    {
                        Version = sourcePackageVersion.UpstreamSdkVersion,
                        VersionDisplay = sourcePackageVersion.UpstreamSdkVersion.ToString(),
                        RuntimeVersion = sourcePackageVersion.UpstreamRuntimeVersion
                    },
                    Sdks = [
                        new Sdk
                        {
                            Version = sourcePackageVersion.UpstreamSdkVersion,
                            VersionDisplay = sourcePackageVersion.UpstreamSdkVersion.ToString(),
                            RuntimeVersion = sourcePackageVersion.UpstreamRuntimeVersion
                        }
                    ],
                    AspNetCoreRuntime = new AspNetCoreRuntime
                    {
                        Version = sourcePackageVersion.UpstreamRuntimeVersion,
                        VersionDisplay = sourcePackageVersion.UpstreamRuntimeVersion.ToString()
                    }
                };
            }
            else
            {
                logger?.LogInformation(
                    "{Name} {Version} is a pre-release version or does not contain the runtime version in the source " +
                    "package version. Downloading packages to determine the versions.",
                    sourcePackage.SourcePackageName, sourcePackage.SourcePackageVersion);

                var binaryFiles = await sourcePackage.GetBinaryFileUrls();

                // Default to amd64 binaries
                binaryFiles = binaryFiles.Where(u => u.ToString().Contains("amd64")).ToList();

                var runtimePackageFileUrl =
                    binaryFiles.Single(u => u.ToString().Contains(versionEntry.RuntimeBinaryPackageName)
                        && u.ToString().EndsWith(".deb"));
                var aspNetCoreRuntimePackageFileUrl =
                    binaryFiles.Single(u => u.ToString().Contains(versionEntry.AspNetCoreRuntimeBinaryPackageName)
                                            && u.ToString().EndsWith(".deb"));
                var sdkPackageFileUrl =
                    binaryFiles.Single(u => u.ToString().Contains(versionEntry.SdkBinaryPackageName)
                                            && !u.ToString().Contains("source-built-artifacts")
                                            && u.ToString().EndsWith(".deb"));

                var versions = await ArchiveActions.ReadVersionFromDotVersionFiles(
                    runtimePackageFileUrl,
                    aspNetCoreRuntimePackageFileUrl,
                    sdkPackageFileUrl,
                    workingDirectory,
                    shouldDeleteFiles: true);

                release = new Release
                {
                    ReleaseVersion = versions.RuntimeVersion,
                    ReleaseDate = DateOnly.FromDateTime(sourcePackage.DatePublished!.Value.UtcDateTime),
                    Security = securityRelease,
                    Runtime = new Runtime
                    {
                        Version = versions.RuntimeVersion,
                        VersionDisplay = versions.RuntimeVersion.ToString()
                    },
                    Sdk = new Sdk
                    {
                        Version = versions.SdkVersion,
                        VersionDisplay = versions.SdkVersion.ToString(),
                        RuntimeVersion = versions.RuntimeVersion
                    },
                    Sdks = [
                        new Sdk
                        {
                            Version = versions.SdkVersion,
                            VersionDisplay = versions.SdkVersion.ToString(),
                            RuntimeVersion = versions.RuntimeVersion
                        }
                    ],
                    AspNetCoreRuntime = new AspNetCoreRuntime
                    {
                        Version = versions.RuntimeVersion,
                        VersionDisplay = versions.RuntimeVersion.ToString()
                    }
                };
            }

            // If security release, analyze changelog for CVE numbers
            if (release.Security)
            {
                var changelogUrl = await sourcePackage.GetChangelogUrl();
                var changelogRequest = await HttpClient.GetStringAsync(changelogUrl);

                using var changelogReader = new DpkgChangelogReader(new StringReader(changelogRequest));

                var entry = await changelogReader.ReadChangelogEntryAsync();
                if (entry is { HasValue: true, Value: not null })
                {
                    var cvePattern = @"CVE-\d{4}-\d{4,7}";
                    var cveMatches = Regex.Matches(entry.Value.Value.Description, cvePattern, RegexOptions.IgnoreCase);
                    var cveNumbers = cveMatches.Select(m => Cve.Parse(m.Value, null)).Distinct().ToArray();

                    if (cveNumbers.Any())
                    {
                        release.CveList = cveNumbers;
                    }
                }
            }

            channelDetailsReleases.Add(release);
        }

        var latestReleaseDate = channelDetailsReleases
            .MaxBy(r => r.ReleaseDate)?
            .ReleaseDate;

        if (latestReleaseDate is not null)
        {
            channelDetails.LatestReleaseDate = latestReleaseDate.Value;
        }

        var releaseDirectory = Directory.CreateDirectory(Path.Join(workingDirectory.FullName, channel.ChannelVersion));
        var filePath = Path.Join(releaseDirectory.FullName, "releases.json");
        await File.WriteAllTextAsync(
            filePath,
            contents: JsonSerializer.Serialize(channelDetails, new JsonSerializerOptions
            {
                WriteIndented = true
            }));

        logger?.LogInformation("Release history for channel {ChannelVersion} written to {FilePath}",
            channel.ChannelVersion, filePath);
    }
}
