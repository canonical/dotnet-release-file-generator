using System.Text.Json;
using System.Text.Json.Serialization;

namespace ReleasesFileGenerator.Launchpad.Converters;

public class UriJsonConverter : JsonConverter<Uri>
{
    public override Uri? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var uriString = reader.GetString();

        if (uriString is null) return null;

        return Uri.TryCreate(uriString, UriKind.Absolute, out var uri) ? uri : null;
    }

    public override void Write(Utf8JsonWriter writer, Uri value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
