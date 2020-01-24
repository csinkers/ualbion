using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace UAlbion.Formats.Config
{
    public class BasicAssetConfig
    {
        public const string Filename = "assets_min.json";
        [JsonIgnore] public string BasePath { get; set; }
        [JsonIgnore] public string BaseDataPath => Path.Combine(BasePath, "data");
        public string XldPath { get; set; }
        public string ExportedXldPath { get; set; }

        public IDictionary<string, BasicXldInfo> Xlds { get; } = new Dictionary<string, BasicXldInfo>();

        public BasicAssetConfig() { }

        public static BasicAssetConfig Extract(FullAssetConfig full)
        {
            var min = new BasicAssetConfig
            {
                BasePath = full.BasePath,
                XldPath = full.XldPath,
                ExportedXldPath = full.ExportedXldPath,
            };

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
            BasicAssetConfig config;
            if (File.Exists(configPath))
            {
                var configText = File.ReadAllText(configPath);
                config = JsonConvert.DeserializeObject<BasicAssetConfig>(configText);

                foreach (var xld in config.Xlds)
                {
                    xld.Value.Name = xld.Key;
                    foreach(var o in xld.Value.Assets)
                    {
                        o.Value.Parent = xld.Value;
                        o.Value.Id = o.Key;
                    }
                }
            }
            else
            {
                config = new BasicAssetConfig
                {
                    XldPath = @"albion_sr/CD/XLDLIBS",
                    ExportedXldPath = @"exported"
                };
            }
            config.BasePath = basePath;
            return config;
        }

        public void Save()
        {
            var configPath = Path.Combine(BasePath, "data", Filename);
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
