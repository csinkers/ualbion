using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Containers;

namespace UAlbion.Game.Assets
{
    public interface IContainerLoaderRegistry
    {
        IContainerLoader GetLoader(ContainerFormat type);
        ISerializer Load(string filename, AssetInfo info, ContainerFormat format);
    }
}