using System;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Config;
using UAlbion.Formats.Parsers;

namespace UAlbion.Game.Assets
{
    [AssetLocator(AssetType.CoreGraphics, AssetType.CoreGraphicsMetadata, AssetType.CoreSpriteConfig)]
    public class CoreSpriteLocator : Component, IAssetLocator
    {
        public CoreSpriteLocator() : base(null) { }
        public object LoadAsset(AssetKey key, string name, Func<AssetKey, string, object> loaderFunc)
        {
            if (key.Type == AssetType.CoreSpriteConfig)
            {
                var settings = Resolve<ISettings>();
                return CoreSpriteConfig.Load(settings.BasePath);
            }

            var assetConfig = (BasicAssetConfig)loaderFunc(new AssetKey(AssetType.AssetConfig), "AssetConfig");
            var coreSpriteConfig = (CoreSpriteConfig)loaderFunc(new AssetKey(AssetType.CoreSpriteConfig), "CoreSpriteConfig");

            if (key.Type == AssetType.CoreGraphics)
                return CoreSpriteLoader.Load((CoreSpriteId)key.Id, assetConfig.BasePath, coreSpriteConfig);

            if (key.Type == AssetType.CoreGraphicsMetadata)
                return CoreSpriteLoader.GetConfig((CoreSpriteId)key.Id, assetConfig.BasePath, coreSpriteConfig, out _);

            throw new InvalidOperationException("CoreSpriteLocator called with an invalid type");
        }
    }
}