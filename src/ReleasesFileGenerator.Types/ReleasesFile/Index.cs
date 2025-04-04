using System.Text.Json.Serialization;

namespace ReleasesFileGenerator.Types.ReleasesFile;

public class Index
{
    [JsonPropertyName("releases-index")]
    public required IEnumerable<Channel> ReleasesIndex { get; set; }
}
