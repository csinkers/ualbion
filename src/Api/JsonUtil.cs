using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using UAlbion.Api.Json;

namespace UAlbion.Api
{
    public class JsonUtil : IJsonUtil
    {
        readonly JsonSerializerOptions _defaultOptions = new(new JsonSerializerOptions
        {
            ReadCommentHandling = JsonCommentHandling.Skip,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            WriteIndented = true,
            Converters =
            {
                VersionConverter.Instance,
                new JsonStringEnumConverter(),
                new DictionaryEnumConverterFactory(),
                new DictionaryAssetIdConverterFactory(),
                new AssetIdConverterFactory()
            }
        });

        public JsonUtil(params JsonConverter[] extraConverters)
        {
            if (extraConverters == null)
                return;

            foreach (var converter in extraConverters)
                _defaultOptions.Converters.Add(converter);
        }

        public T Deserialize<T>(ReadOnlySpan<byte> bytes) => bytes.Length == 0 ? default : JsonSerializer.Deserialize<T>(bytes, _defaultOptions);
        public T Deserialize<T>(string json) => string.IsNullOrEmpty(json) ? default : JsonSerializer.Deserialize<T>(json, _defaultOptions);
        public string Serialize<T>(T input) => JsonSerializer.Serialize(input, _defaultOptions);
    }
}