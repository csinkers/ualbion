using UAlbion.Config;

namespace UAlbion.Formats;

public interface IAssetPostProcessor
{
    object Process(object asset, AssetInfo info);
}