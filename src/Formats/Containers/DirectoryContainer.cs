using SerdesNet;
using UAlbion.Config;

namespace UAlbion.Formats.Containers
{
    [ContainerLoader(ContainerFormat.Directory)]
    public class DirectoryContainer : IContainerLoader
    {
        public ISerializer Open(string file, AssetInfo info)
        {
            throw new System.NotImplementedException();
        }
    }
}