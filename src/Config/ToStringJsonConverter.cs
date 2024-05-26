using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UAlbion.Config;

public class ToStringJsonConverter<T> : JsonConverter<T>
{
    static readonly Lazy<MethodInfo> Parser = new(() => typeof(T).GetMethod("Parse", new[] { typeof(string) }));
    static readonly Lazy<MethodInfo> Serialise = new(() => typeof(T).GetMethod("Serialise"));

    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(typeToConvert);
        if (typeToConvert != typeof(T)) throw new InvalidOperationException($"Invoked ToStringJsonConverter<{typeof(T).Name}>.Read with typeToConvert = {typeToConvert.Name}");

        if (reader.TokenType == JsonTokenType.String)
        {
            var asString = reader.GetString();
            var parser = Parser.Value;
            if (parser != null && parser.IsStatic && parser.ReturnType == typeToConvert)
                return (T)parser.Invoke(null, new object[] { asString });

            if (typeToConvert.IsEnum && asString != null)
                return (T)Enum.Parse(typeToConvert, asString);
        }

        if (reader.TokenType == JsonTokenType.Number)
        {
            long longValue = reader.GetInt64();
            if (typeToConvert.IsEnum)
                return (T)Enum.Parse(typeToConvert, longValue.ToString());
        }

        throw new JsonException($"The {typeToConvert.Name} type does not have a public " +
                                $"static Parse(string) method that returns a {typeToConvert.Name}.");
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        if (value == null) throw new ArgumentNullException(nameof(value));
        var serialise = Serialise.Value;
        if (serialise != null && !serialise.IsStatic && serialise.ReturnType == typeof(string))
            writer.WriteStringValue((string)serialise.Invoke(value, Array.Empty<object>()));
        else
            writer.WriteStringValue(value.ToString());
    }
}
