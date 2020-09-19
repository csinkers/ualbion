using System.Collections.Generic;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Assets
{
    public interface IRawAssetManager : IAssetManager
    {
        void Save(AssetKey key, object asset);
        IEnumerable<AssetKey> EnumerateAssets(AssetType type);
    }
}