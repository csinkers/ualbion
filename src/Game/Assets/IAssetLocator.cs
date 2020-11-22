using UAlbion.Config;
using UAlbion.Core;

namespace UAlbion.Game.Assets
{
    public interface IAssetLocator : IComponent
    {
        object LoadAsset(AssetId key, SerializationContext context, AssetInfo info);
    }
}
