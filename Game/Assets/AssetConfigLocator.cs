using System;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Config;

namespace UAlbion.Game.Assets
{
    [AssetLocator(AssetType.AssetConfig, AssetType.GeneralConfig)]
    public class AssetConfigLocator : Component, IAssetLocator
    {
        public AssetConfigLocator() : base(null) { }

        public object LoadAsset(AssetKey key, string name, Func<AssetKey, string, object> loaderFunc)
        {
            var settings = Resolve<ISettings>();
            return key.Type switch
            {
                AssetType.AssetConfig => BasicAssetConfig.Load(settings.BasePath),
                AssetType.GeneralConfig => GeneralConfig.Load(settings.BasePath),
                _ => throw new ArgumentOutOfRangeException(nameof(key))
            };
        }
    }
}