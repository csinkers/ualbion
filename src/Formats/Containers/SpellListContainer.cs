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
    /// 5 bytes per spell, 30 spells per class, 7 classes. No header.
    /// </summary>
    public class SpellListContainer : IAssetContainer
    {
        static readonly byte[] Blank = { 0, 0, 0, 0, 0 };
        public ISerializer Read(string file, AssetInfo info, IFileSystem disk)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            if (disk == null) throw new ArgumentNullException(nameof(disk));
            var stream = disk.OpenRead(file);
            var br = new BinaryReader(stream);
            stream.Position = info.Index * SpellData.SizeOnDisk;
            return new AlbionReader(br, SpellData.SizeOnDisk);
        }

        public void Write(string path, IList<(AssetInfo, byte[])> assets, IFileSystem disk)
        {
            if (disk == null) throw new ArgumentNullException(nameof(disk));

            var dir = Path.GetDirectoryName(path);
            if (!disk.DirectoryExists(dir))
                disk.CreateDirectory(dir);

            using var fs = disk.OpenWriteTruncate(path);
            using var bw = new BinaryWriter(fs);

            var dict = assets.ToDictionary(x => x.Item1.Index, x => x.Item2);
            var maxId = dict.Keys.Max();

            for(int i = 0; i <= maxId; i++)
            {
                if (!dict.TryGetValue(i, out var bytes))
                    bytes = Blank;

                ApiUtil.Assert(bytes.Length == SpellData.SizeOnDisk,
                    $"Expected spell data for entry {i} to be {SpellData.SizeOnDisk} bytes, but was {bytes.Length}");
                bw.Write(bytes);
            }
        }

        public List<(int, int)> GetSubItemRanges(string path, AssetFileInfo info, IFileSystem disk)
        {
            if (disk == null) throw new ArgumentNullException(nameof(disk));
            if (!disk.FileExists(path))
                return new List<(int, int)>();

            using var f = disk.OpenRead(path);
            return new List<(int, int)> { (0, (int)f.Length / SpellData.SizeOnDisk) };
        }
    }
}