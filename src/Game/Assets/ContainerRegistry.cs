using System;
using System.Collections.Generic;
using System.IO;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Containers;

namespace UAlbion.Game.Assets;

public class ContainerRegistry : ServiceComponent<IContainerRegistry>, IContainerRegistry
{
    readonly object _syncRoot = new();
    readonly Dictionary<Type, IAssetContainer> _containers = new();

    public IAssetContainer GetContainer(string path, Type container, IFileSystem disk)
    {
        ArgumentNullException.ThrowIfNull(disk);
        if (container != null)
            return GetContainer(container);

        switch (Path.GetExtension(path).ToUpperInvariant())
        {
            case ".XLD" : return GetContainer(typeof(XldContainer));
            case ".ZIP" : return GetContainer(typeof(ZipContainer));
            default:
                return disk.DirectoryExists(path) 
                    ? GetContainer(typeof(DirectoryContainer)) 
                    : null;
        }
    }

    IAssetContainer GetContainer(Type type)
    {
        if (type == null)
            throw new InvalidOperationException($"Could not find container type \"{type}\"");

        lock (_syncRoot)
            return _containers.TryGetValue(type, out var container) ? container : Instantiate(type);
    }

    IAssetContainer Instantiate(Type type)
    {
        var constructor = type.GetConstructor(Array.Empty<Type>());
        if (constructor == null)
            throw new InvalidOperationException($"Could not find parameterless constructor for container type \"{type}\"");

        var container = (IAssetContainer)constructor.Invoke(Array.Empty<object>());

        // Can uncomment if we ever end up needing this
        // if (container is IComponent component)
        //     AttachChild(component);

        _containers[type] = container;
        return container;
    }
}