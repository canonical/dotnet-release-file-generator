using System.Text.Json.Serialization;
using ReleasesFileGenerator.Types.ReleasesFile.Enums;

namespace ReleasesFileGenerator.Console.Models;

public class AvailableVersionEntry
{
    [JsonPropertyName("channel-version")]
    public required string ChannelVersion { get; set; }

    [JsonPropertyName("product")]
    public required string Product { get; set; }

    [JsonPropertyName("support-phase")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ReleaseSupportPhase SupportPhase { get; set; }

    [JsonPropertyName("release-type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ReleaseType ReleaseType { get; set; }

    [JsonPropertyName("eol-date")]
    public DateOnly EolDate { get; set; }

    [JsonPropertyName("source-package-name")]
    public required string SourcePackageName { get; set; }

    [JsonPropertyName("runtime-binary-package-name")]
    public required string RuntimeBinaryPackageName { get; set; }

    [JsonPropertyName("aspnetcore-runtime-binary-package-name")]
    public required string AspNetCoreRuntimeBinaryPackageName { get; set; }

    [JsonPropertyName("sdk-binary-package-name")]
    public required string SdkBinaryPackageName { get; set; }

    [JsonPropertyName("archive")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required Archive Archive { get; set; }
}
