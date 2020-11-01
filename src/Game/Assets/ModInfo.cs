using System;
using UAlbion.Config;

namespace UAlbion.Game.Assets
{
    public class ModInfo
    {
        public ModInfo(string name, AssetConfig assetConfig, ModConfig modConfig)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            AssetConfig = assetConfig ?? throw new ArgumentNullException(nameof(assetConfig));
            ModConfig = modConfig ?? throw new ArgumentNullException(nameof(modConfig));
        }

        public string Name { get; }
        public AssetConfig AssetConfig { get; }
        public ModConfig ModConfig { get; }
        public AssetMapping Mapping { get; } = new AssetMapping();
    }
}