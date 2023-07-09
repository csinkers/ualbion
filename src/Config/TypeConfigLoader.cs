using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using UAlbion.Api;

namespace UAlbion.Config;

public class TypeConfigLoader
{
    readonly IJsonUtil _jsonUtil;
    public TypeConfigLoader(IJsonUtil jsonUtil) 
        => _jsonUtil = jsonUtil ?? throw new ArgumentNullException(nameof(jsonUtil));

#pragma warning disable CA1812 // 'AssetConfigLoader.RawTypeConfig' is an internal class that is apparently never instantiated. If so, remove the code from the assembly. If this class is intended to contain only static members, make it 'static' (Module in Visual Basic).
    class RawTypeConfig
    {
        [JsonInclude, JsonPropertyName("Languages")] public Dictionary<string, LanguageConfig> Languages { get; private set; } = new();
        [JsonInclude, JsonPropertyName("IdTypes")] public Dictionary<string, AssetTypeInfo> IdTypes { get; private set; } = new();
        [JsonInclude, JsonPropertyName("Containers")] public Dictionary<string, string> Containers { get; private set; } = new();
        [JsonInclude, JsonPropertyName("Loaders")] public Dictionary<string, string> Loaders { get; private set; } = new();
        [JsonInclude, JsonPropertyName("PostProcessors")] public Dictionary<string, string> PostProcessors { get; private set; } = new();
        [JsonInclude, JsonPropertyName("GlobalPropertyTypes")] public List<string> GlobalPropertyTypes { get; private set; } = new();
        [JsonInclude, JsonPropertyName("VarTypes")] public List<string> VarTypes { get; private set; } = new();
    }
#pragma warning restore CA1812 // 'AssetConfigLoader.RawTypeConfig' is an internal class that is apparently never instantiated. If so, remove the code from the assembly. If this class is intended to contain only static members, make it 'static' (Module in Visual Basic).

    public TypeConfig Load(string configPath, string modName, TypeConfig parent, AssetMapping mapping, IFileSystem disk)
    {
        if (disk == null) throw new ArgumentNullException(nameof(disk));
        if (!disk.FileExists(configPath))
            throw new FileNotFoundException($"Could not open asset config from {configPath}");

        var configText = disk.ReadAllBytes(configPath);
        var config = Parse(configText, modName, parent, mapping);
        if (config == null)
            throw new FileLoadException($"Could not load asset config from \"{configPath}\"");

        return config;
    }

    public IReadOnlyDictionary<string, AssetTypeInfo> LoadIdTypesOnly(string configPath, IFileSystem disk)
    {
        if (disk == null) throw new ArgumentNullException(nameof(disk));
        if (!disk.FileExists(configPath))
            throw new FileNotFoundException($"Could not open asset config from {configPath}");

        var configText = disk.ReadAllBytes(configPath);
        var raw = _jsonUtil.Deserialize<RawTypeConfig>(configText);
        if (raw == null)
            return null;

        foreach (var kvp in raw.IdTypes)
            kvp.Value.Alias = kvp.Key;

        return raw.IdTypes;
    }

    public TypeConfig Parse(byte[] configText, string modName, TypeConfig parent, AssetMapping mapping)
    {
        var raw = _jsonUtil.Deserialize<RawTypeConfig>(configText);
        if (raw == null)
            return null;

        foreach (var kvp in raw.IdTypes)
            kvp.Value.Alias = kvp.Key;

        foreach (var kvp in raw.Languages)
            kvp.Value.Id = kvp.Key;

        var loaders = GetTypes(raw.Loaders);
        var containers = GetTypes(raw.Containers);
        var postProcessors = GetTypes(raw.PostProcessors);

        var config = new TypeConfig(modName, mapping)
        {
            IdTypes        = parent != null ? new FallbackDictionary<string, AssetTypeInfo>(raw.IdTypes, parent.IdTypes)      : raw.IdTypes,
            Loaders        = parent != null ? new FallbackDictionary<string, Type>(loaders, parent.Loaders)                   : loaders,
            Containers     = parent != null ? new FallbackDictionary<string, Type>(containers, parent.Containers)             : containers,
            PostProcessors = parent != null ? new FallbackDictionary<string, Type>(postProcessors, parent.PostProcessors)     : postProcessors,
            Languages      = parent != null ? new FallbackDictionary<string, LanguageConfig>(raw.Languages, parent.Languages) : raw.Languages,
            PropertyTypes  = LoadPropertyTypes(modName, parent, raw),
            VarTypes       = raw.VarTypes
        };

        return config;
    }

    static AssetProperties LoadPropertyTypes(string modName, TypeConfig parent, RawTypeConfig raw)
    {
        var properties = new AssetProperties(parent?.PropertyTypes);

        foreach (var kvp in raw.Loaders)
        {
            var type = Type.GetType(kvp.Value);
            if (type == null)
                throw new InvalidOperationException($"Could not load type \"{kvp.Value}\" as property container from mod {modName}");

            properties.LoadAssetPropertiesFromType(false, type);
        }

        foreach (var kvp in raw.Containers)
        {
            var type = Type.GetType(kvp.Value);
            if (type == null)
                throw new InvalidOperationException($"Could not load type \"{kvp.Value}\" as property container from mod {modName}");

            properties.LoadAssetPropertiesFromType(false, type);
        }

        foreach (var typeName in raw.GlobalPropertyTypes)
        {
            var type = Type.GetType(typeName);
            if (type == null)
                throw new InvalidOperationException($"Could not load type \"{typeName}\" as property container from mod {modName}");

            properties.LoadAssetPropertiesFromType(true, type);
        }

        return properties;
    }

    static Dictionary<string, Type> GetTypes(Dictionary<string, string> typeNames)
    {
        var results = new Dictionary<string, Type>();
        foreach (var kvp in typeNames)
        {
            var type = Type.GetType(kvp.Value);
            results[kvp.Key] = type;
        }

        return results;
    }
}