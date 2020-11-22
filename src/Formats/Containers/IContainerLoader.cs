using SerdesNet;
using UAlbion.Config;

namespace UAlbion.Formats.Containers
{
    public interface IContainerLoader
    {
        ISerializer Open(string file, AssetInfo info);
    }
}