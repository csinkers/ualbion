using System;
using System.Text.Json;

namespace UAlbion.Config.Properties;

public class AssetRangeAssetProperty : IAssetProperty<AssetRange>
{
    public AssetRangeAssetProperty(string name) => Name = name;
    public string Name { get; }
    public AssetRange DefaultValue => new(AssetId.None, AssetId.None);
    public object FromJson(JsonElement elem, TypeConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        var asString = elem.GetString();
        return config.ParseIdRange(asString);
    }
}