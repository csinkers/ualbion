using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Config;

namespace UAlbion.Game.Assets
{
    public class AssetLoaderRegistry : ServiceComponent<IAssetLoaderRegistry>, IAssetLoaderRegistry
    {
        readonly IDictionary<FileFormat, IAssetLoader> _loaders = new Dictionary<FileFormat, IAssetLoader>();

        public IAssetLoader GetLoader(FileFormat type) => _loaders[type];
        public IAssetLoader<T> GetLoader<T>(FileFormat type) where T : class => (IAssetLoader<T>)_loaders[type];
        public object Load(BinaryReader br, AssetKey key, int streamLength, AssetInfo config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            return GetLoader(config.Format).Load(br, streamLength, key, config);
        }

        public AssetLoaderRegistry AddLoader(IAssetLoader loader)
        {
            if (loader == null) throw new ArgumentNullException(nameof(loader));
            var attribute = (AssetLoaderAttribute)loader.GetType().GetCustomAttribute(typeof(AssetLoaderAttribute), false);
            if (attribute != null)
                foreach (var format in attribute.SupportedFormats)
                    _loaders.Add(format, loader);
            return this;
        }
    }
}
