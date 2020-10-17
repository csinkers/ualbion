using System;
using System.Collections.Generic;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Game.Assets;

namespace UAlbion.Game.Tests
{
    public class MockAssetLocatorRegistry : ServiceComponent<IAssetLocatorRegistry>, IAssetLocatorRegistry
    {
        readonly Dictionary<AssetId, object> _assets = new Dictionary<AssetId, object>();
        public IAssetLocatorRegistry AddAssetLocator(IAssetLocator locator, bool useAsDefault) => this;
        public IAssetLocatorRegistry AddAssetPostProcessor(IAssetPostProcessor postProcessor) => this;
        public object LoadAsset(AssetId key, SerializationContext context) => _assets[key];
        public object LoadAssetCached(AssetId key, SerializationContext context) => _assets[key];
        public object LoadAsset<T>(T id, SerializationContext context) where T : unmanaged, Enum => LoadAsset(AssetId.From(id), context);
        public object LoadAssetCached<T>(T id, SerializationContext context) where T : unmanaged, Enum => LoadAsset(AssetId.From(id), context);
        public AssetInfo GetAssetInfo(AssetId key) => null;
        public MockAssetLocatorRegistry Add(AssetId key, object asset) { _assets[key] = asset; return this; }
    }
}
