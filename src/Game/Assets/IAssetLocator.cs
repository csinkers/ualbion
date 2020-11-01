using System.Collections.Generic;
using UAlbion.Config;

namespace UAlbion.Game.Assets
{
    public interface IAssetLocator
    {
        object LoadAsset(AssetId key, SerializationContext context, AssetInfo info);
        IEnumerable<AssetType> SupportedTypes { get; }
    }
}
