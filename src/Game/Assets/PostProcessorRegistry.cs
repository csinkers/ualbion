using System;
using System.Collections.Generic;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats;

namespace UAlbion.Game.Assets;

public class PostProcessorRegistry : ServiceComponent<IAssetPostProcessorRegistry>, IAssetPostProcessorRegistry
{
    readonly object _syncRoot = new();
    readonly Dictionary<Type, IAssetPostProcessor> _loaders = new();

    public IAssetPostProcessor GetPostProcessor(Type postProcessorType)
    {
        ArgumentNullException.ThrowIfNull(postProcessorType);

        lock (_syncRoot)
            return _loaders.TryGetValue(postProcessorType, out var postProcessor) ? postProcessor : Instantiate(postProcessorType);
    }

    IAssetPostProcessor Instantiate(Type type)
    {
        var constructor = type.GetConstructor(Array.Empty<Type>());
        if(constructor == null)
            throw new InvalidOperationException($"Could not find parameterless constructor for post-processor type \"{type}\"");

        var postProcessor = (IAssetPostProcessor)constructor.Invoke(Array.Empty<object>());

        if (postProcessor is IComponent component)
            AttachChild(component);

        _loaders[type] = postProcessor;
        return postProcessor;
    }
}