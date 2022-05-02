using System;
using System.Collections.Generic;
using System.IO;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Formats.Containers;

namespace UAlbion.Game.Assets;

public class ContainerRegistry : ServiceComponent<IContainerRegistry>, IContainerRegistry
{
    readonly object _syncRoot = new();
    readonly IDictionary<Type, IAssetContainer> _containers = new Dictionary<Type, IAssetContainer>();

    public IAssetContainer GetContainer(string path, string container, IFileSystem disk)
    {
        if (disk == null) throw new ArgumentNullException(nameof(disk));
        if (!string.IsNullOrEmpty(container))
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

    IAssetContainer GetContainer(string containerName)
    {
        if (string.IsNullOrEmpty(containerName))
            throw new ArgumentNullException(nameof(containerName));

        var type = Type.GetType(containerName);
        if (type == null)
            throw new InvalidOperationException($"Could not find container type \"{containerName}\"");

        lock (_syncRoot)
            return _containers.TryGetValue(type, out var container) ? container : Instantiate(type);
    }

    IAssetContainer GetContainer(Type containerType)
    {
        if (containerType == null) throw new ArgumentNullException(nameof(containerType));
        lock (_syncRoot)
            return _containers.TryGetValue(containerType, out var container) ? container : Instantiate(containerType);
    }

    IAssetContainer Instantiate(Type type)
    {
        var constructor = type.GetConstructor(Array.Empty<Type>());
        if (constructor == null)
            throw new InvalidOperationException($"Could not find parameterless constructor for container type \"{type}\"");

        var container = (IAssetContainer)constructor.Invoke(Array.Empty<object>());

        if (container is IComponent component)
            AttachChild(component);

        _containers[type] = container;
        return container;
    }
}