using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SerdesNet;
using UAlbion.Config;

namespace UAlbion.Formats.Containers
{
    /// <summary>
    /// Read chunks from a binary file using offsets & lengths specified in the assets.json file.
    /// </summary>
    public class BinaryOffsetContainerLoader : IContainerLoader
    {
        public ISerializer Open(string file, AssetInfo info)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            using var stream = File.OpenRead(file);
            using var br = new BinaryReader(stream);
            stream.Position = info.Get("Offset", 0);
            var bytes = br.ReadBytes(info.Width * info.Height);
            var ms = new MemoryStream(bytes);
            return new AlbionReader(new BinaryReader(ms));
        }

        public List<(int, int)> GetSubItemRanges(string path, AssetFileInfo info) // All sub-items must be given explicitly for binary offset containers
            => FormatUtil.SortedIntsToRanges(info?.Map.Keys.OrderBy(x => x));
    }
}
