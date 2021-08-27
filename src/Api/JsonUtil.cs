using System;
using System.Text.Json;
using UAlbion.Api.Json;

namespace UAlbion.Api
{
    public static class JsonUtil
    {
        public static readonly JsonSerializerOptions DefaultOptions = new(new JsonSerializerOptions
        {
            ReadCommentHandling = JsonCommentHandling.Skip,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true,
            Converters =
            {
                VersionConverter.Instance,
                new DictionaryEnumConverterFactory(),
                new DictionaryAssetIdConverterFactory(),
            }
        });

        public static T Deserialize<T>(ReadOnlySpan<byte> bytes) => bytes.Length == 0 ? default : JsonSerializer.Deserialize<T>(bytes, DefaultOptions);
        public static T Deserialize<T>(string json) => string.IsNullOrEmpty(json) ? default : JsonSerializer.Deserialize<T>(json, DefaultOptions);
        public static string Serialize<T>(T input) => JsonSerializer.Serialize(input, DefaultOptions);
    }
}