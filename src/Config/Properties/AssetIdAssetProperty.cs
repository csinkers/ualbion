using System;
using System.Text.Json;
using UAlbion.Api;

namespace UAlbion.Config.Properties;

public class AssetIdAssetProperty : IAssetProperty<AssetId>
{
    public AssetIdAssetProperty(string name) => Name = name;
    public string Name { get; }
    public AssetId DefaultValue => AssetId.None;
    public object FromJson(JsonElement elem, TypeConfig config)
    {
        if (config == null) throw new ArgumentNullException(nameof(config));
        var asString = elem.GetString();
        return config.ResolveId(asString);
    }
}

public class AssetIdAssetProperty<T> : IAssetProperty<T> where T : IAssetId
{
    readonly Func<AssetId, T> _converter;

    public AssetIdAssetProperty(string name, T defaultValue, Func<AssetId, T> converter)
    {
        _converter = converter;
        Name = name;
        DefaultValue = defaultValue;
    }

    public string Name { get; }
    public T DefaultValue { get; }
    public object FromJson(JsonElement elem, TypeConfig config)
    {
        if (config == null) throw new ArgumentNullException(nameof(config));
        var asString = elem.GetString();
        AssetId id = config.ResolveId(asString);
        return _converter(id);
    }
}