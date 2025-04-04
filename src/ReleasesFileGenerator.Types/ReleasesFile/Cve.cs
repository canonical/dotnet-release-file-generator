using System.Text.Json.Serialization;

namespace ReleasesFileGenerator.Types.ReleasesFile;

public class Cve
{
    [JsonPropertyName("cve-id")]
    public required string Id { get; set; }

    [JsonPropertyName("cve-url")]
    public required Uri Url { get; set; }
}
