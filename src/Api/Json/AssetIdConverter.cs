﻿using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UAlbion.Api.Json
{
    class AssetIdConverter<T> : JsonConverter<T> where T : struct, IAssetId
    {
        static readonly IAssetId.ParserDelegate<T> Parser = IAssetId.GetParser<T>();
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException($"Expected token type String when deserializing {typeof(T).Name}, but was {reader.TokenType}");
            return Parser(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) 
            => writer.WriteStringValue(value.ToString());
    }
}