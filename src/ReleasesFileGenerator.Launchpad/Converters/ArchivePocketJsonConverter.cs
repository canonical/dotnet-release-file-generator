using System.Text.Json;
using System.Text.Json.Serialization;
using ReleasesFileGenerator.Launchpad.Types.Enums;

namespace ReleasesFileGenerator.Launchpad.Converters;

public class ArchivePocketJsonConverter : JsonConverter<ArchivePocket>
{
    public override ArchivePocket Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var archivePocketString = reader.GetString();

        if (string.IsNullOrEmpty(archivePocketString))
        {
            throw new JsonException("Archive pocket string is null or empty.");
        }

        if (Enum.TryParse<ArchivePocket>(archivePocketString, ignoreCase: true, out var archivePocket))
        {
            return archivePocket;
        }

        throw new JsonException("Archive pocket string is invalid.");
    }

    public override void Write(Utf8JsonWriter writer, ArchivePocket value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(Enum.GetName(value));
    }
}
