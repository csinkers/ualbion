using UAlbion.Config;
using UAlbion.Formats.Containers;

namespace UAlbion.Game.Assets
{
    public interface IContainerLoaderRegistry
    {
        IContainerLoader GetLoader(ContainerFormat type);
    }
}