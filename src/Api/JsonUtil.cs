using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using UAlbion.Api.Json;

namespace UAlbion.Api;

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

    public T Deserialize<T>(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length == 0)
            return default;

        var result = JsonSerializer.Deserialize<T>(bytes, _defaultOptions);
        if (result is IJsonPostDeserialise post)
            post.OnDeserialized();
        return result;
    }

    public T Deserialize<T>(string json)
    {
        if (string.IsNullOrEmpty(json))
            return default;

        var result = JsonSerializer.Deserialize<T>(json, _defaultOptions);
        if (result is IJsonPostDeserialise post)
            post.OnDeserialized();
        return result;
    }

    public string Serialize<T>(T input) => JsonSerializer.Serialize(input, _defaultOptions);
}