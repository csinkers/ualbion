using System;
using UAlbion.Api;
using UAlbion.Config;

namespace UAlbion.Game.Assets;

public class ModInfo
{
    public ModInfo(
        string name,
        TypeConfig typeConfig,
        AssetConfig assetConfig,
        ModConfig modConfig,
        AssetMapping mapping,
        IJsonUtil json,
        IFileSystem disk) // Current directory of disk should be the mod's root dir
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentNullException.ThrowIfNull(disk);

        Name = name ?? throw new ArgumentNullException(nameof(name));
        AssetConfig = assetConfig ?? throw new ArgumentNullException(nameof(assetConfig));
        TypeConfig = typeConfig ?? throw new ArgumentNullException(nameof(typeConfig));
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

        ModContext = new ModContext(name, json, disk, mapping);
    }

    public string Name { get; }
    public string ShaderPath => ModConfig.ShaderPath;
    public AssetConfig AssetConfig { get; }
    public TypeConfig TypeConfig { get; }
    public ModConfig ModConfig { get; }
    public ModContext ModContext { get; }
    public override string ToString() => $"Mod:{Name}";
}