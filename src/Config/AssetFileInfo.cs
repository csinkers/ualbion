using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UAlbion.Config
{
    public class AssetFileInfo
    {
        string _loader;
        [JsonIgnore] public AssetTypeInfo EnumType { get; set; } // i.e. parent
        [JsonIgnore] public string Filename { get; set; } // Just mirrors the dictionary key
        [JsonConverter(typeof(StringEnumConverter))]
        public ContainerFormat ContainerFormat { get; set; }
        public string Loader { get => _loader ?? EnumType.Loader; set => _loader = value; }
        public string Format { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public bool? Transposed { get; set; }
        public string Sha256Hash { get; set; } // Currently only used for MAIN.EXE
        public IDictionary<int, AssetInfo> Assets { get; } = new Dictionary<int, AssetInfo>();

        // TODO: Text encoding
        public void PostLoad()
        {
            int lastKey = 0;
            int lastId = 0;
            foreach (var o in Assets)
            {
                o.Value.File = this;
                o.Value.SubAssetId = o.Key;
                if (o.Value.Id == 0)
                    o.Value.Id = o.Key - lastKey + lastId;

                lastKey = o.Key;
                lastId = o.Value.Id;
            }
        }
    }
}
