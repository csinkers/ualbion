using System.Collections.Generic;
using UAlbion.Config;
using UAlbion.Formats;

namespace UAlbion.Game.Assets;

public interface IRawAssetManager : IAssetManager
{
    void Save(AssetId key, object asset);
    IEnumerable<AssetId> EnumerateAssets(AssetType type);
}