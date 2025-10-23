using System.Collections.ObjectModel;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ReleasesFileGenerator.Console.Models;
using ReleasesFileGenerator.Launchpad.Types;
using ReleasesFileGenerator.Types;
using ReleasesFileGenerator.Types.ReleasesFile;
using Archive = ReleasesFileGenerator.Launchpad.Types.Archive;

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
    /// <param name="backportsArchive">.NET backports PPA archive object to query.</param>
    /// <param name="distroSeries">Distro series object to query.</param>
    /// <param name="loggerFactory">Logger factory.</param>
    public static async Task Generate(
        DirectoryInfo workingDirectory,
        ReadOnlyCollection<AvailableVersionEntry> availableVersions,
        Archive ubuntuArchive,
        Archive backportsArchive,
        DistroSeries distroSeries,
        ILoggerFactory? loggerFactory = null)
    {
        var logger = default(ILogger);
        if (loggerFactory is not null)
        {
            logger = loggerFactory.CreateLogger(nameof(ReleaseIndexGenerator));
        }

        // Get releases.json URL origin from the environment variable, if defined.
        var urlOrigin = Environment.GetEnvironmentVariable("RELEASES_JSON_URL_ORIGIN");
        if (string.IsNullOrWhiteSpace(urlOrigin))
        {
            urlOrigin = workingDirectory.FullName;
        }

        logger?.LogInformation("Using releases.json URL origin: {UrlOrigin}", urlOrigin);

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
                ReleasesJsonUrl = new Uri($"{urlOrigin}/{version.ChannelVersion}/releases.json", UriKind.Absolute)
            };

            var channelDetails = await ReleaseHistoryGenerator.Generate(
                workingDirectory,
                version,
                channel,
                ubuntuArchive,
                backportsArchive,
                distroSeries,
                loggerFactory);

            channel.LatestRelease = channelDetails.LatestRelease;
            channel.LatestSdk = channelDetails.LatestSdk;
            channel.LatestRuntime = channelDetails.LatestRuntime;
            channel.LatestReleaseDate = channelDetails.LatestReleaseDate;
            channel.Security = channelDetails.Releases.MaxBy(r => r.ReleaseDate)!.Security;

            logger?.LogInformation("Version {Version} latest versions: Runtime: {Runtime}, SDK: {Sdk}",
                version.ChannelVersion, channel.LatestRuntime, channel.LatestSdk);

            index.Add(channel);
        }

        var releasesFile = new Types.ReleasesFile.Index
        {
            ReleasesIndex = index
        };

        await File.WriteAllTextAsync(
            $"{workingDirectory.FullName}/releases-index.json",
            JsonSerializer.Serialize(releasesFile, new JsonSerializerOptions
            {
                WriteIndented = true
            }));

        logger?.LogInformation("Releases file generated at {Path}",
            $"{workingDirectory.FullName}/releases-index.json");
    }
}
