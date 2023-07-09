using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using UAlbion.Api;

namespace UAlbion.Config;

public class ModConfig
{
    public const string ModConfigFilename = "modinfo.json";
    public string Repo { get; set; } // URL of the mod's git repo
    public string Name { get; set; } // Display name for the mod
    public string Description { get; set; }
    public string Author { get; set; }
    public Version Version { get; set; }
    public string ShaderPath { get; set; }
    public string AssetConfig { get; set; }
    public string TypeConfig { get; set; }
    public string InheritAssetConfigFrom { get; set; }
    public string InheritTypeConfigFrom { get; set; }
    [JsonInclude] public List<string> Dependencies { get; private set; } = new();
    [JsonInclude] public Dictionary<string, string> SymLinks { get; private set; } = new();

    public static ModConfig Load(string configPath, IFileSystem disk, IJsonUtil jsonUtil)
    {
        if (disk == null) throw new ArgumentNullException(nameof(disk));
        if (jsonUtil == null) throw new ArgumentNullException(nameof(jsonUtil));
        if (!disk.FileExists(configPath))
            throw new FileNotFoundException($"mod.config not found for mod {configPath}");

        var configText = disk.ReadAllBytes(configPath);
        return jsonUtil.Deserialize<ModConfig>(configText);
    }
}