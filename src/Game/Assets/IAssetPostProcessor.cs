using UAlbion.Config;

namespace UAlbion.Game.Assets;

public interface IAssetPostProcessor
{
    object Process(object asset, AssetInfo info);
}