using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UAlbion.Api.Json
{
#pragma warning disable CA1812 // Internal class that is apparently never instantiated; this class is instantiated generically
    class DictionaryAssetIdConverter<TKey, TValue> : JsonConverter<Dictionary<TKey, TValue>> where TKey : struct, IAssetId
    {
        static readonly Type _valueType = typeof(TValue);
        static readonly IAssetId.ParserDelegate<TKey> _parser = IAssetId.GetParser<TKey>();
        readonly JsonConverter<TValue> _valueConverter;

        public DictionaryAssetIdConverter(JsonSerializerOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            _valueConverter = (JsonConverter<TValue>)options.GetConverter(typeof(TValue));
        }

        public override Dictionary<TKey, TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            DictionaryConverterUtil<TKey, TValue>.Read(ref reader, typeToConvert, options, _valueConverter, _valueType, _parser);

        public override void Write(Utf8JsonWriter writer, Dictionary<TKey, TValue> value, JsonSerializerOptions options) => 
            DictionaryConverterUtil<TKey, TValue>.Write(writer, value, options, _valueConverter);
    }
#pragma warning restore CA1812 // Internal class that is apparently never instantiated; this class is instantiated generically
}