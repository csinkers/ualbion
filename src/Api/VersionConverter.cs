using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UAlbion.Api
{
    public class VersionConverter : JsonConverter<Version>
    {
        public static VersionConverter Instance { get; } = new();
        VersionConverter() { }
        public override Version Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var asString = reader.GetString();
            return asString == null ? null : new Version(asString);
        }

        public override void Write(Utf8JsonWriter writer, Version value, JsonSerializerOptions options)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            if (value == null)
                writer.WriteNullValue();
            else
                writer.WriteStringValue(value.ToString());
        }
    }
}