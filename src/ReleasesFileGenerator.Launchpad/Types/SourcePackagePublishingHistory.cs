using System.Text.Json.Serialization;
using ReleasesFileGenerator.Launchpad.Models;
using ReleasesFileGenerator.Launchpad.Types.Enums;

namespace ReleasesFileGenerator.Launchpad.Types;

public class SourcePackagePublishingHistory : LaunchpadEntryType
{
    [JsonPropertyName("display_name")]
    public required string DisplayName { get; set; }

    [JsonPropertyName("component_name")]
    public required string ComponentName { get; set; }

    [JsonPropertyName("section_name")]
    public required string SectionName { get; set; }

    [JsonPropertyName("status")]
    [JsonConverter(typeof(JsonStringEnumConverter<ArchivePublishingStatus>))]
    public ArchivePublishingStatus Status { get; set; }

    [JsonPropertyName("distro_series_link")]
    public required Uri DistroSeriesLink { get; set; }

    [JsonPropertyName("date_published")]
    public DateTimeOffset DatePublished { get; set; }

    [JsonPropertyName("scheduled_deletion_date")]
    public DateTimeOffset? ScheduledDeletionDate { get; set; }

    [JsonPropertyName("pocket")]
    [JsonConverter(typeof(JsonStringEnumConverter<ArchivePocket>))]
    public ArchivePocket Pocket { get; set; }

    [JsonPropertyName("archive_link")]
    public required Uri ArchiveLink { get; set; }

    [JsonPropertyName("copied_from_archive_link")]
    public required Uri? CopiedFromArchiveLink { get; set; }

    [JsonPropertyName("date_superseded")]
    public DateTimeOffset? DateSuperseded { get; set; }

    [JsonPropertyName("date_created")]
    public DateTimeOffset DateCreated { get; set; }

    [JsonPropertyName("date_made_pending")]
    public DateTimeOffset? DateMadePending { get; set; }

    [JsonPropertyName("date_removed")]
    public DateTimeOffset? DateRemoved { get; set; }

    [JsonPropertyName("removed_by_link")]
    public Uri? RemovedByLink { get; set; }

    [JsonPropertyName("removal_comment")]
    public string? RemovalComment { get; set; }

    [JsonPropertyName("source_package_name")]
    public required string SourcePackageName { get; set; }

    [JsonPropertyName("source_package_version")]
    public required string SourcePackageVersion { get; set; }

    [JsonPropertyName("package_creator_link")]
    public required Uri PackageCreatorLink { get; set; }

    [JsonPropertyName("package_maintainer_link")]
    public required Uri PackageMaintainerLink { get; set; }

    [JsonPropertyName("package_signer_link")]
    public required Uri PackageSignerLink { get; set; }

    [JsonPropertyName("creator_link")]
    public Uri? CreatorLink { get; set; }

    [JsonPropertyName("sponsor_link")]
    public Uri? SponsorLink { get; set; }

    [JsonPropertyName("packageupload_link")]
    public required Uri PackageUploadLink { get; set; }
}
