using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace UAlbion.Formats.Config
{
    public class FullAssetConfig
    {
        public const string Filename = "assets.json";
        [JsonIgnore]
        public string BasePath { get; set; }

        [JsonIgnore] public string BaseDataPath => Path.Combine(BasePath, "data");
        public string XldPath { get; set; }
        public string ExportedXldPath { get; set; }

        public IDictionary<string, FullXldInfo> Xlds { get; } = new Dictionary<string, FullXldInfo>();

        public static FullAssetConfig Load(string basePath)
        {
            var configPath = Path.Combine(basePath, "data", Filename);
            FullAssetConfig config;
            if (File.Exists(configPath))
            {
                var configText = File.ReadAllText(configPath);
                config = JsonConvert.DeserializeObject<FullAssetConfig>(configText);

                foreach (var xld in config.Xlds)
                {
                    xld.Value.Name = xld.Key;
                    foreach(var o in xld.Value.Assets)
                    {
                        o.Value.Parent = xld.Value;
                        o.Value.Id = o.Key;
                        o.Value.PaletteHints ??= new List<int>();
                    }
                }
            }
            else
            {
                config = new FullAssetConfig
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
            foreach (var xld in Xlds)
            {
                if (xld.Value.Transposed == false)
                    xld.Value.Transposed = null;
                foreach (var asset in xld.Value.Assets)
                {
                    if (string.IsNullOrWhiteSpace(asset.Value.Name))
                        asset.Value.Name = null;

                    if (asset.Value.PaletteHints != null && !asset.Value.PaletteHints.Any())
                        asset.Value.PaletteHints = null;
                }
            }

            var configPath = Path.Combine(BasePath, "data", Filename);
            var serializerSettings = new JsonSerializerSettings { Formatting = Formatting.Indented, NullValueHandling = NullValueHandling.Ignore };
            var json = JsonConvert.SerializeObject(this, serializerSettings);
            File.WriteAllText(configPath, json);

            var basicConfig = BasicAssetConfig.Extract(this);
            basicConfig.Save();

            foreach (var xld in Xlds)
            {
                foreach (var asset in xld.Value.Assets)
                {
                    if (asset.Value.PaletteHints == null)
                        asset.Value.PaletteHints = new List<int>();
                }
            }
        }

        public FullAssetInfo GetAsset(string xldName, int id)
        {
            if (!Xlds.TryGetValue(xldName, out var xld))
                return null;

            if (!xld.Assets.TryGetValue(id, out var asset))
            {
                asset = new FullAssetInfo {Parent = xld,};
                xld.Assets[id] = asset;
            }

            return asset;
        }
    }
}