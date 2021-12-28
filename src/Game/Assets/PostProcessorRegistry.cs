using System;
using System.Collections.Generic;
using UAlbion.Core;

namespace UAlbion.Game.Assets;

public class PostProcessorRegistry : ServiceComponent<IAssetPostProcessorRegistry>, IAssetPostProcessorRegistry
{
    readonly object _syncRoot = new();
    readonly IDictionary<string, IAssetPostProcessor> _loaders = new Dictionary<string, IAssetPostProcessor>();

    public IAssetPostProcessor GetPostProcessor(string postProcessorName)
    {
        lock (_syncRoot)
            return _loaders.TryGetValue(postProcessorName, out var postProcessor) ? postProcessor : Instantiate(postProcessorName);
    }

    IAssetPostProcessor Instantiate(string postProcessorName)
    {
        if(string.IsNullOrEmpty(postProcessorName))
            throw new ArgumentNullException(nameof(postProcessorName));

        var type = Type.GetType(postProcessorName);
        if(type == null)
            throw new InvalidOperationException($"Could not find post-processor type \"{postProcessorName}\"");

        var constructor = type.GetConstructor(Array.Empty<Type>());
        if(constructor == null)
            throw new InvalidOperationException($"Could not find parameterless constructor for post-processor type \"{type}\"");

        var postProcessor = (IAssetPostProcessor)constructor.Invoke(Array.Empty<object>());

        if (postProcessor is IComponent component)
            AttachChild(component);

        _loaders[postProcessorName] = postProcessor;
        return postProcessor;
    }
}