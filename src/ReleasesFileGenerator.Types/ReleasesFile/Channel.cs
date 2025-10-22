using System.Text.Json.Serialization;
using ReleasesFileGenerator.Types.Converters;
using ReleasesFileGenerator.Types.ReleasesFile.Enums;

namespace ReleasesFileGenerator.Types.ReleasesFile;

public class Channel
{
    [JsonPropertyName("channel-version")]
    public required string ChannelVersion { get; set; }

    [JsonPropertyName("latest-release")]
    [JsonConverter(typeof(DotnetVersionJsonConverter))]
    public required DotnetVersion LatestRelease { get; set; }

    [JsonPropertyName("latest-release-date")]
    public DateOnly LatestReleaseDate { get; set; }

    [JsonPropertyName("security")]
    public bool Security { get; set; }

    [JsonPropertyName("latest-runtime")]
    [JsonConverter(typeof(DotnetVersionJsonConverter))]
    public required DotnetVersion LatestRuntime { get; set; }

    [JsonPropertyName("latest-sdk")]
    [JsonConverter(typeof(DotnetVersionJsonConverter))]
    public required DotnetVersion LatestSdk { get; set; }

    [JsonPropertyName("product")]
    public required string Product { get; set; }

    [JsonPropertyName("support-phase")]
    [JsonConverter(typeof(JsonStringEnumConverter<ReleaseSupportPhase>))]
    public ReleaseSupportPhase SupportPhase { get; set; }

    [JsonPropertyName("eol-date")]
    public DateOnly? EolDate { get; set; }

    [JsonPropertyName("release-type")]
    [JsonConverter(typeof(JsonStringEnumConverter<ReleaseType>))]
    public ReleaseType ReleaseType { get; set; }

    [JsonPropertyName("releases.json")]
    public required Uri ReleasesJsonUrl { get; set; }

    [JsonPropertyName("supported-os.json")]
    public Uri? SupportedOsJsonUrl { get; set; }
}
