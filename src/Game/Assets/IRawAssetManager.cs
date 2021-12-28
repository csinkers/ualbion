using System.Collections.Generic;
using UAlbion.Config;

namespace UAlbion.Game.Assets;

public interface IRawAssetManager : IAssetManager
{
    void Save(AssetId key, object asset);
    IEnumerable<AssetId> EnumerateAssets(AssetType type);
}