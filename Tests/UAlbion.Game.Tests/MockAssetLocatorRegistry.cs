using System.Collections.Generic;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Assets;

namespace UAlbion.TestCommon
{
    public class MockAssetLocatorRegistry : ServiceComponent<IAssetLocatorRegistry>, IAssetLocatorRegistry
    {
        readonly Dictionary<AssetKey, object> _assets = new Dictionary<AssetKey, object>();
        public void Dispose() { }
        public IAssetLocatorRegistry AddAssetLocator(IAssetLocator locator) => this;
        public IAssetLocatorRegistry AddAssetPostProcessor(IAssetPostProcessor postProcessor) => this;
        public object LoadAsset(AssetKey key) => _assets[key];
        public object LoadAssetCached(AssetKey key) => _assets[key];
        public MockAssetLocatorRegistry Add(AssetKey key, object asset) { _assets[key] = asset; return this; }
    }
}