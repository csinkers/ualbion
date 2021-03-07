using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UAlbion.Api;

namespace UAlbion.Config
{
    public class AssetConfig : IAssetConfig
    {
        public IDictionary<string, AssetTypeInfo> IdTypes { get; } = new Dictionary<string, AssetTypeInfo>();
        public IDictionary<string, string> StringMappings { get; } = new Dictionary<string, string>();
        public IDictionary<string, string> Loaders { get; } = new Dictionary<string, string>();
        public IDictionary<string, string> Containers { get; } = new Dictionary<string, string>();
        public IDictionary<string, string> PostProcessors { get; } = new Dictionary<string, string>();
        public IDictionary<string, LanguageConfig> Languages { get; } = new Dictionary<string, LanguageConfig>();
        public IDictionary<string, AssetFileInfo> Files { get; } = new Dictionary<string, AssetFileInfo>();
        Dictionary<AssetId, AssetInfo[]> _assetLookup;

        public static AssetConfig Parse(string configText)
        {
            var config = JsonConvert.DeserializeObject<AssetConfig>(configText);
            if (config == null)
                return null;

            config?.PostLoad();
            return (AssetConfig)config;
        }

        public static AssetConfig Load(string configPath, IFileSystem disk)
        {
            if (disk == null) throw new ArgumentNullException(nameof(disk));
            if (!disk.FileExists(configPath))
                throw new FileNotFoundException($"Could not open asset config from {configPath}");

            var configText = disk.ReadAllText(configPath);
            var config = Parse(configText);
            if(config == null)
                throw new FileLoadException($"Could not load asset config from \"{configPath}\"");
            return config;
        }

        public void Save(string configPath, IFileSystem disk)
        {
            if (disk == null) throw new ArgumentNullException(nameof(disk));
            var json = JsonConvert.SerializeObject(this, ConfigUtil.JsonSerializerSettings);
            disk.WriteAllText(configPath, json);
        }

        public AssetInfo[] GetAssetInfo(AssetId id)
            => _assetLookup.TryGetValue(id, out var info) 
                ? info 
                : Array.Empty<AssetInfo>();

        void PostLoad()
        {
            foreach (var kvp in IdTypes)
                kvp.Value.Alias = kvp.Key;

            foreach (var kvp in Languages)
                kvp.Value.Id = kvp.Key;

            foreach (var kvp in Files)
            {
                int index = kvp.Key.IndexOf('#');
                kvp.Value.Filename = index == -1 ? kvp.Key : kvp.Key.Substring(0, index);
                if (index != -1)
                    kvp.Value.Sha256Hash = kvp.Key.Substring(index + 1);

                // Resolve type aliases
                if (kvp.Value.Loader != null && Loaders.TryGetValue(kvp.Value.Loader, out var typeName)) kvp.Value.Loader = typeName;
                if (kvp.Value.Container != null && Containers.TryGetValue(kvp.Value.Container, out typeName)) kvp.Value.Container = typeName;
                if (kvp.Value.Post != null && PostProcessors.TryGetValue(kvp.Value.Post, out typeName)) kvp.Value.Post = typeName;

                foreach (var asset in kvp.Value.Map)
                {
                    asset.Value.SubAssetId = asset.Key;
                    asset.Value.File = kvp.Value;
                }
            }
        }

        public void PopulateAssetIds(AssetMapping mapping, Func<AssetFileInfo, IList<(int, int)>> getSubItemCountForFile)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));

            var temp = new Dictionary<AssetId, List<AssetInfo>>();
            foreach (var file in Files)
            {
                file.Value.PopulateAssetIds(x => ResolveId(mapping, x), getSubItemCountForFile);

                foreach (var asset in file.Value.Map.Values)
                {
                    if (!temp.TryGetValue(asset.AssetId, out var list))
                    {
                        list = new List<AssetInfo>();
                        temp[asset.AssetId] = list;
                    }

                    list.Add(asset);
                }
            }

            _assetLookup = temp.ToDictionary(x => x.Key, x => x.Value.ToArray());
        }

        public void RegisterStringRedirects(AssetMapping mapping)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            foreach (var kvp in StringMappings)
            {
                var (type, range) = SplitId(kvp.Key, '.');
                var enumType = ResolveIdType(type);
                var (targetStr, offsetStr) = SplitId(kvp.Value, ':');
                if (!int.TryParse(offsetStr, out var offset))
                    offset = 0;

                var target = ResolveId(mapping, targetStr);
                var (min, max) = ParseRange(range);
                mapping.RegisterStringRedirect(enumType, target, min, max, offset);
            }
        }

        static (int, int) ParseRange(string s)
        {
            if (s == "*")
                return (0, int.MaxValue);

            int index = s.IndexOf('-');
            if (index == -1)
            {
                if(!int.TryParse(s, out var asInt))
                    throw new FormatException($"Invalid id range \"{s}\"");

                return (asInt, asInt);
            }

            var from = s.Substring(0, index);
            var to = s.Substring(index + 1);
            if(!int.TryParse(from, out var min))
                throw new FormatException($"Invalid id range \"{s}\"");

            if(!int.TryParse(to, out var max))
                throw new FormatException($"Invalid id range \"{s}\"");

            return (min, max);
        }

        static (string, string) SplitId(string id, char separator)
        {
            int index = id.IndexOf(separator);
            if (index == -1)
                return (id, null);

            var type = id.Substring(0, index);
            var val = id.Substring(index + 1);
            return (type, val);
        }

        Type ResolveIdType(string type)
        {
            if (!IdTypes.TryGetValue(type, out var assetType))
                throw new FormatException($"Could not resolve asset id alias \"{type}\"");

            var enumType = Type.GetType(assetType.EnumType);
            if (enumType == null)
                throw new FormatException($"Could not resolve type \"{assetType.EnumType}\" (alias \"{assetType.Alias}\")");

            return enumType;
        }

        AssetId ResolveId(AssetMapping mapping, string id)
        {
            var (type, val) = SplitId(id, '.');
            if (val == null)
                throw new FormatException("Asset IDs should consist of an alias type and value, separated by a '.' character");
            var enumType = ResolveIdType(type);
            return mapping.EnumToId(enumType, val);
        }
    }
}
