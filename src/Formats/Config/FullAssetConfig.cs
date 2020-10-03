using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace UAlbion.Formats.Config
{
    public class FullAssetConfig : IAssetConfig
    {
        public const string Filename = "assets.json";

        public IDictionary<string, FullAssetFileInfo> Files { get; } = new Dictionary<string, FullAssetFileInfo>();

        public static FullAssetConfig Load(string basePath)
        {
            var configPath = Path.Combine(basePath, "data", Filename);
            FullAssetConfig config = new FullAssetConfig();
            if (File.Exists(configPath))
            {
                var configText = File.ReadAllText(configPath);
                var files = JsonConvert.DeserializeObject<IDictionary<string, FullAssetFileInfo>>(configText);

                foreach (var file in files)
                {
                    file.Value.Name = file.Key;
                    foreach(var o in file.Value.Assets)
                    {
                        o.Value.Parent = file.Value;
                        o.Value.Id = o.Key + file.Value.IdOffset;
                        o.Value.PaletteHints ??= new List<int>();
                    }

                    config.Files[file.Key] = file.Value;
                }
            }
            return config;
        }

        public void Save(string basePath)
        {
            foreach (var xld in Files)
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

            var configPath = Path.Combine(basePath, "data", Filename);
            var serializerSettings = new JsonSerializerSettings { Formatting = Formatting.Indented, NullValueHandling = NullValueHandling.Ignore };
            var json = JsonConvert.SerializeObject(Files, serializerSettings);
            File.WriteAllText(configPath, json);

            var basicConfig = BasicAssetConfig.Extract(this);
            basicConfig.Save(basePath);

            foreach (var file in Files)
                foreach (var asset in file.Value.Assets)
                    asset.Value.PaletteHints ??= new List<int>();
        }

        public AssetInfo GetAsset(string filename, int subObject, int id)
        {
            if (!Files.TryGetValue(filename, out var xld))
                return null;

            if (!xld.Assets.TryGetValue(subObject, out var asset))
            {
                asset = new FullAssetInfo { Parent = xld, Id = id };
                xld.Assets[subObject] = asset;
            }

            return asset;
        }
    }
}
