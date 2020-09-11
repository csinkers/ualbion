using System;
using System.Collections.Generic;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Config;

namespace UAlbion.Game.Assets
{
    public class AssetConfigLocator : Component, IAssetLocator
    {
        readonly bool _useFullConfig;

        public AssetConfigLocator(bool useFullConfig) => _useFullConfig = useFullConfig;

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
                AssetType.AssetConfig when !_useFullConfig => BasicAssetConfig.Load(settings.BasePath),
                AssetType.AssetConfig => FullAssetConfig.Load(settings.BasePath),
                AssetType.GeneralConfig => GeneralConfig.Load(settings.BasePath),
                _ => throw new ArgumentOutOfRangeException(nameof(key))
            };
        }

        public AssetInfo GetAssetInfo(AssetKey key, Func<AssetKey, object> loaderFunc) => null;
    }
}
