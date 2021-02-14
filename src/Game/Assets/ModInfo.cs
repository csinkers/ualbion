using System;
using UAlbion.Config;

namespace UAlbion.Game.Assets
{
    public class ModInfo
    {
        public ModInfo(string name, AssetConfig assetConfig, ModConfig modConfig, string path)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            AssetConfig = assetConfig ?? throw new ArgumentNullException(nameof(assetConfig));
            ModConfig = modConfig ?? throw new ArgumentNullException(nameof(modConfig));
            Path = path;
            AssetPath = Path;
            if (!string.IsNullOrEmpty(ModConfig.AssetPath))
            {
                if (System.IO.Path.IsPathRooted(ModConfig.AssetPath) || ModConfig.AssetPath.Contains("..", StringComparison.Ordinal))
                {
                    throw new FormatException(
                        $"The asset path ({ModConfig.AssetPath}) for mod {Name} " +
                        "is invalid - asset paths must be a relative path to a location inside the mod directory");
                }

                AssetPath = System.IO.Path.Combine(Path, ModConfig.AssetPath);
            }
        }

        public string Name { get; }
        public AssetConfig AssetConfig { get; }
        public ModConfig ModConfig { get; }
        public AssetMapping Mapping { get; } = new AssetMapping();
        public string Path { get; }
        public string AssetPath { get; }
    }
}