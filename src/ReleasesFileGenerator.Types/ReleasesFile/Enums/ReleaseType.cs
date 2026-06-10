using System.Text.Json.Serialization;

namespace ReleasesFileGenerator.Types.ReleasesFile.Enums;

public enum ReleaseType
{
    [JsonStringEnumMemberName("sts")]
    Sts,

    [JsonStringEnumMemberName("lts")]
    Lts
}
