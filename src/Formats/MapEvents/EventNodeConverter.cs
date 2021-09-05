using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UAlbion.Formats.MapEvents
{
    public class EventNodeConverter : JsonConverter<IEventNode>
    {
        EventNodeConverter() {}
        public static readonly EventNodeConverter Instance = new();
        public override IEventNode? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException($"Tried to deserialize a token of type {reader.TokenType} as an IEventNode, expected String");

            return EventNode.Parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, IEventNode value, JsonSerializerOptions options)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            if (value == null) throw new ArgumentNullException(nameof(value));
            writer.WriteStringValue(value.ToString());
        }
    }
}