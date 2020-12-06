using System;
using System.Globalization;
using System.Reflection;
using Newtonsoft.Json;

namespace UAlbion.Config
{
    public class ToStringJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => true;
        public override bool CanRead => true;
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            if (value == null) throw new ArgumentNullException(nameof(value));
            MethodInfo parse = value.GetType().GetMethod("Serialise");
            if (parse != null && !parse.IsStatic && parse.ReturnType == typeof(string))
                writer.WriteValue(parse.Invoke(value, Array.Empty<object>()));
            else
                writer.WriteValue(value.ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            if (objectType == null) throw new ArgumentNullException(nameof(objectType));

            MethodInfo parse = objectType.GetMethod("Parse", new[] { typeof(string) });
            if (parse != null && parse.IsStatic && parse.ReturnType == objectType)
                return parse.Invoke(null, new object[] { (string)reader.Value });

            if (objectType.IsEnum && reader.Value is string strValue)
                return Enum.Parse(objectType, strValue);
            if (objectType.IsEnum && reader.Value is int intValue)
                return Enum.Parse(objectType, intValue.ToString(CultureInfo.InvariantCulture));
            if (objectType.IsEnum && reader.Value is long longValue)
                return Enum.Parse(objectType, longValue.ToString(CultureInfo.InvariantCulture));

            throw new JsonException($"The {objectType.Name} type does not have a public " +
                                    $"static Parse(string) method that returns a {objectType.Name}.");
        }
    }
}