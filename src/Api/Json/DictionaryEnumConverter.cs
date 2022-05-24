using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UAlbion.Api.Json;
#pragma warning disable CA1812 // Internal class that is apparently never instantiated; this class is instantiated generically
class DictionaryEnumConverter<TKey, TValue> : JsonConverter<Dictionary<TKey, TValue>> where TKey : struct, Enum
{
    static readonly Type KeyType = typeof(TKey);
    static readonly Type ValueType = typeof(TValue);
    readonly JsonConverter<TValue> _valueConverter;

    public DictionaryEnumConverter(JsonSerializerOptions options) 
        => _valueConverter = (JsonConverter<TValue>)options.GetConverter(typeof(TValue));

    static TKey Parser(string propertyName)
    {
        // For performance, parse with ignoreCase:false first.
        if (Enum.TryParse(propertyName, false, out TKey key))
            return key;

        if (Enum.TryParse(propertyName, true, out key))
            return key;

        throw new JsonException($"Unable to convert \"{propertyName}\" to Enum \"{KeyType}\".");
    }

    public override Dictionary<TKey, TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => 
        DictionaryConverterUtil<TKey, TValue>.Read(ref reader, typeToConvert, options, _valueConverter, ValueType, Parser);

    public override void Write(Utf8JsonWriter writer, Dictionary<TKey, TValue> value, JsonSerializerOptions options) => 
        DictionaryConverterUtil<TKey, TValue>.Write(writer, value, options, _valueConverter);
}
#pragma warning restore CA1812 // Internal class that is apparently never instantiated; this class is instantiated generically