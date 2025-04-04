using System.Text.Json;
using System.Text.Json.Serialization;

namespace ReleasesFileGenerator.Types.Converters;

public class DotnetVersionJsonConverter : JsonConverter<DotnetVersion>
{
    public override DotnetVersion? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var stringValue = reader.GetString();

        return string.IsNullOrWhiteSpace(stringValue) ? null : DotnetVersion.Parse(stringValue);
    }

    public override void Write(Utf8JsonWriter writer, DotnetVersion value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
