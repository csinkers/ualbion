using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Api.Eventing;
using UAlbion.Config;

namespace UAlbion.Game.Assets;

public sealed class AssetLoaderRegistry : ServiceComponent<IAssetLoaderRegistry>, IAssetLoaderRegistry, IDisposable
{
    readonly object _syncRoot = new();
    readonly IDictionary<Type, IAssetLoader> _loaders = new Dictionary<Type, IAssetLoader>();

    public IAssetLoader GetLoader(Type loaderType)
    {
        lock (_syncRoot)
            return _loaders.TryGetValue(loaderType, out var loader) ? loader : Instantiate(loaderType);
    }

    IAssetLoader Instantiate(Type loaderType)
    {
        if (loaderType == null) throw new ArgumentNullException(nameof(loaderType));

        var constructor = loaderType.GetConstructor(Array.Empty<Type>());
        if(constructor == null)
            throw new InvalidOperationException($"Could not find parameterless constructor for loader type \"{loaderType}\"");

        var loader = (IAssetLoader)constructor.Invoke(Array.Empty<object>());

        if (loader is IComponent component)
            AttachChild(component);

        _loaders[loaderType] = loader;
        return loader;
    }

    public void Dispose()
    {
        foreach(var loader in _loaders.Values.OfType<IDisposable>())
            loader.Dispose();
        _loaders.Clear();
    }
}
