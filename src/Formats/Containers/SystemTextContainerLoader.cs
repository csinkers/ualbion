using SerdesNet;
using UAlbion.Config;

namespace UAlbion.Formats.Containers
{
    [ContainerLoader(ContainerFormat.SystemTextList)]
    public class SystemTextContainerLoader : IContainerLoader
    {
        public ISerializer Open(string file, AssetInfo info)
        {
            throw new System.NotImplementedException();
        }
    }
}