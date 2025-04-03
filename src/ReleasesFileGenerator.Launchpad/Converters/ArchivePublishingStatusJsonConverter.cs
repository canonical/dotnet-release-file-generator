using System.Text.Json;
using System.Text.Json.Serialization;
using ReleasesFileGenerator.Launchpad.Types.Enums;

namespace ReleasesFileGenerator.Launchpad.Converters;

public class ArchivePublishingStatusJsonConverter : JsonConverter<ArchivePublishingStatus>
{
    public override ArchivePublishingStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var archivePublishingStatusString = reader.GetString();

        if (string.IsNullOrEmpty(archivePublishingStatusString))
        {
            throw new JsonException("Archive publishing status string is null or empty.");
        }

        if (Enum.TryParse<ArchivePublishingStatus>(
                archivePublishingStatusString, ignoreCase: true, out var archivePublishingStatus))
        {
            return archivePublishingStatus;
        }

        throw new JsonException("Archive publishing status string is invalid.");
    }

    public override void Write(Utf8JsonWriter writer, ArchivePublishingStatus value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(Enum.GetName(value));
    }
}
