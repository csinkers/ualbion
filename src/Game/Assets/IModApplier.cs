using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats.Assets.Save;

namespace UAlbion.Game.Assets
{
    public interface IModApplier : IComponent
    {
        IModApplier AddAssetPostProcessor(IAssetPostProcessor postProcessor);
        AssetInfo GetAssetInfo(AssetId key);
        object LoadAsset(AssetId id);
        object LoadAssetCached(AssetId assetId);
        SavedGame LoadSavedGame(string path);
    }
}