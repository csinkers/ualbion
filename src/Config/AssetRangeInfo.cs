using System.Collections.Generic;

namespace UAlbion.Config;

public class AssetRangeInfo
{
    public AssetRange Range { get; }
    public AssetNode Node { get; }
    public List<AssetFileInfo> Files { get; } = new();
    public AssetRangeInfo(AssetRange range)
    {
        Range = range;
        Node = new AssetNode(range.From, null);
    }

    public override string ToString() => $"{Range} ({Files.Count} files)";
}