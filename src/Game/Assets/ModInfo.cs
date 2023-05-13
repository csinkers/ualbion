using System;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats;

namespace UAlbion.Game.Assets;

public class ModInfo
{
    public ModInfo(
        string name,
        AssetConfig assetConfig,
        ModConfig modConfig,
        AssetMapping mapping,
        IJsonUtil json,
        IFileSystem disk) // Current directory of disk should be the mod's root dir
    {
        if (json == null) throw new ArgumentNullException(nameof(json));
        if (disk == null) throw new ArgumentNullException(nameof(disk));

        Name = name ?? throw new ArgumentNullException(nameof(name));
        AssetConfig = assetConfig ?? throw new ArgumentNullException(nameof(assetConfig));
        ModConfig = modConfig ?? throw new ArgumentNullException(nameof(modConfig));

        if (!string.IsNullOrEmpty(ModConfig.AssetConfig))
        {
            if (System.IO.Path.IsPathRooted(ModConfig.AssetConfig) || ModConfig.AssetConfig.Contains("..", StringComparison.Ordinal))
            {
                throw new FormatException(
                    $"The asset path ({ModConfig.AssetConfig}) for mod {Name} " +
                    "is invalid - asset paths must be a relative path to a location inside the mod directory");
            }
        }

        SerdesContext = new SerdesContext(name, json, mapping, disk);
    }

    public string Name { get; }
    public string ShaderPath => ModConfig.ShaderPath;
    public AssetConfig AssetConfig { get; }
    public ModConfig ModConfig { get; }
    public SerdesContext SerdesContext { get; }
    public override string ToString() => $"Mod:{Name}";
}