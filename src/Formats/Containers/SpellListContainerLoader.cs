using SerdesNet;
using UAlbion.Config;

namespace UAlbion.Formats.Containers
{
    [ContainerLoader(ContainerFormat.SpellList)]
    public class SpellListContainerLoader : IContainerLoader
    {
        public ISerializer Open(string file, AssetInfo info)
        {
            throw new System.NotImplementedException();
        }
    }
}