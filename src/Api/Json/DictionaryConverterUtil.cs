using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using UAlbion.Api.Visual;

namespace UAlbion.Api.Json
{
    static class DictionaryConverterUtil<TKey, TValue>
    {
        public static Dictionary<TKey, TValue> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options,
            JsonConverter<TValue> valueConverter,
            Type valueType,
            IAssetId.ParserDelegate<TKey> parser)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException($"Expected '{{' when reading {typeToConvert}, but was {reader.TokenType}");

            var value = new Dictionary<TKey, TValue>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    return value;

                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException($"Expected property name when reading {typeToConvert}, but was {reader.TokenType}");

                var key = parser(reader.GetString());

                TValue v;
                if (valueConverter != null)
                {
                    reader.Read();
                    v = valueConverter.Read(ref reader, valueType, options);
                }
                else
                {
                    v = JsonSerializer.Deserialize<TValue>(ref reader, options);
                }

                value.Add(key, v);
            }

            throw new JsonException($"Expected '}}' when reading {typeToConvert}, but all input has been consumed");
        }

        public static void Write(Utf8JsonWriter writer, IDictionary<TKey, TValue> value, JsonSerializerOptions options, JsonConverter<TValue> valueConverter)
        {
            writer.WriteStartObject();

            foreach (var kvp in value)
            {
                writer.WritePropertyName(kvp.Key.ToString() ?? string.Empty);

                if (valueConverter != null)
                    valueConverter.Write(writer, kvp.Value, options);
                else
                    JsonSerializer.Serialize(writer, kvp.Value, options);
            }

            writer.WriteEndObject();
        }
    }
}