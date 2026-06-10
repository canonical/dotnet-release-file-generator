using System.Text.Json.Serialization;

namespace ReleasesFileGenerator.Types.ReleasesFile.Enums;

public enum ReleaseSupportPhase
{
    [JsonStringEnumMemberName("preview")]
    Preview,

    [JsonStringEnumMemberName("go-live")]
    GoLive,

    [JsonStringEnumMemberName("active")]
    Active,

    [JsonStringEnumMemberName("maintenance")]
    Maintenance,

    [JsonStringEnumMemberName("eol")]
    Eol
}
