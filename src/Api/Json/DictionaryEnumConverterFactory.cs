using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UAlbion.Api.Json
{
    sealed class DictionaryEnumConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            if (!typeToConvert.IsGenericType)
                return false;

            var generic = typeToConvert.GetGenericTypeDefinition();
            if (generic != typeof(Dictionary<,>) && generic != typeof(IDictionary<,>))
                return false;

            return typeToConvert.GetGenericArguments()[0].IsEnum;
        }

        public override JsonConverter CreateConverter(Type type, JsonSerializerOptions options)
        {
            Type keyType = type.GetGenericArguments()[0];
            Type valueType = type.GetGenericArguments()[1];

            bool isInterface = type.GetGenericTypeDefinition() == typeof(IDictionary<,>);
            var baseType = isInterface
                ? typeof(DictionaryEnumInterfaceConverter<,>)
                : typeof(DictionaryEnumConverter<,>);

            return (JsonConverter)Activator.CreateInstance(
                baseType.MakeGenericType(keyType, valueType),
                BindingFlags.Instance | BindingFlags.Public,
                null,
                new object[] { options },
                null);
        }
    }
}