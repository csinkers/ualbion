using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UAlbion.Api.Json
{
    sealed class AssetIdConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert) => typeToConvert.IsAssignableTo(typeof(IAssetId));

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
            (JsonConverter)Activator.CreateInstance(
                typeof(AssetIdConverter<>).MakeGenericType(typeToConvert),
                BindingFlags.Instance | BindingFlags.Public,
                null,
                null,
                null);
    }
}