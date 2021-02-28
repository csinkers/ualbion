using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Containers
{
    /// <summary>
    /// 0x3A bytes per item, no header.
    /// </summary>
    public class ItemListContainer : IAssetContainer
    {
        public ISerializer Read(string file, AssetInfo info)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            var stream = File.OpenRead(file);
            var br = new BinaryReader(stream);
            stream.Position = info.SubAssetId * ItemData.SizeOnDisk;
            return new AlbionReader(br, ItemData.SizeOnDisk);
        }

        public void Write(string path, IList<(AssetInfo, byte[])> assets)
        {
            using var fs = File.OpenWrite(path);
            using var bw = new BinaryWriter(fs);
            foreach (var (info, bytes) in assets.OrderBy(x => x.Item1.SubAssetId))
            {
                ApiUtil.Assert(bytes.Length == ItemData.SizeOnDisk,
                    $"Expected item data for {info.AssetId} to be {ItemData.SizeOnDisk} bytes, but was {bytes.Length}");
                bw.Write(bytes);
            }
        }

        public List<(int, int)> GetSubItemRanges(string path, AssetFileInfo info)
        {
            using var f = File.OpenRead(path);
            return new List<(int, int)> { (0, (int)f.Length / ItemData.SizeOnDisk) };
        }
    }
}