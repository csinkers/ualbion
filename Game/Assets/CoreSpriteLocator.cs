using System;
using System.IO;
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

            var generalConfig = (GeneralConfig)loaderFunc(new AssetKey(AssetType.GeneralConfig), "GeneralConfig");
            var coreSpriteConfig = (CoreSpriteConfig)loaderFunc(new AssetKey(AssetType.CoreSpriteConfig), "CoreSpriteConfig");

            var exePath = Path.Combine(generalConfig.BasePath, generalConfig.ExePath);
            if (key.Type == AssetType.CoreGraphics)
                return CoreSpriteLoader.Load((CoreSpriteId)key.Id, exePath, coreSpriteConfig);

            if (key.Type == AssetType.CoreGraphicsMetadata)
                return CoreSpriteLoader.GetConfig((CoreSpriteId)key.Id, exePath, coreSpriteConfig, out _);

            throw new InvalidOperationException("CoreSpriteLocator called with an invalid type");
        }
    }
}