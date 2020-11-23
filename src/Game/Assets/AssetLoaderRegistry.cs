using System;
using System.Collections.Generic;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats;

namespace UAlbion.Game.Assets
{
    public class AssetLoaderRegistry : ServiceComponent<IAssetLoaderRegistry>, IAssetLoaderRegistry
    {
        readonly object _syncRoot = new object();
        readonly IDictionary<string, IAssetLoader> _loaders = new Dictionary<string, IAssetLoader>();

        public IAssetLoader GetLoader(string loaderName)
        {
            lock (_syncRoot)
                return _loaders.TryGetValue(loaderName, out var loader) ? loader : Instantiate(loaderName);
        }

        public IAssetLoader<T> GetLoader<T>(string loaderName) where T : class => (IAssetLoader<T>)GetLoader(loaderName);

        public object Load(AssetInfo config, AssetMapping mapping, ISerializer s)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            return GetLoader(config.File.Loader).Serdes(null, config, mapping, s);
        }

        IAssetLoader Instantiate(string loaderName)
        {
            if(string.IsNullOrEmpty(loaderName))
                throw new ArgumentNullException(nameof(loaderName));

            var type = Type.GetType(loaderName);
            if(type == null)
                throw new InvalidOperationException($"Could not find loader type \"{loaderName}\"");

            var constructor = type.GetConstructor(Array.Empty<Type>());
            if(constructor == null)
                throw new InvalidOperationException($"Could not find parameterless constructor for loader type \"{type}\"");

            var loader = (IAssetLoader)constructor.Invoke(Array.Empty<object>());

            if (loader is IComponent component)
                AttachChild(component);

            _loaders[loaderName] = loader;
            return loader;
        }
    }
}
