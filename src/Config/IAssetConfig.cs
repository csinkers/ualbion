using System.Collections.Generic;

namespace UAlbion.Config;

public interface IAssetConfig
{
    IEnumerable<AssetNode> GetAssetInfo(AssetId id);
}