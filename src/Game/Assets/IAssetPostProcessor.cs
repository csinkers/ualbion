using UAlbion.Config;
using UAlbion.Core;

namespace UAlbion.Game.Assets
{
    public interface IAssetPostProcessor
    {
        object Process(object asset, AssetInfo info, ICoreFactory factory);
    }
}
