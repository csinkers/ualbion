using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;

namespace UAlbion.Formats.Containers;

public class DummyContainer : IAssetContainer
{
    public ISerializer Read(string path, AssetInfo info, SerdesContext context)
    {
        var ms = new MemoryStream(Array.Empty<byte>());
        var br = new BinaryReader(ms);
        return new AlbionReader(br, 1, () => { br.Dispose(); ms.Dispose(); });
    }

    public void Write(string path, IList<(AssetInfo, byte[])> assets, SerdesContext context) { }
    public List<(int num, int count)> GetSubItemRanges(string path, AssetFileInfo info, SerdesContext context) 
        => FormatUtil.SortedIntsToRanges(info.Map.Keys.OrderBy(x => x));
}