namespace UAlbion.Config;

public interface IAssetPostProcessor
{
    object Process(object asset, AssetLoadContext context);
}