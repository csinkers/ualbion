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
    public ISerdes Read(string path, AssetLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var stream = context.Disk.OpenRead(path);
        stream.Position = context.Index * ItemData.SizeOnDisk;
        return AlbionSerdes.CreateReader(stream, ItemData.SizeOnDisk);
    }

    public void Write(string path, IList<(AssetLoadContext, ReadOnlyMemory<byte>)> assets, ModContext context)
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
            bw.Write(bytes.Span);
        }
    }
}