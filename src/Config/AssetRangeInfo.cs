using System.Collections.Generic;

namespace UAlbion.Config;

public class AssetRangeInfo
{
    public AssetRange Range { get; }
    public AssetNode Node { get; }
    public int Sequence { get; }
    public List<AssetFileInfo> Files { get; } = new();
    public AssetRangeInfo(AssetRange range, int sequence)
    {
        Range = range;
        Sequence = sequence;
        Node = new AssetNode(range.From);
    }

    public override string ToString() => $"{Range} ({Files.Count} files)";
}