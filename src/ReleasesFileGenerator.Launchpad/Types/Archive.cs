using System.Net.Http.Json;
using System.Text.Json.Serialization;
using ReleasesFileGenerator.Launchpad.Models;
using ReleasesFileGenerator.Launchpad.Types.Enums;
using ReleasesFileGenerator.Launchpad.Types.Options.Archive;

namespace ReleasesFileGenerator.Launchpad.Types;

/// <summary>
/// Main Archive interface.
/// </summary>
public class Archive : LaunchpadEntryType
{
    /// <summary>
    /// The canonical human-addressable web link to this resource.
    /// </summary>
    [JsonPropertyName("web_link")]
    public required Uri WebLink { get; init; }

    /// <summary>
    /// The archive owner.
    /// </summary>
    [JsonPropertyName("owner_link")]
    public required Uri OwnerLink { get; set; }

    /// <summary>
    /// At least one lowercase letter or number, followed by letters, numbers, dots, hyphens or pluses.
    /// Keep this name short; it is used in URLs.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>
    /// A short title for the archive.
    /// </summary>
    [JsonPropertyName("displayname")]
    public required string DisplayName { get; set; }

    /// <summary>
    /// A string to uniquely identify the archive.
    /// </summary>
    [JsonPropertyName("reference")]
    public required string Reference { get; init; }

    /// <summary>
    /// The distribution that uses or is used by this archive.
    /// </summary>
    [JsonPropertyName("distribution_link")]
    public required Uri DistributionLink { get; set; }

    /// <summary>
    /// Restrict access to the archive to its owner and subscribers.
    /// This can only be changed if the archive has never had any sources published.
    /// </summary>
    [JsonPropertyName("private")]
    public bool Private { get; set; }

    /// <summary>
    /// Whether subscribers to private PPAs get emails about their subscriptions. Has no effect on a public PPA.
    /// </summary>
    [JsonPropertyName("suppress_subscription_notifications")]
    public bool SuppressSubscriptionNotifications { get; set; }

    /// <summary>
    /// Archive dependencies recorded for this archive.
    /// </summary>
    [JsonPropertyName("dependencies_collection_link")]
    public required Uri DependenciesCollectionLink { get; init; }

    /// <summary>
    /// A short description of the archive. URLs are allowed and will be rendered as links.
    /// </summary>
    [JsonPropertyName("description")]
    public required string Description { get; set; }

    /// <summary>
    /// A OpenPGP signing key fingerprint (40 chars) for this PPA or None if there is no signing key available.
    /// </summary>
    [JsonPropertyName("signing_key_fingerprint")]
    public string? SigningKeyFingerprint { get; set; }

    /// <summary>
    /// Only build the archive's packages on virtual builders.
    /// </summary>
    [JsonPropertyName("require_virtualized")]
    public bool RequireVirtualized { get; set; }

    /// <summary>
    /// Create debug symbol packages for builds in the archive.
    /// </summary>
    [JsonPropertyName("build_debug_symbols")]
    public bool BuildDebugSymbols { get; set; }

    /// <summary>
    /// Publish debug symbol packages in the apt repository.
    /// </summary>
    [JsonPropertyName("publish_debug_symbols")]
    public bool PublishDebugSymbols { get; set; }

    /// <summary>
    /// Allow uploads targeted to obsolete series.
    /// </summary>
    [JsonPropertyName("permit_obsolete_series_uploads")]
    public bool PermitObsoleteSeriesUploads { get; set; }

    /// <summary>
    /// Maximum size, in MiB, allowed for the archive.
    /// </summary>
    [JsonPropertyName("authorized_size")]
    public long? AuthorizedSize { get; set; }

    /// <summary>
    /// Status of archive.
    /// </summary>
    [JsonPropertyName("status")]
    public required string Status { get; init; }

