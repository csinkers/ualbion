using System;
using System.Collections.Generic;
using System.Reflection;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.Containers;

namespace UAlbion.Game.Assets
{
    public class ContainerLoaderRegistry : ServiceComponent<IContainerLoaderRegistry>, IContainerLoaderRegistry
    {
        readonly IDictionary<ContainerFormat, IContainerLoader> _loaders = new Dictionary<ContainerFormat, IContainerLoader>();

        public IContainerLoader GetLoader(ContainerFormat type) => _loaders[type];
        public ISerializer Load(string filename, AssetInfo info, ContainerFormat format) => GetLoader(format).Open(filename, info);

        public ContainerLoaderRegistry AddLoader(IContainerLoader loader)
        {
            if (loader == null) throw new ArgumentNullException(nameof(loader));
            var attribute = (ContainerLoaderAttribute)loader.GetType().GetCustomAttribute(typeof(ContainerLoaderAttribute), false);
            if (attribute != null)
                foreach (var format in attribute.SupportedFormats)
                    _loaders.Add(format, loader);

            if (loader is IComponent component)
                AttachChild(component);
            return this;
        }
    }
}