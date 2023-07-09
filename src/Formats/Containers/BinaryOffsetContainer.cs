using System;
using System.Collections.Generic;
using System.IO;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Config.Properties;

namespace UAlbion.Formats.Containers;

/// <summary>
/// Read chunks from a binary file using offsets &amp; lengths specified in the assets.json file.
/// </summary>
public class BinaryOffsetContainer : IAssetContainer
{
    public static readonly IntAssetProperty Offset = new("Offset"); // int, used for BinaryOffsetContainer, e.g. MAIN.EXE
    public static readonly StringAssetProperty Hotspot = new("Hotspot"); // for cursors, formatted like "5 -2"

    public ISerializer Read(string path, AssetLoadContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        using var stream = context.Disk.OpenRead(path);
        using var br = new BinaryReader(stream);
        stream.Position = context.GetProperty(Offset);
        var bytes = br.ReadBytes(context.Node.Width * context.Node.Height);
        var ms = new MemoryStream(bytes);
        return new AlbionReader(new BinaryReader(ms));
    }

    public void Write(string path, IList<(AssetLoadContext, byte[])> assets, ModContext context)
        => ApiUtil.Assert("Binary offset containers do not currently support saving");
}