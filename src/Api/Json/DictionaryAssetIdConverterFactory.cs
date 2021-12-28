﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UAlbion.Api.Json;

sealed class DictionaryAssetIdConverterFactory : JsonConverterFactory
{
    static readonly Type AssetType = typeof(IAssetId);
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType)
            return false;

        var generic = typeToConvert.GetGenericTypeDefinition();
        if (generic != typeof(Dictionary<,>) && generic != typeof(IDictionary<,>))
            return false;

        var keyType = typeToConvert.GetGenericArguments()[0];
        return AssetType.IsAssignableFrom(keyType);
    }

    public override JsonConverter CreateConverter(Type type, JsonSerializerOptions options)
    {
        Type keyType = type.GetGenericArguments()[0];
        Type valueType = type.GetGenericArguments()[1];

        bool isInterface = type.GetGenericTypeDefinition() == typeof(IDictionary<,>);
        var baseType = isInterface
            ? typeof(DictionaryAssetIdInterfaceConverter<,>)
            : typeof(DictionaryAssetIdConverter<,>);

        return (JsonConverter)Activator.CreateInstance(
            baseType.MakeGenericType(keyType, valueType),
            BindingFlags.Instance | BindingFlags.Public,
            null,
            new object[] { options },
            null);
    }
}