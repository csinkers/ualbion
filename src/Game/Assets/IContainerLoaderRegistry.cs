using System;
using UAlbion.Formats.Containers;

namespace UAlbion.Game.Assets
{
    public interface IContainerLoaderRegistry
    {
        IContainerLoader GetLoader(string containerName);
        IContainerLoader GetLoader(Type containerType);
    }
}