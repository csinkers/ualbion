using UAlbion.Formats;

namespace UAlbion.Game.Assets
{
    public interface IAssetLoaderRegistry
    {
        IAssetLoader GetLoader(string loaderName);
    }
}
