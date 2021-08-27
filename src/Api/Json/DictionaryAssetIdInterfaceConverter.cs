using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using UAlbion.Api.Visual;

namespace UAlbion.Api.Json
{
#pragma warning disable CA1812 // Internal class that is apparently never instantiated; this class is instantiated generically
    class DictionaryAssetIdInterfaceConverter<TKey, TValue> : JsonConverter<IDictionary<TKey, TValue>> where TKey : struct, IAssetId
    {
        readonly JsonConverter<TValue> _valueConverter;
        readonly Type _valueType = typeof(TValue);
        readonly IAssetId.ParserDelegate<TKey> _parser = IAssetId.GetParser<TKey>();

        public DictionaryAssetIdInterfaceConverter(JsonSerializerOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            _valueConverter = (JsonConverter<TValue>)options.GetConverter(typeof(TValue));
        }

        public override IDictionary<TKey, TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            DictionaryConverterUtil<TKey, TValue>.Read(ref reader, typeToConvert, options, _valueConverter, _valueType, _parser);

        public override void Write(Utf8JsonWriter writer, IDictionary<TKey, TValue> value, JsonSerializerOptions options) => 
            DictionaryConverterUtil<TKey, TValue>.Write(writer, value, options, _valueConverter);
    }
#pragma warning restore CA1812 // Internal class that is apparently never instantiated; this class is instantiated generically
}