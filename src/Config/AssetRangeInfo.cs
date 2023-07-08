using System.Collections.Generic;

namespace UAlbion.Config;

public class AssetRangeInfo
{
    public AssetRange Range { get; }
    public AssetNode Node { get; }
    public List<AssetFileInfo> Files { get; } = new();

    public IEnumerable<AssetId> Assets
    {
        get
        {
            var id = Range.From;
            while (id <= Range.To)
            {
                yield return id;
                id = new AssetId(id.Type, id.Id + 1);
            }
        }
    }

    public AssetRangeInfo(AssetRange range)
    {
        Range = range;
        Node = new AssetNode(range.From, null);
    }

    public override string ToString() => $"{Range} ({Files.Count} files)";
}