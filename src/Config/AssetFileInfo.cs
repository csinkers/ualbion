using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UAlbion.Config
{
    public class AssetFileInfo
    {
        [JsonIgnore] public string Name { get; set; } // Just mirrors the dictionary key
        [JsonConverter(typeof(StringEnumConverter))]
        public ContainerFormat ContainerFormat { get; set; }
        public FileFormat Format { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public bool? Transposed { get; set; }
        public IDictionary<int, AssetInfo> Assets { get; } = new Dictionary<int, AssetInfo>();
        // TODO: Text encoding
        public void PostLoad()
        {
            int lastKey = 0;
            int lastId = 0;
            foreach (var o in Assets)
            {
                o.Value.Parent = this;
                if (o.Value.Id == 0)
                    o.Value.Id = o.Key - lastKey + lastId;

                lastKey = o.Key;
                lastId = o.Value.Id;
                o.Value.PaletteHints ??= new List<int>();
            }
        }
    }
}
