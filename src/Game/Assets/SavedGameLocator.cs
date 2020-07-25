using System;
using System.Collections.Generic;
using System.IO;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets.Save;
using UAlbion.Formats.Config;

namespace UAlbion.Game.Assets
{
    public class SavedGameLocator : Component, IAssetLocator
    {
        public IEnumerable<AssetType> SupportedTypes => new[] { AssetType.SavedGame };
        public object LoadAsset(AssetKey key, string name, Func<AssetKey, object> loaderFunc)
        {
            var generalConfig = (IGeneralConfig)loaderFunc(new AssetKey(AssetType.GeneralConfig));
            var filename = Path.Combine(generalConfig.BasePath, generalConfig.SavePath, $"SAVE.{key.Id:D3}");

            var loader = AssetLoaderRegistry.GetLoader<SavedGame>(FileFormat.SavedGame);
            using var stream = File.Open(filename, FileMode.Open);
            using var br = new BinaryReader(stream);
            return loader.Serdes(
                null,
                new AlbionReader(br, stream.Length), key, null);
        }
    }
}