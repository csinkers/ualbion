using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets.Inv;

namespace UAlbion.Formats.Containers;

/// <summary>
/// 0x3A bytes per item, no header.
/// </summary>
public class ItemListContainer : IAssetContainer
{
    public ISerializer Read(string path, AssetLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var stream = context.Disk.OpenRead(path);
        var br = new BinaryReader(stream);
        stream.Position = context.Index * ItemData.SizeOnDisk;
        return new AlbionReader(br, ItemData.SizeOnDisk);
    }

    public void Write(string path, IList<(AssetLoadContext, byte[])> assets, ModContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

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
}