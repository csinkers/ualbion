using System;
using System.Collections.Generic;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Config;

namespace UAlbion.Game.Assets
{
    public class AssetConfigLocator : Component, IAssetLocator
    {
        public IEnumerable<AssetType> SupportedTypes => new[]
        {
            AssetType.AssetConfig,
            AssetType.GeneralConfig,
        };

        public object LoadAsset(AssetKey key, string name, Func<AssetKey, object> loaderFunc)
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
