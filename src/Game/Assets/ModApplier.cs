using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats.Assets.Save;

namespace UAlbion.Game.Assets
{
    public class ModApplier : Component, IModApplier
    {
        IAssetLocatorRegistry _assetLocatorRegistry;
        protected override void Subscribed()
        {
            _assetLocatorRegistry = Resolve<IAssetLocatorRegistry>();
            Exchange.Register<IModApplier>(this);
        }

        public AssetInfo GetAssetInfo(AssetId key)
        {
            throw new System.NotImplementedException();
        }

        public object LoadAsset(AssetId id)
        {
            throw new System.NotImplementedException();
        }

        public object LoadAssetCached(AssetId assetId)
        {
            throw new System.NotImplementedException();
        }

        public SavedGame LoadSavedGame(string path)
        {
            throw new System.NotImplementedException();
        }
    }
}