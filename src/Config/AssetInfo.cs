using System;

namespace UAlbion.Config;

public class AssetInfo
{
    public AssetId Id { get; }
    public AssetFileInfo File { get; }
    public AssetNode Node { get; }

    public AssetInfo(AssetId id, AssetFileInfo file)
    {
        Id = id;
        File = file ?? throw new ArgumentNullException(nameof(file));
        Node = new AssetNode(file.Node);
    }
}