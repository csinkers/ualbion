using System;
using System.Collections.Generic;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats.Config;

namespace UAlbion.Game.Assets
{
    public class AssetConfigLocator : Component, IAssetLocator
    {
        readonly bool _useFullConfig;
        public AssetConfigLocator(bool useFullConfig) => _useFullConfig = useFullConfig;
        public IEnumerable<AssetType> SupportedTypes => new[] { AssetType.Special, };

        public object LoadAsset(AssetId key, SerializationContext context, AssetInfo info)
        {
            var settings = Resolve<ISettings>();
            if(key == AssetId.AssetConfig)
                return AssetConfig.Load(settings.BasePath);
            if(key == AssetId.GeneralConfig)
                return GeneralConfig.Load(settings.BasePath);
            throw new ArgumentOutOfRangeException(nameof(key));
        }

        public AssetInfo GetAssetInfo(AssetId key) => null;
    }
}
