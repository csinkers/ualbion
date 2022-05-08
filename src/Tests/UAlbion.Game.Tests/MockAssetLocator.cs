using System.Collections.Generic;
using System.IO;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats;
using UAlbion.Game.Assets;

namespace UAlbion.Game.Tests;

public class MockAssetLocator : ServiceComponent<IAssetLocator>, IAssetLocator
{
    readonly Dictionary<AssetId, object> _assets = new();
    public IAssetLocator AddAssetLocator(IAssetLocator locator, bool useAsDefault) => this;
    public IAssetLocator AddAssetPostProcessor(IAssetPostProcessor postProcessor) => this;
    public MockAssetLocator Add(AssetId key, object asset) { _assets[key] = asset; return this; }
    public object LoadAsset(AssetInfo info, SerdesContext context, TextWriter annotationWriter) => _assets[info.AssetId];
    public List<(int, int)> GetSubItemRangesForFile(AssetFileInfo info, SerdesContext context) => new() { (0, 100) };
}