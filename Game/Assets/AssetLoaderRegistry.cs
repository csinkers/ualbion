using System.Collections.Generic;
using System.IO;
using UAlbion.Formats;
using UAlbion.Formats.Config;

namespace UAlbion.Game.Assets
{
    public static class AssetLoaderRegistry
    {
        static readonly IDictionary<FileFormat, IAssetLoader> Loaders = GetAssetLoaders();
        static IDictionary<FileFormat, IAssetLoader> GetAssetLoaders()
        {
            var dict = new Dictionary<FileFormat, IAssetLoader>();
            foreach(var (loader, attribute) in ReflectionHelper.GetAttributeTypes<IAssetLoader, AssetLoaderAttribute>())
                foreach (var format in attribute.SupportedFormats)
                    dict.Add(format, loader);
            return dict;
        }

        public static IAssetLoader GetLoader(FileFormat type) => Loaders[type];
        public static object Load(BinaryReader br, string name, int streamLength, AssetConfig.Asset config) => GetLoader(config.Type).Load(br, streamLength, name, config);
    }
}