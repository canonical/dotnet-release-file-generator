using System.Text.Json.Serialization;

namespace ReleasesFileGenerator.Launchpad.Types;

public class BinaryPackageFile
{
    [JsonPropertyName("url")]
    public required Uri Url { get; set; }

    [JsonPropertyName("size")]
    public long? Size { get; set; }

    [JsonPropertyName("sha1")]
    public string? Sha1 { get; set; }

    [JsonPropertyName("sha256")]
    public string? Sha256 { get; set; }
}
