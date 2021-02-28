using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;

namespace UAlbion.Formats.Containers
{
    /// <summary>
    /// Simple file containing a single asset.
    /// </summary>
    public class RawContainer : IAssetContainer
    {
        public ISerializer Read(string file, AssetInfo info)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            ApiUtil.Assert(info.SubAssetId == 0, "SubItem should always be 0 when accessing a non-container file");
            var stream = File.OpenRead(file);
            var br = new BinaryReader(stream);
            return new AlbionReader(br);
        }

        public void Write(string path, IList<(AssetInfo, byte[])> assets)
        {
            if (assets == null) throw new ArgumentNullException(nameof(assets));
            if(assets.Count != 1)
                throw new ArgumentOutOfRangeException(nameof(assets), "A RawContainer can only hold a single asset");

            var (_, bytes) = assets.Single();
            File.WriteAllBytes(path, bytes);
        }

        public List<(int, int)> GetSubItemRanges(string path, AssetFileInfo info) => new List<(int, int)> { (0, 1) };
    }
}
