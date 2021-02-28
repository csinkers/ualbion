using System;
using UAlbion.Formats.Containers;

namespace UAlbion.Game.Assets
{
    public interface IContainerRegistry
    {
        IAssetContainer GetContainer(string containerName);
        IAssetContainer GetContainer(Type containerType);
    }
}