﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UAlbion.Api.Eventing;
using UAlbion.Config;

namespace UAlbion.Game.Assets;

public sealed class AssetLoaderRegistry : ServiceComponent<IAssetLoaderRegistry>, IAssetLoaderRegistry, IDisposable
{
    readonly Lock _syncRoot = new();
    readonly Dictionary<Type, IAssetLoader> _loaders = [];

    public IAssetLoader GetLoader(Type loaderType)
    {
        lock (_syncRoot)
            return _loaders.TryGetValue(loaderType, out var loader) ? loader : Instantiate(loaderType);
    }

    IAssetLoader Instantiate(Type loaderType)
    {
        ArgumentNullException.ThrowIfNull(loaderType);

        var constructor = loaderType.GetConstructor([]);
        if(constructor == null)
            throw new InvalidOperationException($"Could not find parameterless constructor for loader type \"{loaderType}\"");

        var loader = (IAssetLoader)constructor.Invoke([]);

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
