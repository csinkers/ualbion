using System.Collections.Generic;
using System.Linq;
using SerdesNet;
using UAlbion.Config;

namespace UAlbion.Formats.Containers;

public class DummyContainer : IAssetContainer
{
    static readonly EmptySerializer Empty = new();
    public ISerializer Read(string path, AssetInfo info, SerdesContext context) => Empty;
    public void Write(string path, IList<(AssetInfo, byte[])> assets, SerdesContext context) { }
    public List<(int num, int count)> GetSubItemRanges(string path, AssetFileInfo info, SerdesContext context) 
        => FormatUtil.SortedIntsToRanges(info.Map.Keys.OrderBy(x => x));
}