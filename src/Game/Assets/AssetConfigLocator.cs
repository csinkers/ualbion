/*
using System;
using System.Collections.Generic;
using UAlbion.Base;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats.Config;

namespace UAlbion.Game.Assets
{
    public class AssetConfigLocator : Component, IAssetLocator
    {
        public IEnumerable<AssetType> SupportedTypes => new[] { AssetType.Special, };

        public object LoadAsset(AssetId key, SerializationContext context, AssetInfo info)
        {
            var settings = Resolve<ISettings>();
            if (key == AssetId.From(Special.AssetConfig))
                return AssetConfig.Load(settings.BasePath);
            if (key == AssetId.From(Special.GeneralConfig))
                return GeneralConfig.Load(settings.BasePath);
            throw new ArgumentOutOfRangeException(nameof(key));
        }
    }
}
*/
