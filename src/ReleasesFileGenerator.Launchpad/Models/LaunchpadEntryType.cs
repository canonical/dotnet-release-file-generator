using System.Text.Json.Serialization;

namespace ReleasesFileGenerator.Launchpad.Models;

public abstract class LaunchpadEntryType
{
    [JsonPropertyName("self_link")]
    public required Uri SelfLink { get; set; }

    [JsonPropertyName("resource_type_link")]
    public required Uri ResourceTypeLink { get; set; }

    [JsonPropertyName("http_etag")]
    public required string HttpEtag { get; set; }
}
