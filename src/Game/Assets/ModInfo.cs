using System;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats;

namespace UAlbion.Game.Assets;

public class ModInfo
{
    public ModInfo(
        string name,
        string path,
        AssetConfig assetConfig,
        ModConfig modConfig,
        AssetMapping mapping,
        IJsonUtil json,
        IFileSystem disk)
    {
        if (json == null) throw new ArgumentNullException(nameof(json));
        if (disk == null) throw new ArgumentNullException(nameof(disk));
        if (disk == null) throw new ArgumentNullException(nameof(disk));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Path = path;
        AssetConfig = assetConfig ?? throw new ArgumentNullException(nameof(assetConfig));
        ModConfig = modConfig ?? throw new ArgumentNullException(nameof(modConfig));

        var assetPath = path;

        if (!string.IsNullOrEmpty(ModConfig.ShaderPath))
        {
            if (System.IO.Path.IsPathRooted(ModConfig.ShaderPath) || ModConfig.ShaderPath.Contains("..", StringComparison.Ordinal))
            {
                throw new FormatException(
                    $"The shader path ({ModConfig.ShaderPath}) for mod {Name} " +
                    "is invalid - asset paths must be a relative path to a location inside the mod directory");
            }

            ShaderPath = System.IO.Path.Combine(path, ModConfig.ShaderPath);
        }

        if (!string.IsNullOrEmpty(ModConfig.AssetPath))
        {
            if (System.IO.Path.IsPathRooted(ModConfig.AssetPath) || ModConfig.AssetPath.Contains("..", StringComparison.Ordinal))
            {
                throw new FormatException(
                    $"The asset path ({ModConfig.AssetPath}) for mod {Name} " +
                    "is invalid - asset paths must be a relative path to a location inside the mod directory");
            }

            assetPath = System.IO.Path.Combine(Path, ModConfig.AssetPath);
        }

        SerdesContext = new SerdesContext(json, mapping, disk.Duplicate(assetPath));
    }

    public string Name { get; }
    public string Path { get; }
    public string ShaderPath { get; }
    public AssetConfig AssetConfig { get; }
    public ModConfig ModConfig { get; }
    public SerdesContext SerdesContext { get; }
    public override string ToString() => $"Mod:{Name}";
}