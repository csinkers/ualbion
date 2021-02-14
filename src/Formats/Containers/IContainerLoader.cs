using System.Collections.Generic;
using SerdesNet;
using UAlbion.Config;

namespace UAlbion.Formats.Containers
{
    public interface IContainerLoader
    {
        ISerializer Open(string path, AssetInfo info);
        public List<(int, int)> GetSubItemRanges(string path, AssetFileInfo info); // pairs = (subItemId, count)
    }
}