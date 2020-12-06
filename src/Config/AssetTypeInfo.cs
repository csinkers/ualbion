using System.Collections.Generic;
using Newtonsoft.Json;

namespace UAlbion.Config
{
    public class AssetTypeInfo
    {
        [JsonProperty(Order = 1)] public AssetType AssetType { get; set; }
        [JsonProperty(Order = 2)] public string EnumType { get; set; }
        [JsonProperty(Order = 3)] public string CopiedFrom { get; set; }
        [JsonProperty(Order = 4)] public string Loader { get; set; }
        [JsonProperty(Order = 5)] public string Locator { get; set; }
        [JsonProperty(Order = 6)] public IDictionary<string, AssetFileInfo> Files { get; } = new Dictionary<string, AssetFileInfo>();

        public void PostLoad()
        {
            foreach (var file in Files)
            {
                file.Value.Filename = file.Key;
                file.Value.EnumType = this;
                file.Value.PostLoad();
            }
        }

        public void PreSave()
        {
            foreach (var xld in Files)
            {
                if (xld.Value.Transposed == false)
                    xld.Value.Transposed = null;

                foreach (var asset in xld.Value.Assets)
                {
                    if (string.IsNullOrWhiteSpace(asset.Value.Name))
                        asset.Value.Name = null;
                }
            }
        }
    }
}
