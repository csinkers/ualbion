using System;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.Assets.Save;
using UAlbion.Game.Assets;

namespace UAlbion.Game.Tests
{
    public class MockModApplier : ServiceComponent<IModApplier>, IModApplier
    {
        IAssetLocatorRegistry _registry;
        readonly AssetMapping _mapping;

        public MockModApplier(AssetMapping mapping, GameLanguage language)
        {
            _mapping = mapping;
            Language = language;
        }

        public GameLanguage Language { get; set; }

        protected override void Subscribed()
        {
            _registry ??= Resolve<IAssetLocatorRegistry>()
                          ??  throw new InvalidOperationException(
                              $"{nameof(MockModApplier)} is missing requirement of type {nameof(IAssetLocatorRegistry)}");
            base.Subscribed();
        }

        public AssetInfo GetAssetInfo(AssetId key) => _registry.GetAssetInfo(key);
        public object LoadAsset(AssetId id) => _registry.LoadAsset(id, new SerializationContext(_mapping, Language));
        public object LoadAssetCached(AssetId assetId) => _registry.LoadAssetCached(assetId, new SerializationContext(_mapping, Language));
        public SavedGame LoadSavedGame(string path) => throw new NotImplementedException();
    }
}