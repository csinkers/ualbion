using System.Collections.Generic;
using SerdesNet;
using UAlbion.Config;

namespace UAlbion.Formats.Containers
{
    [ContainerLoader(ContainerFormat.Zip)]
    public class ZipContainer : IContainerLoader
    {
        public ISerializer Open(string file, AssetInfo info)
        {
            throw new System.NotImplementedException();
        }

        public List<(int, int)> GetSubItemRanges(string path, AssetFileInfo info)
        {
            throw new System.NotImplementedException();
        }
    }
}