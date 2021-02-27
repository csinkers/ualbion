using System;
using System.Collections.Generic;
using System.IO;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;

namespace UAlbion.Formats.Containers
{
    /// <summary>
    /// Simple file containing a single asset.
    /// </summary>
    public class RawContainerLoader : IContainerLoader
    {
        public ISerializer Open(string file, AssetInfo info)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            ApiUtil.Assert(info.SubAssetId == 0, "SubItem should always be 0 when accessing a non-container file");
            var stream = File.OpenRead(file);
            var br = new BinaryReader(stream);
            return new AlbionReader(br);
        }

        public List<(int, int)> GetSubItemRanges(string path, AssetFileInfo info) => new List<(int, int)> { (0, 1) };
    }
}
