using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UAlbion.Formats.Assets.Maps
{
    class NpcWaypointConverter : JsonConverter<NpcWaypoint>
    {
        NpcWaypointConverter() {}
        public static readonly NpcWaypointConverter Instance = new();
        public override NpcWaypoint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException($"Expected token type String when deserializing NpcWaypoint, but was {reader.TokenType}");
            return NpcWaypoint.Parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, NpcWaypoint value, JsonSerializerOptions options) 
            => writer.WriteStringValue(value.ToString());
    }
}