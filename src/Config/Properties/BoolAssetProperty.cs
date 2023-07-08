using System.Text.Json;

namespace UAlbion.Config.Properties;

public class BoolAssetProperty : IAssetProperty<bool>
{
    public BoolAssetProperty(string name) => Name = name;
    public string Name { get; }
    public bool DefaultValue => false;
    public object FromJson(JsonElement elem, TypeConfig config) => elem.GetBoolean();
}