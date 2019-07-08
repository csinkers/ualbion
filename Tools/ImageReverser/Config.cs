using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UAlbion.ImageReverser
{
    public class ConfigObjectConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(Config.ConfigObject).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var item = JObject.Load(reader);
            switch (item.Value<string>("Type"))
            {
                case "texture": return item.ToObject<Config.Texture>();
                case "interlaced_bitmap": return item.ToObject<Config.Texture>();
                case "palette": return item.ToObject<Config.Palette>();
                case "unknown": return item.ToObject<Config.ConfigObject>();
                default: throw new NotImplementedException();
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var o = JObject.FromObject(value);
            /*
            switch (value)
            {
                case Config.Texture _: o.AddFirst(new JProperty("Type", new JValue("texture"))); break;
                case Config.ConfigObject _: o.AddFirst(new JProperty("Type", new JValue("unknown"))); break;
                default: throw new NotImplementedException();
            }
            */
            o.WriteTo(writer);
        }
    }

    public class Config
    {
        public class ConfigObject
        {
            public string Name { get; set; }
            public string Type { get; set; }
        }

        public class Texture : ConfigObject
        {
            public int Width { get; set; }
            public int Offset { get; set; }
        }

        public class Palette : ConfigObject { }

        public class Xld
        {
            [JsonIgnore]
            public string Name;
            public IDictionary<int, ConfigObject> Objects { get; } = new Dictionary<int, ConfigObject>();
        }

        public string BaseXldPath { get; set; }
        public string ExportedXldPath { get; set; }

        public IDictionary<string, Xld> Xlds { get; } = new Dictionary<string, Xld>();
    }
}
