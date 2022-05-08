using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;

namespace UAlbion.Formats.Containers;

/// <summary>
/// Simple file containing a single asset.
/// </summary>
public class RawContainer : IAssetContainer
{
    public ISerializer Read(string file, AssetInfo info, SerdesContext context)
    {
        if (info == null) throw new ArgumentNullException(nameof(info));
        if (context == null) throw new ArgumentNullException(nameof(context));
        ApiUtil.Assert(info.Index == 0, "SubItem should always be 0 when accessing a non-container file");

        if (!context.Disk.FileExists(file))
            return null;

        var stream = context.Disk.OpenRead(file);
        var br = new BinaryReader(stream);
        return new AlbionReader(br);
    }

    public void Write(string path, IList<(AssetInfo, byte[])> assets, SerdesContext context)
    {
        if (assets == null) throw new ArgumentNullException(nameof(assets));
        if (context == null) throw new ArgumentNullException(nameof(context));

        var dir = Path.GetDirectoryName(path);
        if (!context.Disk.DirectoryExists(dir))
            context.Disk.CreateDirectory(dir);

        if (assets.Count == 0)
        {
            if (context.Disk.FileExists(path))
                context.Disk.DeleteFile(path);
            return;
        }

        if (assets.Count > 1) throw new ArgumentOutOfRangeException(nameof(assets), "A RawContainer can only hold a single asset");

        var (_, bytes) = assets.Single();
        context.Disk.WriteAllBytes(path, bytes);
    }

    public List<(int, int)> GetSubItemRanges(string path, AssetFileInfo info, SerdesContext context)
        => new() { (0, 1) };
}