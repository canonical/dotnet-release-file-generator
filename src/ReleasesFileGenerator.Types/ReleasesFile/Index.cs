using System.Text.Json.Serialization;

namespace ReleasesFileGenerator.Types.ReleasesFile;

public class Index
{
    [JsonPropertyName("$schema")]
    public string Schema { get; } = "https://json.schemastore.org/dotnet-releases-index.json";

    [JsonPropertyName("releases-index")]
    public required IEnumerable<Channel> ReleasesIndex { get; set; }
}
