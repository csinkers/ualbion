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
        public ISerializer Read(string file, AssetInfo info, IFileSystem disk)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            if (disk == null) throw new ArgumentNullException(nameof(disk));
            var stream = disk.OpenRead(file);
            var br = new BinaryReader(stream);
            stream.Position = info.Index * ItemData.SizeOnDisk;
            return new AlbionReader(br, ItemData.SizeOnDisk);
        }

        public void Write(string path, IList<(AssetInfo, byte[])> assets, IFileSystem disk)
        {
            if (disk == null) throw new ArgumentNullException(nameof(disk));

            var dir = Path.GetDirectoryName(path);
            if (!disk.DirectoryExists(dir))
                disk.CreateDirectory(dir);

            using var fs = disk.OpenWriteTruncate(path);
            using var bw = new BinaryWriter(fs);
            foreach (var (info, bytes) in assets.OrderBy(x => x.Item1.Index))
            {
                ApiUtil.Assert(bytes.Length == ItemData.SizeOnDisk,
                    $"Expected item data for {info.AssetId} to be {ItemData.SizeOnDisk} bytes, but was {bytes.Length}");
                bw.Write(bytes);
            }
        }

        public List<(int, int)> GetSubItemRanges(string path, AssetFileInfo info, IFileSystem disk)
        {
            if (disk == null) throw new ArgumentNullException(nameof(disk));
            if (!disk.FileExists(path))
                return new List<(int, int)>();

            using var f = disk.OpenRead(path);
            return new List<(int, int)> { (0, (int)f.Length / ItemData.SizeOnDisk) };
        }
    }
}