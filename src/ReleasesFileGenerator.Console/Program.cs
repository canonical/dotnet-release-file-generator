using System.Text.Json;
using Microsoft.Extensions.Logging;
using ReleasesFileGenerator.Console.Models;
using ReleasesFileGenerator.Launchpad.Collections;
using ReleasesFileGenerator.Launchpad.Collections.Options.Archives;
using ReleasesFileGenerator.Launchpad.Types.Options.Distribution;

namespace ReleasesFileGenerator.Console;

public static class Program
{
    private static DirectoryInfo? _workingDirectory;
    private static ILogger? _logger;

    private static async Task<int> Main(string[] args)
    {
        #region Logging
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole(options =>
            {
                options.LogToStandardErrorThreshold = LogLevel.Error;
            });
        });
        _logger = loggerFactory.CreateLogger(nameof(Program));
        #endregion

        var series = args.ElementAtOrDefault(0);
        if (string.IsNullOrWhiteSpace(series))
        {
            _logger.LogError("No series provided.");
            return -1;
        }

        var workingDirectoryPath = args.ElementAtOrDefault(1);
        if (string.IsNullOrWhiteSpace(workingDirectoryPath))
        {
            _workingDirectory = Directory.CreateTempSubdirectory("releases-file-generator-");
        }
        else
        {
            _workingDirectory = new DirectoryInfo(workingDirectoryPath);
            if (!_workingDirectory.Exists)
            {
                _workingDirectory.Create();
            }
        }

        _logger.LogInformation("Generating releases file for series {Series}", series);
        _logger.LogInformation("Using working directory {Path}", _workingDirectory.FullName);

        var executablePath = Environment.ProcessPath;
        var manifestsDirLocation = Path.Combine(
            Path.GetDirectoryName(executablePath) ?? string.Empty,
            "Manifests");

        var manifest = await File.ReadAllTextAsync(Path.Combine(manifestsDirLocation, $"{series}.json"));
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
        // Get the distribution series based on the provided series name.
        var distroSeries = await distribution.GetSeriesAsync(new GetSeriesOptions(series));

        await ReleaseIndexGenerator.Generate(
            _workingDirectory,
            currentlyAvailableVersions.AsReadOnly(),
            ubuntuArchive,
            distroSeries,
            loggerFactory);

        return 0;
    }
}
