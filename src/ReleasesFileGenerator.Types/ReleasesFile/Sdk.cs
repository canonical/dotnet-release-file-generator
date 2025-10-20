using System.Text.Json.Serialization;
using ReleasesFileGenerator.Types.Converters;

namespace ReleasesFileGenerator.Types.ReleasesFile;

public class Sdk
{
    [JsonPropertyName("version")]
    [JsonConverter(typeof(DotnetVersionJsonConverter))]
    public required DotnetVersion Version { get; set; }

    [JsonPropertyName("version-display")]
    public required string VersionDisplay { get; set; }

    [JsonPropertyName("runtime-version")]
    [JsonConverter(typeof(DotnetVersionJsonConverter))]
    public required DotnetVersion RuntimeVersion { get; set; }
}
