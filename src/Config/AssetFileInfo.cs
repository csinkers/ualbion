using System;
using System.Collections.Generic;

namespace UAlbion.Config;

public class AssetFileInfo
{
    public AssetRangeInfo Range { get; }
    public AssetNode Node { get; }

    public AssetFileInfo(AssetRangeInfo range)
    {
        Range = range ?? throw new ArgumentNullException(nameof(range));
        Node = new AssetNode(range.Node);
    }

    public Dictionary<AssetId, AssetInfo> Map { get; } = new();

    public override string ToString() => $"AssetFile: {Node.Filename}{(string.IsNullOrEmpty(Node.Sha256Hash) ? "" : $"#{Node.Sha256Hash}")}";
}