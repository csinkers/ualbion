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
/// 5 bytes per spell, 30 spells per class, 7 classes. No header.
/// </summary>
public class SpellListContainer : IAssetContainer
{
    static readonly byte[] Blank = { 0, 0, 0, 0, 0 };
    public ISerializer Read(string path, AssetLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var stream = context.Disk.OpenRead(path);
        var br = new BinaryReader(stream);
        stream.Position = context.Index * SpellData.SizeOnDisk;
        return new AlbionReader(br, SpellData.SizeOnDisk);
    }

    public void Write(string path, IList<(AssetLoadContext, byte[])> assets, ModContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var dir = Path.GetDirectoryName(path);
        if (!context.Disk.DirectoryExists(dir))
            context.Disk.CreateDirectory(dir);

        using var fs = context.Disk.OpenWriteTruncate(path);
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
}