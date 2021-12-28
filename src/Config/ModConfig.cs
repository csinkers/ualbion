using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using UAlbion.Api;

namespace UAlbion.Config;

public class ModConfig
{
    public string Repo { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Author { get; set; }
    public Version Version { get; set; }
    public string AssetPath { get; set; }
    public string ShaderPath { get; set; }
    [JsonInclude] public List<string> Dependencies { get; private set; } = new();

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