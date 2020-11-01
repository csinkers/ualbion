using System;
using System.Collections.Generic;
using System.IO;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats.Parsers;

namespace UAlbion.Game.Assets
{
    public class CoreSpriteLocator : Component, IAssetLocator
    {
        public IEnumerable<AssetType> SupportedTypes => new[] { AssetType.CoreGraphics, AssetType.Special };
        public object LoadAsset(AssetId key, SerializationContext context, AssetInfo info)
        {
            if (key == AssetId.CoreSpriteConfig)
            {
                var settings = Resolve<ISettings>();
                return CoreSpriteConfig.Load(settings.BasePath);
            }

            var assets = Resolve<IAssetManager>();
            var generalConfig = assets.LoadGeneralConfig(); // (GeneralConfig)loaderFunc(AssetId.GeneralConfig, context);
            var coreSpriteConfig = assets.LoadCoreSpriteConfig(); // (CoreSpriteConfig)loaderFunc(AssetId.CoreSpriteConfig, context);

            var exePath = Path.Combine(generalConfig.BasePath, generalConfig.ExePath);
            if (key.Type == AssetType.CoreGraphics)
                return CoreSpriteLoader.Load(key, exePath, coreSpriteConfig);

            if (key == AssetId.CoreGraphicsMetadata)
                return CoreSpriteLoader.GetConfig(key, exePath, coreSpriteConfig, out _);

            throw new InvalidOperationException("CoreSpriteLocator called with an invalid type");
        }
    }
}
