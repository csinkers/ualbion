using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace UAlbion.Formats.Config
{
    public class BasicAssetConfig : IAssetConfig
    {
        public const string Filename = "assets_min.json";
        public IDictionary<string, BasicAssetFileInfo> Files { get; } = new Dictionary<string, BasicAssetFileInfo>();

        public static BasicAssetConfig Extract(FullAssetConfig full)
        {
            if (full == null) throw new ArgumentNullException(nameof(full));
            var min = new BasicAssetConfig();

            foreach (var kvp in full.Files)
            {
                var old = kvp.Value;
                var newFile = new BasicAssetFileInfo(old);

                foreach(var asset in old.Assets.Values)
                {
                    var newAsset = new BasicAssetInfo(asset) { Parent = newFile };
                    if (newAsset.ContainsData)
                        newFile.Assets[asset.Id - asset.Parent.IdOffset] = newAsset;
                }

                min.Files[kvp.Key] = newFile;
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
                var files = JsonConvert.DeserializeObject<Dictionary<string, BasicAssetFileInfo>>(configText);

                foreach (var file in files)
                {
                    file.Value.Name = file.Key;
                    foreach(var o in file.Value.Assets)
                    {
                        o.Value.Parent = file.Value;
                        o.Value.Id = o.Key;
                    }

                    config.Files[file.Key] = file.Value;
                }
            }

            return config;
        }

        public void Save(string basePath)
        {
            var configPath = Path.Combine(basePath, "data", Filename);
            var serializerSettings = new JsonSerializerSettings { Formatting = Formatting.Indented, NullValueHandling = NullValueHandling.Ignore };
            var json = JsonConvert.SerializeObject(Files, serializerSettings);
            File.WriteAllText(configPath, json);
        }

        public AssetInfo GetAsset(string filename, int subObject, int id)
        {
            if (!Files.TryGetValue(filename, out var file))
                return null;

            if (!file.Assets.TryGetValue(subObject, out var asset))
            {
                asset = new BasicAssetInfo { Parent = file, Id = id };
                file.Assets[subObject] = asset;
            }

            return asset;
        }
    }
}
