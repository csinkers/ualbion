using System.Text.Json;

namespace UAlbion.Config.Properties;

public class PathPatternProperty : IAssetProperty<AssetPathPattern>
{
    public PathPatternProperty(string name)
    {
        Name = name;
        DefaultValue = AssetPathPattern.Build("");
    }

    public PathPatternProperty(string name, string defaultPath)
    {
        Name = name;
        DefaultValue = AssetPathPattern.Build(defaultPath);
    }

    public string Name { get; }
    public AssetPathPattern DefaultValue { get; }
    public object FromJson(JsonElement elem, TypeConfig config)
    {
        var asString = elem.GetString();
        return AssetPathPattern.Build(asString);
    }
}