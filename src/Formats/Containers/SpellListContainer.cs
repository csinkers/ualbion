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
        public ISerializer Read(string file, AssetInfo info)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            var stream = File.OpenRead(file);
            var br = new BinaryReader(stream);
            stream.Position = info.SubAssetId * SpellData.SizeOnDisk;
            return new AlbionReader(br, SpellData.SizeOnDisk);
        }

        public void Write(string path, IList<(AssetInfo, byte[])> assets)
        {
            using var fs = File.OpenWrite(path);
            using var bw = new BinaryWriter(fs);
            foreach (var (info, bytes) in assets.OrderBy(x => x.Item1.SubAssetId))
            {
                ApiUtil.Assert(bytes.Length == SpellData.SizeOnDisk,
                    $"Expected spell data for {info.AssetId} to be {SpellData.SizeOnDisk} bytes, but was {bytes.Length}");
                bw.Write(bytes);
            }
        }

        public List<(int, int)> GetSubItemRanges(string path, AssetFileInfo info)
        {
            using var f = File.OpenRead(path);
            return new List<(int, int)> { (0, (int)f.Length / SpellData.SizeOnDisk) };
        }
    }
}