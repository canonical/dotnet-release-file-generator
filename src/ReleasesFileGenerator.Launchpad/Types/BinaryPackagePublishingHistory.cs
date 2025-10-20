using System.Text.Json;
using System.Text.Json.Serialization;
using ReleasesFileGenerator.Launchpad.Models;
using ReleasesFileGenerator.Launchpad.Types.Enums;
using ReleasesFileGenerator.Launchpad.Types.Models;
using ReleasesFileGenerator.Launchpad.Types.Options.BinaryPackagePublishingHistory;

namespace ReleasesFileGenerator.Launchpad.Types;

public class BinaryPackagePublishingHistory : LaunchpadEntryType
{
    [JsonPropertyName("display_name")]
    public required string DisplayName { get; set; }

    [JsonPropertyName("component_name")]
    public required string ComponentName { get; set; }

    [JsonPropertyName("section_name")]
    public required string SectionName { get; set; }

    [JsonPropertyName("source_package_name")]
    public required string SourcePackageName { get; set; }

    [JsonPropertyName("source_package_version")]
    public required string SourcePackageVersion { get; set; }

    [JsonPropertyName("distro_arch_series_link")]
    public required Uri DistroArchSeriesLink { get; set; }

    [JsonPropertyName("phased_update_percentage")]
    public int? PhasedUpdatePercentage { get; set; }

    [JsonPropertyName("date_published")]
    public DateTimeOffset DatePublished { get; set; }

    [JsonPropertyName("scheduled_deletion_date")]
    public DateTimeOffset? ScheduledDeletionDate { get; set; }

    [JsonPropertyName("status")]
    [JsonConverter(typeof(JsonStringEnumConverter<ArchivePublishingStatus>))]
    public ArchivePublishingStatus Status { get; set; }

    [JsonPropertyName("pocket")]
    [JsonConverter(typeof(JsonStringEnumConverter<ArchivePocket>))]
    public ArchivePocket Pocket { get; set; }

    [JsonPropertyName("creator_link")]
    public Uri? CreatorLink { get; set; }

    [JsonPropertyName("date_created")]
    public DateTimeOffset DateCreated { get; set; }

    [JsonPropertyName("date_superseded")]
    public DateTimeOffset? DateSuperseded { get; set; }

    [JsonPropertyName("date_made_pending")]
    public DateTimeOffset? DateMadePending { get; set; }

    [JsonPropertyName("date_removed")]
    public DateTimeOffset? DateRemoved { get; set; }

    [JsonPropertyName("archive_link")]
    public required Uri ArchiveLink { get; set; }

    [JsonPropertyName("copied_from_archive_link")]
    public required Uri? CopiedFromArchiveLink { get; set; }

    [JsonPropertyName("removed_by_link")]
    public Uri? RemovedByLink { get; set; }

    [JsonPropertyName("removal_comment")]
    public string? RemovalComment { get; set; }

    [JsonPropertyName("binary_package_name")]
    public required string BinaryPackageName { get; set; }

    [JsonPropertyName("binary_package_version")]
    public required string BinaryPackageVersion { get; set; }

    [JsonPropertyName("build_link")]
    public required Uri BuildLink { get; set; }

    [JsonPropertyName("architecture_specific")]
    public bool ArchitectureSpecific { get; set; }

    [JsonPropertyName("priority_name")]
    public required string PriorityName { get; set; }

    [JsonPropertyName("is_debug")]
    public bool IsDebug { get; set; }

    public async Task<IEnumerable<BinaryPackageFile>> GetBinaryPackageFiles(
        GetBinaryPackageFilesOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= GetBinaryPackageFilesOptions.Empty;
        var response =
            await LaunchpadClient.GetAsync(LaunchpadClient.GetResourcePath(SelfLink), options, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new ApplicationException("Failed to get binary package files");
        }

        var result = await response.Content.ReadAsStringAsync(cancellationToken: cancellationToken);

        if (result is null)
        {
            throw new ApplicationException("Failed to get binary package files");
        }

        List<BinaryPackageFile>? binaryPackageFiles = null;
        using var doc = JsonDocument.Parse(result);
        if (doc.RootElement.ValueKind == JsonValueKind.Array && doc.RootElement.GetArrayLength() > 0)
        {
            var firstElement = doc.RootElement[0];
            switch (firstElement.ValueKind)
            {
                case JsonValueKind.String:
                    binaryPackageFiles = doc.RootElement.EnumerateArray()
                        .Select(url => new BinaryPackageFile { Url = new Uri(url.GetString()!) })
                            .ToList();
                    break;
                case JsonValueKind.Object:
                    var deserializedResult = JsonSerializer.Deserialize<List<BinaryPackageFile>>(result);
                    binaryPackageFiles = deserializedResult ??
                                         throw new ApplicationException("Failed to deserialize binary package files");
                    break;
            }
        }

        if (binaryPackageFiles is null)
        {
            throw new ApplicationException("Failed to get binary package files");
        }

        return binaryPackageFiles;
    }
}