    /// <summary>
    /// Newline-separated list of repositories to be used to retrieve any external build-dependencies when building
    /// packages in the archive, in the format: <c>deb http[s]://[user:pass@]&lt;host&gt;[/path] %(series)s[-pocket]
    /// [components]</c>. The <c>series</c> variable is replaced with the series name of the context build.
    /// <br/>
    /// NOTE: This is for migration of OEM PPAs only!
    /// </summary>
    [JsonPropertyName("external_dependencies")]
    public string? ExternalDependencies { get; set; }

    /// <summary>
    /// A JSON object containing metadata overrides for this archive.
    /// <br/>
    /// Accepted keys are <c>Origin</c>, <c>Label</c>, <c>Suite</c> and <c>Snapshots</c>.The values for all these keys
    /// should be a string and can use a <c>{series}</c> placeholder, which will get substituted by the name of the
    /// series that is currently being published.
    /// </summary>
    [JsonPropertyName("metadata_overrides")]
    public string? MetadataOverrides { get; set; }

    /// <summary>
    /// The architectures on which the archive can build.
    /// </summary>
    [JsonPropertyName("processors_collection_link")]
    public required Uri ProcessorsCollectionLink { get; init; }

    /// <summary>
    /// Publishing method.
    /// </summary>
    [JsonPropertyName("publishing_method")]
    [JsonConverter(typeof(JsonStringEnumConverter<PublishingMethod>))]
    public required PublishingMethod PublishingMethod { get; set; }

    /// <summary>
    /// Repository format.
    /// </summary>
    [JsonPropertyName("repository_format")]
    [JsonConverter(typeof(JsonStringEnumConverter<RepositoryFormat>))]
    public required RepositoryFormat RepositoryFormat { get; set; }

    /// <summary>
    /// Whether to update the apt repository. If disabled, nothing will be published. If the archive is private
    /// then additionally no builds will be dispatched.
    /// </summary>
    [JsonPropertyName("publish")]
    public bool Publish { get; set; }

    /// <summary>
    /// A delta to apply to all build scores for the archive. Builds with a higher score will build sooner.
    /// </summary>
    [JsonPropertyName("relative_build_score")]
    public int RelativeBuildScore { get; set; }

    /// <summary>
    /// Get the <see cref="Distribution">Distribution</see> for this archive.
    /// </summary>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>
    /// A <see cref="Distribution">Distribution</see> object representing the distribution for this archive.
    /// </returns>
    /// <exception cref="ApplicationException">
    /// When either the request or deserialization fails.
    /// </exception>
    public async Task<Distribution> GetDistributionAsync(
        CancellationToken cancellationToken = default)
    {
        var response = await LaunchpadClient.GetAsync(LaunchpadClient.GetResourcePath(DistributionLink),
            cancellationToken: cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new ApplicationException("Failed to get distribution for archive");
        }

        var result = await response.Content.ReadFromJsonAsync<Distribution>(cancellationToken: cancellationToken);

        if (result is null)
        {
            throw new ApplicationException("Failed to get distribution for archive");
        }

        return result;
    }

    /// <summary>
    /// All <see cref="BinaryPackagePublishingHistory">BinaryPackagePublishingHistory</see> target to this archive.
    /// </summary>
    /// <param name="options">The request parameters.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>
    /// A <see cref="LaunchpadCollectionResponse{BinaryPackagePublishingHistory}"/> containing the
    /// <see cref="BinaryPackagePublishingHistory">BinaryPackagePublishingHistory</see> objects
    /// that have been published to this archive.
    /// </returns>
    /// <exception cref="ApplicationException">When either the request or deserialization fails.</exception>
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

    /// <summary>
    /// All <see cref="SourcePackagePublishingHistory">SourcePackagePublishingHistory</see> target to this archive.
    /// </summary>
    /// <param name="options">The request parameters.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>
    /// A <see cref="LaunchpadCollectionResponse{SourcePackagePublishingHistory}"/> containing the
    /// <see cref="SourcePackagePublishingHistory">SourcePackagePublishingHistory</see> objects
    /// that have been published to this archive.
    /// </returns>
    /// <exception cref="ApplicationException">When either the request or deserialization fails.</exception>
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
