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
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            var assets = Resolve<IAssetManager>();
            var generalConfig = assets.LoadGeneralConfig();

            if (key == AssetId.From(Base.Special.CoreSpriteConfig))
                return CoreSpriteConfig.Load(info.Filename);

            var coreSpriteConfig = assets.LoadCoreSpriteConfig();
            var exePath = Path.Combine(generalConfig.BasePath, generalConfig.ExePath);

            if (key == AssetId.From(Base.Special.CoreGraphicsMetadata))
                return CoreSpriteLoader.GetConfig(key, exePath, coreSpriteConfig, out _);

            if (key.Type == AssetType.CoreGraphics)
                return CoreSpriteLoader.Load(key, exePath, coreSpriteConfig);

            throw new InvalidOperationException("CoreSpriteLocator called with an invalid type");
        }
    }
}
