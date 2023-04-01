using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using UAlbion.Api;

namespace UAlbion.Config;

/// <summary>
/// A JSON configuration file containing all details of how to load and save assets for a given mod.
/// </summary>
public class AssetConfig : IAssetConfig
{
    /// <summary>
    /// A set of aliases for enums to be used as asset ids
    /// </summary>
    [JsonIgnore] public IReadOnlyDictionary<string, AssetTypeInfo>  IdTypes        { get; private set; }
    /// <summary>
    /// A set of mappings between asset ids and string ids
    /// </summary>
    [JsonIgnore] public IReadOnlyDictionary<string, string>         StringMappings { get; private set; }
    /// <summary>
    /// A set of aliases for .NET types (deriving from IAssetLoader) to be used for loading assets
    /// </summary>
    [JsonIgnore] public IReadOnlyDictionary<string, string>         Loaders        { get; private set; }
    /// <summary>
    /// A set of aliases for .NET types (deriving from IAssetContainer) to be used for extracting individual assets from container formats.
    /// </summary>
    [JsonIgnore] public IReadOnlyDictionary<string, string>         Containers     { get; private set; }
    /// <summary>
    /// A set of aliases for .NET types (deriving from IAssetPostProcessor) to be used for additional post-load asset processing.
    /// </summary>
    [JsonIgnore] public IReadOnlyDictionary<string, string>         PostProcessors { get; private set; }
    /// <summary>
    /// A set of supported natural languages that the player can choose to play the game in.
    /// </summary>
    [JsonIgnore] public IReadOnlyDictionary<string, LanguageConfig> Languages      { get; private set; }
    /// <summary>
    /// The collection of files comprising the mod, paths are relative to the mod directory by default.
    /// </summary>
    [JsonIgnore] public IReadOnlyDictionary<string, AssetFileInfo>  Files          { get; private set; }

    /// <summary>
    /// The collection of types containing static IVar instances to be registered
    /// </summary>
    [JsonInclude, JsonPropertyName("VarTypes")] public List<string> VarTypes { get; private set; } = new();

    // Dictionaries containing only the details defined in this particular mod, and not in any that this mod inherits from.
    [JsonInclude, JsonPropertyName("IdTypes")]        public Dictionary<string, AssetTypeInfo>  RawIdTypes        { get; private set; } = new();
    [JsonInclude, JsonPropertyName("StringMappings")] public Dictionary<string, string>         RawStringMappings { get; private set; } = new();
    [JsonInclude, JsonPropertyName("Loaders")]        public Dictionary<string, string>         RawLoaders        { get; private set; } = new();
    [JsonInclude, JsonPropertyName("Containers")]     public Dictionary<string, string>         RawContainers     { get; private set; } = new();
    [JsonInclude, JsonPropertyName("PostProcessors")] public Dictionary<string, string>         RawPostProcessors { get; private set; } = new();
    [JsonInclude, JsonPropertyName("Languages")]      public Dictionary<string, LanguageConfig> RawLanguages      { get; private set; } = new();
    [JsonInclude, JsonPropertyName("Files")]          public Dictionary<string, AssetFileInfo>  RawFiles          { get; private set; } = new();

    Dictionary<AssetId, AssetInfo[]> _assetLookup;
    AssetMapping _mapping;

    public static AssetConfig Parse(
        byte[] configText,
        AssetConfig parent,
        AssetMapping mapping,
        IJsonUtil jsonUtil)
    {
        if (jsonUtil == null) throw new ArgumentNullException(nameof(jsonUtil));
        var config = jsonUtil.Deserialize<AssetConfig>(configText);
        if (config == null)
            return null;

        if (parent != null)
        {
            config.IdTypes        = new FallbackDictionary<string, AssetTypeInfo> (config.RawIdTypes,        parent.IdTypes);
            config.StringMappings = new FallbackDictionary<string, string>        (config.RawStringMappings, parent.StringMappings);
            config.Loaders        = new FallbackDictionary<string, string>        (config.RawLoaders,        parent.Loaders);
            config.Containers     = new FallbackDictionary<string, string>        (config.RawContainers,     parent.Containers);
            config.PostProcessors = new FallbackDictionary<string, string>        (config.RawPostProcessors, parent.PostProcessors);
            config.Languages      = new FallbackDictionary<string, LanguageConfig>(config.RawLanguages,      parent.Languages);
            config.Files          = new FallbackDictionary<string, AssetFileInfo> (config.RawFiles,          parent.Files);
        }
        else
        {
            config.IdTypes        = config.RawIdTypes;
            config.StringMappings = config.RawStringMappings;
            config.Loaders        = config.RawLoaders;
            config.Containers     = config.RawContainers;
            config.PostProcessors = config.RawPostProcessors;
            config.Languages      = config.RawLanguages;
            config.Files          = config.RawFiles;
        }

        config.PostLoad(mapping);
        return config;
    }

