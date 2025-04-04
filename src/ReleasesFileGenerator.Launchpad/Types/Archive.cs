using System.Net.Http.Json;
using System.Text.Json.Serialization;
using ReleasesFileGenerator.Launchpad.Models;
using ReleasesFileGenerator.Launchpad.Types.Enums;
using ReleasesFileGenerator.Launchpad.Types.Options.Archive;

namespace ReleasesFileGenerator.Launchpad.Types;

public class Archive : LaunchpadEntryType
{
    [JsonPropertyName("web_link")]
    public required Uri WebLink { get; set; }

    [JsonPropertyName("owner_link")]
    public required Uri OwnerLink { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("displayname")]
    public required string DisplayName { get; set; }

    [JsonPropertyName("reference")]
    public required string Reference { get; set; }

    [JsonPropertyName("distribution_link")]
    public required Uri DistributionLink { get; set; }

    [JsonPropertyName("private")]
    public bool Private { get; set; }

    [JsonPropertyName("suppress_subscription_notifications")]
    public bool SuppressSubscriptionNotifications { get; set; }

    [JsonPropertyName("dependencies_collection_link")]
    public required Uri DependenciesCollectionLink { get; set; }

    [JsonPropertyName("description")]
    public required string Description { get; set; }

    [JsonPropertyName("signing_key_fingerprint")]
    public string? SigningKeyFingerprint { get; set; }

    [JsonPropertyName("require_virtualized")]
    public bool RequireVirtualized { get; set; }

    [JsonPropertyName("build_debug_symbols")]
    public bool BuildDebugSymbols { get; set; }

    [JsonPropertyName("publish_debug_symbols")]
    public bool PublishDebugSymbols { get; set; }

    [JsonPropertyName("permit_obsolete_series_uploads")]
    public bool PermitObsoleteSeriesUploads { get; set; }

    [JsonPropertyName("authorized_size")]
    public long? AuthorizedSize { get; set; }

    [JsonPropertyName("status")]
    public required string Status { get; set; }

    [JsonPropertyName("external_dependencies")]
    public string? ExternalDependencies { get; set; }

    [JsonPropertyName("metadata_overrides")]
    public string? MetadataOverrides { get; set; }

    [JsonPropertyName("processors_collection_link")]
    public required Uri ProcessorsCollectionLink { get; set; }

    [JsonPropertyName("enabled_restricted_processors_collection_link")]
    public required Uri EnabledRestrictedProcessorsCollectionLink { get; set; }

    [JsonPropertyName("publishing_method")]
    [JsonConverter(typeof(JsonStringEnumConverter<PublishingMethod>))]
    public required PublishingMethod PublishingMethod { get; set; }

    [JsonPropertyName("repository_format")]
    [JsonConverter(typeof(JsonStringEnumConverter<RepositoryFormat>))]
    public required RepositoryFormat RepositoryFormat { get; set; }

    [JsonPropertyName("publish")]
    public bool Publish { get; set; }

    [JsonPropertyName("relative_build_score")]
    public int RelativeBuildScore { get; set; }

    public async Task<LaunchpadCollectionResponse<BinaryPackagePublishingHistory>> GetPublishedBinariesAsync(
        GetPublishedBinariesOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= GetPublishedBinariesOptions.Empty;
        var response =
            await LaunchpadClient.GetAsync(LaunchpadClient.GetResourcePath(SelfLink), options, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new ApplicationException("Failed to get binary package publishing history");
        }

        var result = await response.Content
            .ReadFromJsonAsync<LaunchpadCollectionResponse<BinaryPackagePublishingHistory>>(
                cancellationToken: cancellationToken);

        if (result is null)
        {
            throw new ApplicationException("Failed to get binary package publishing history");
        }

        return result;
    }

    public async Task<LaunchpadCollectionResponse<SourcePackagePublishingHistory>> GetPublishedSourcesAsync(
        GetPublishedSourcesOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= GetPublishedSourcesOptions.Empty;
        var response =
            await LaunchpadClient.GetAsync(LaunchpadClient.GetResourcePath(SelfLink), options, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new ApplicationException("Failed to get source package publishing history");
        }

        var result = await response.Content
            .ReadFromJsonAsync<LaunchpadCollectionResponse<SourcePackagePublishingHistory>>(
                cancellationToken: cancellationToken);

        if (result is null)
        {
            throw new ApplicationException("Failed to get source publishing history");
        }

        return result;
    }
}
