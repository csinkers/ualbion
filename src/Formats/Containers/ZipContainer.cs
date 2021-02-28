using System.Collections.Generic;
using SerdesNet;
using UAlbion.Config;

namespace UAlbion.Formats.Containers
{
    /// <summary>
    /// Zip compressed archive, sub-assets named 0_Foo, 1, 2_Bar etc (anything after an underscore is ignored when loading)
    /// </summary>
    public class ZipContainer : IAssetContainer
    {
        public ISerializer Read(string file, AssetInfo info)
        {
            throw new System.NotImplementedException();
        }

        public void Write(string path, IList<(AssetInfo, byte[])> assets)
        {
            throw new System.NotImplementedException();
        }

        public List<(int, int)> GetSubItemRanges(string path, AssetFileInfo info)
        {
            throw new System.NotImplementedException();
        }
    }
}