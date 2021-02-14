using System.Collections.Generic;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Game.Assets;

namespace UAlbion.Game.Tests
{
    public class MockAssetLocator : ServiceComponent<IAssetLocator>, IAssetLocator
    {
        readonly Dictionary<AssetId, object> _assets = new Dictionary<AssetId, object>();
        public IAssetLocator AddAssetLocator(IAssetLocator locator, bool useAsDefault) => this;
        public IAssetLocator AddAssetPostProcessor(IAssetPostProcessor postProcessor) => this;
        public MockAssetLocator Add(AssetId key, object asset) { _assets[key] = asset; return this; }
        public object LoadAsset(AssetId id, AssetMapping mapping, AssetInfo info, IDictionary<string, string> extraPaths) => _assets[id];
        public List<(int, int)> GetSubItemRangesForFile(AssetFileInfo info, IDictionary<string, string> extraPaths)
            => new List<(int, int)> { (0, 100) };
    }
}