    public static AssetConfig Load(
        string configPath,
        AssetConfig parent,
        AssetMapping mapping,
        IFileSystem disk,
        IJsonUtil jsonUtil)
    {
        if (disk == null) throw new ArgumentNullException(nameof(disk));
        if (jsonUtil == null) throw new ArgumentNullException(nameof(jsonUtil));
        if (!disk.FileExists(configPath))
            throw new FileNotFoundException($"Could not open asset config from {configPath}");

        var configText = disk.ReadAllBytes(configPath);
        var config = Parse(configText, parent, mapping, jsonUtil);
        if(config == null)
            throw new FileLoadException($"Could not load asset config from \"{configPath}\"");
        return config;
    }

    public void Save(string configPath, IFileSystem disk, IJsonUtil jsonUtil)
    {
        if (disk == null) throw new ArgumentNullException(nameof(disk));
        if (jsonUtil == null) throw new ArgumentNullException(nameof(jsonUtil));
        var json = jsonUtil.Serialize(this);
        disk.WriteAllText(configPath, json);
    }

    public AssetInfo[] GetAssetInfo(AssetId id)
        => _assetLookup.TryGetValue(id, out var info) 
            ? info 
            : Array.Empty<AssetInfo>();

    void PostLoad(AssetMapping mapping)
    {
        _mapping = mapping ?? throw new ArgumentNullException(nameof(mapping));
        foreach (var kvp in IdTypes)
            kvp.Value.Alias = kvp.Key;

        foreach (var kvp in Languages)
            kvp.Value.Id = kvp.Key;

        foreach (var kvp in Files)
        {
            kvp.Value.Config = this;
            int index = kvp.Key.IndexOf('#', StringComparison.Ordinal);
            kvp.Value.Filename = index == -1 ? kvp.Key : kvp.Key[..index];
            if (index != -1)
                kvp.Value.Sha256Hash = kvp.Key[(index + 1)..];

            // Resolve type aliases
            if (kvp.Value.Loader != null && Loaders.TryGetValue(kvp.Value.Loader, out var typeName)) kvp.Value.Loader = typeName;
            if (kvp.Value.Container != null && Containers.TryGetValue(kvp.Value.Container, out typeName)) kvp.Value.Container = typeName;
            if (kvp.Value.Post != null && PostProcessors.TryGetValue(kvp.Value.Post, out typeName)) kvp.Value.Post = typeName;

            foreach (var asset in kvp.Value.Map)
            {
                asset.Value.Index = asset.Key;
                asset.Value.File = kvp.Value;
            }
        }
    }

    public delegate IList<(int RangeStart, int RangeLength)> GetSubItemCountMethod(AssetFileInfo info);
    public delegate byte[] ReadAllBytesMethod(string path);
    public void PopulateAssetIds(
        IJsonUtil jsonUtil,
        GetSubItemCountMethod getSubItemCountForFile,
        ReadAllBytesMethod readAllBytesFunc)
    {
        if (jsonUtil == null) throw new ArgumentNullException(nameof(jsonUtil));

        var temp = new Dictionary<AssetId, List<AssetInfo>>();
        foreach (var file in Files)
        {
            file.Value.PopulateAssetIds(jsonUtil, getSubItemCountForFile, readAllBytesFunc);

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

            var target = ResolveId(targetStr);
            var (min, max) = ParseRange(range);
            mapping.RegisterStringRedirect(enumType, target, min, max, offset);
        }
    }

    static IEnumerable<string> Validate(IEnumerable<string> types, string category, string assetConfigPath)
    {
        foreach (var type in types)
        {
            var enumType = Type.GetType(type);
            if (enumType == null)
               yield return $"Could not load {category} type \"{type}\" defined in \"{assetConfigPath}\"";
        }
    }

    public void Validate(string assetConfigPath)
    {
        var errors = new List<string>();
        errors.AddRange(Validate(IdTypes.Values.Select(x => x.EnumType), "enum", assetConfigPath));
        errors.AddRange(Validate(Loaders.Values, "loader", assetConfigPath));
        errors.AddRange(Validate(Containers.Values, "container", assetConfigPath));
        errors.AddRange(Validate(PostProcessors.Values, "post-processor", assetConfigPath));
        if (errors.Count > 0)
        {
            var combined = string.Join(Environment.NewLine, errors);
            throw new InvalidOperationException(combined);
        }
    }

    static (int, int) ParseRange(string s)
    {
        if (s == "*")
            return (0, int.MaxValue);

        int index = s.IndexOf('-', StringComparison.Ordinal);
        if (index == -1)
        {
            if(!int.TryParse(s, out var asInt))
                throw new FormatException($"Invalid id range \"{s}\"");

            return (asInt, asInt);
        }

        var from = s[..index];
        var to = s[(index + 1)..];
        if(!int.TryParse(from, out var min))
            throw new FormatException($"Invalid id range \"{s}\"");

        if(!int.TryParse(to, out var max))
            throw new FormatException($"Invalid id range \"{s}\"");

        return (min, max);
    }

    static (string, string) SplitId(string id, char separator)
    {
        int index = id.IndexOf(separator, StringComparison.Ordinal);
        if (index == -1)
            return (id, null);

        var type = id[..index];
        var val = id[(index + 1)..];
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

    internal AssetId ResolveId(string id)
    {
        var (type, val) = SplitId(id, '.');
        if (val == null)
            throw new FormatException("Asset IDs should consist of an alias type and value, separated by a '.' character");
        var enumType = ResolveIdType(type);
        return _mapping.EnumToId(enumType, val);
    }
}
