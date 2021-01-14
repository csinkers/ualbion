using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace UAlbion.Config
{
    public class AssetConfig : IAssetConfig
    {
        public IDictionary<string, AssetTypeInfo> Types { get; } = new Dictionary<string, AssetTypeInfo>();
        Dictionary<(string, int), AssetInfo> _assetLookup;

        public static AssetConfig Load(string configPath)
        {
            if (!File.Exists(configPath))
                throw new FileNotFoundException($"Could not open asset config from {configPath}");

            AssetConfig config = new AssetConfig();
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

        public void Save(string configPath)
        {
            foreach (var assetType in Types)
                assetType.Value.PreSave();

            var serializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            };
            var json = JsonConvert.SerializeObject(Types, serializerSettings);
            File.WriteAllText(configPath, json);

            // var basicConfig = BasicAssetConfig.Extract(this);
            // basicConfig.Save(basePath);
        }

        public AssetInfo GetAssetInfo(string typeName, int id)
            => _assetLookup.TryGetValue((typeName, id), out var info) ? info : null;

        public void PopulateAssetIds(AssetMapping mapping)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            foreach (var typeKvp in Types)
            {
                var type = Type.GetType(typeKvp.Key);
                foreach(var asset in typeKvp.Value.Files.SelectMany(x => x.Value.Assets))
                    asset.Value.AssetId = mapping.EnumToId(type, asset.Value.Id);
            }
        }

        public AssetTypeInfo GetTypeInfo(string name) => Types.TryGetValue(name, out var info) ? info : null;
    }
}
