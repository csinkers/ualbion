#if false
using System;
using System.Collections.Generic;
using System.IO;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.Assets.Save;
using UAlbion.Formats.Config;

namespace UAlbion.Game.Assets
{
    public class SavedGameLocator : Component, IAssetLocator
    {
        readonly IAssetLoaderRegistry _assetLoaderRegistry;

        public SavedGameLocator(IAssetLoaderRegistry assetLoaderRegistry)
        {
            _assetLoaderRegistry = assetLoaderRegistry;
        }

        public IEnumerable<AssetType> SupportedTypes => new[] { AssetType.SavedGame };
        public object LoadAsset(AssetId key, SerializationContext context, Func<AssetId, SerializationContext, object> loaderFunc)
        {
            if (loaderFunc == null) throw new ArgumentNullException(nameof(loaderFunc));
            var generalConfig = (IGeneralConfig)loaderFunc(new AssetId(AssetType.GeneralConfig), context);
            var filename = Path.Combine(generalConfig.BasePath, generalConfig.SavePath, $"SAVE.{key.Id:D3}");

            var loader = _assetLoaderRegistry.GetLoader<SavedGame>(FileFormat.SavedGame);
            using var stream = File.Open(filename, FileMode.Open);
            using var br = new BinaryReader(stream);
            return loader.Serdes(
                null,
                context.Mapping,
                new AlbionReader(br, stream.Length), key, null);
        }

        public AssetInfo GetAssetInfo(AssetId key, Func<AssetId, SerializationContext, object> loaderFunc) => null;
    }
}
#endif
