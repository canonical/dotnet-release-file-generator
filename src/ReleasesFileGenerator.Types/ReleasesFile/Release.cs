using System.Text.Json.Serialization;
using ReleasesFileGenerator.Types.Converters;

namespace ReleasesFileGenerator.Types.ReleasesFile;

public class Release
{
    [JsonPropertyName("release-date")]
    public DateOnly ReleaseDate { get; set; }

    [JsonPropertyName("release-version")]
    [JsonConverter(typeof(DotnetVersionJsonConverter))]
    public required DotnetVersion ReleaseVersion { get; set; }

    [JsonPropertyName("security")]
    public bool Security { get; set; }

    [JsonPropertyName("cve-list")]
    public IEnumerable<Cve>? CveList { get; set; }

    [JsonPropertyName("release-notes")]
    public Uri? ReleaseNotes { get; set; }
}
