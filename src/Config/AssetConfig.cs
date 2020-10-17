using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace UAlbion.Config
{
    public class AssetConfig : IAssetConfig
    {
        public const string Filename = "assets.json";
        public IDictionary<string, AssetTypeInfo> Types { get; } = new Dictionary<string, AssetTypeInfo>();
        Dictionary<(string, int), AssetInfo> _assetLookup;

        public static AssetConfig Load(string basePath)
        {
            var configPath = Path.Combine(basePath, "data", "Base", Filename);
            AssetConfig config = new AssetConfig();
            if (!File.Exists(configPath))
                return config;

            var configText = File.ReadAllText(configPath);
            var assetTypes = JsonConvert.DeserializeObject<IDictionary<string, AssetTypeInfo>>(configText);

            foreach (var assetType in assetTypes)
            {
                assetType.Value.PostLoad();
                config.Types[assetType.Key] = assetType.Value;
            }

            config._assetLookup =
                (from type in config.Types
                 from file in type.Value.Files
                 from asset in file.Value.Assets
                 select (type.Key, asset.Value))
                .ToDictionary(x => (x.Key, x.Value.Id), x => x.Value);

            return config;
        }

        public void Save(string basePath)
        {
            foreach (var assetType in Types)
                assetType.Value.PreSave();

            var configPath = Path.Combine(basePath, "data", Filename);
            var serializerSettings = new JsonSerializerSettings { Formatting = Formatting.Indented, NullValueHandling = NullValueHandling.Ignore };
            var json = JsonConvert.SerializeObject(Types, serializerSettings);
            File.WriteAllText(configPath, json);

            // var basicConfig = BasicAssetConfig.Extract(this);
            // basicConfig.Save(basePath);

            foreach (var assetType in Types)
                assetType.Value.PostSave();
        }

        public AssetInfo GetAsset(string typeName, int id) => _assetLookup.TryGetValue((typeName, id), out var info) ? info : null;
    }
}
