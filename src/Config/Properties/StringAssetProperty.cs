using System.Text.Json;

namespace UAlbion.Config.Properties;

public class StringAssetProperty : IAssetProperty<string>
{
    public StringAssetProperty(string name) => Name = name;
    public string Name { get; }
    public string DefaultValue => null;
    public object FromJson(JsonElement elem, TypeConfig config) => elem.GetString();
}