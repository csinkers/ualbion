using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace UAlbion.Formats.Config
{
    public class BasicAssetConfig
    {
        public const string Filename = "assets_min.json";
        public IDictionary<string, BasicXldInfo> Xlds { get; } = new Dictionary<string, BasicXldInfo>();

        public static BasicAssetConfig Extract(FullAssetConfig full)
        {
            var min = new BasicAssetConfig();

            foreach (var kvp in full.Xlds)
            {
                var old = kvp.Value;
                var newXld = new BasicXldInfo(old);

                foreach(var asset in old.Assets.Values)
                {
                    var newAsset = new BasicAssetInfo(asset) { Parent = newXld };
                    if (newAsset.ContainsData)
                        newXld.Assets[asset.Id] = newAsset;
                }

                min.Xlds[kvp.Key] = newXld;
            }

            return min;
        }

        public static BasicAssetConfig Load(string basePath)
        {
            var configPath = Path.Combine(basePath, "data", Filename);
            BasicAssetConfig config = new BasicAssetConfig();
            if (File.Exists(configPath))
            {
                var configText = File.ReadAllText(configPath);
                var xlds = JsonConvert.DeserializeObject<Dictionary<string, BasicXldInfo>>(configText);

                foreach (var xld in xlds)
                {
                    xld.Value.Name = xld.Key;
                    foreach(var o in xld.Value.Assets)
                    {
                        o.Value.Parent = xld.Value;
                        o.Value.Id = o.Key;
                    }

                    config.Xlds[xld.Key] = xld.Value;
                }
            }

            return config;
        }

        public void Save(string basePath)
        {
            var configPath = Path.Combine(basePath, "data", Filename);
            var serializerSettings = new JsonSerializerSettings { Formatting = Formatting.Indented, NullValueHandling = NullValueHandling.Ignore };
            var json = JsonConvert.SerializeObject(this, serializerSettings);
            File.WriteAllText(configPath, json);
        }

        public BasicAssetInfo GetAsset(string xldName, int id)
        {
            if (!Xlds.TryGetValue(xldName, out var xld))
                return null;

            if (!xld.Assets.TryGetValue(id, out var asset))
            {
                asset = new BasicAssetInfo { Parent = xld, };
                xld.Assets[id] = asset;
            }

            return asset;
        }
    }
}
