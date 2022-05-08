using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Containers;

/// <summary>
/// 0x3A bytes per item, no header.
/// </summary>
public class ItemListContainer : IAssetContainer
{
    public ISerializer Read(string file, AssetInfo info, SerdesContext context)
    {
        if (info == null) throw new ArgumentNullException(nameof(info));
        if (context == null) throw new ArgumentNullException(nameof(context));
        var stream = context.Disk.OpenRead(file);
        var br = new BinaryReader(stream);
        stream.Position = info.Index * ItemData.SizeOnDisk;
        return new AlbionReader(br, ItemData.SizeOnDisk);
    }

    public void Write(string path, IList<(AssetInfo, byte[])> assets, SerdesContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var dir = Path.GetDirectoryName(path);
        if (!context.Disk.DirectoryExists(dir))
            context.Disk.CreateDirectory(dir);

        using var fs = context.Disk.OpenWriteTruncate(path);
        using var bw = new BinaryWriter(fs);
        foreach (var (info, bytes) in assets.OrderBy(x => x.Item1.Index))
        {
            ApiUtil.Assert(bytes.Length == ItemData.SizeOnDisk,
                $"Expected item data for {info.AssetId} to be {ItemData.SizeOnDisk} bytes, but was {bytes.Length}");
            bw.Write(bytes);
        }
    }

    public List<(int, int)> GetSubItemRanges(string path, AssetFileInfo info, SerdesContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        if (!context.Disk.FileExists(path))
            return new List<(int, int)>();

        using var f = context.Disk.OpenRead(path);
        return new List<(int, int)> { (0, (int)f.Length / ItemData.SizeOnDisk) };
    }
}