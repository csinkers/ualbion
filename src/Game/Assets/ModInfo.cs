﻿using System;
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
            AssetPath = path;

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

                AssetPath = System.IO.Path.Combine(Path, ModConfig.AssetPath);
            }
        }

        public string Name { get; }
        public AssetConfig AssetConfig { get; }
        public ModConfig ModConfig { get; }
        public AssetMapping Mapping { get; } = new();
        public string Path { get; }
        public string AssetPath { get; }
        public string ShaderPath { get; }
        public override string ToString() => $"Mod:{Name}";
    }
}