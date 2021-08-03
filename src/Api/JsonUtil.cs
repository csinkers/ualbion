using System;
using System.Text.Json;

namespace UAlbion.Api
{
    public static class JsonUtil
    {
        public static readonly JsonSerializerOptions DefaultOptions = new(new JsonSerializerOptions
        {
            ReadCommentHandling = JsonCommentHandling.Skip,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true,
            Converters = { VersionConverter.Instance }
        });

        public static T Deserialize<T>(ReadOnlySpan<byte> bytes) => bytes.Length == 0 ? default : JsonSerializer.Deserialize<T>(bytes, DefaultOptions);
        public static string Serialize<T>(T input) => JsonSerializer.Serialize(input, DefaultOptions);
    }
}