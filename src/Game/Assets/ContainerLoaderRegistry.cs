using System;
using System.Collections.Generic;
using UAlbion.Core;
using UAlbion.Formats.Containers;

namespace UAlbion.Game.Assets
{
    public class ContainerLoaderRegistry : ServiceComponent<IContainerLoaderRegistry>, IContainerLoaderRegistry
    {
        readonly object _syncRoot = new object();
        readonly IDictionary<Type, IContainerLoader> _loaders = new Dictionary<Type, IContainerLoader>();

        public IContainerLoader GetLoader(string loaderName)
        {
            if (string.IsNullOrEmpty(loaderName))
                throw new ArgumentNullException(nameof(loaderName));

            var type = Type.GetType(loaderName);
            if (type == null)
                throw new InvalidOperationException($"Could not find container type \"{loaderName}\"");

            lock (_syncRoot)
                return _loaders.TryGetValue(type, out var loader) ? loader : Instantiate(type);
        }

        public IContainerLoader GetLoader(Type containerType)
        {
            if (containerType == null) throw new ArgumentNullException(nameof(containerType));
            lock (_syncRoot)
                return _loaders.TryGetValue(containerType, out var loader) ? loader : Instantiate(containerType);
        }

        IContainerLoader Instantiate(Type type)
        {
            var constructor = type.GetConstructor(Array.Empty<Type>());
            if (constructor == null)
                throw new InvalidOperationException($"Could not find parameterless constructor for container type \"{type}\"");

            var loader = (IContainerLoader)constructor.Invoke(Array.Empty<object>());

            if (loader is IComponent component)
                AttachChild(component);

            _loaders[type] = loader;
            return loader;
        }
    }
}