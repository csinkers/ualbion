using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;

namespace UAlbion.Formats.Containers;

/// <summary>
/// Read chunks from a binary file using offsets &amp; lengths specified in the assets.json file.
/// </summary>
public class BinaryOffsetContainer : IAssetContainer
{
    public ISerializer Read(string file, AssetInfo info, SerdesContext context)
    {
        if (info == null) throw new ArgumentNullException(nameof(info));
        if (context == null) throw new ArgumentNullException(nameof(context));

        using var stream = context.Disk.OpenRead(file);
        using var br = new BinaryReader(stream);
        stream.Position = info.Get(AssetProperty.Offset, 0);
        var bytes = br.ReadBytes(info.Width * info.Height);
        var ms = new MemoryStream(bytes);
        return new AlbionReader(new BinaryReader(ms));
    }

    public void Write(string path, IList<(AssetInfo, byte[])> assets, SerdesContext context)
        => ApiUtil.Assert("Binary offset containers do not currently support saving");

    public List<(int, int)> GetSubItemRanges(string path, AssetFileInfo info, SerdesContext context) // All sub-items must be given explicitly for binary offset containers
        => FormatUtil.SortedIntsToRanges(info?.Map.Keys.OrderBy(x => x));
}