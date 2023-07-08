using System.Text.Json;

namespace UAlbion.Config.Properties;

public class IntAssetProperty : IAssetProperty<int>
{
    public IntAssetProperty(string name) => Name = name;
    public string Name { get; }
    public int DefaultValue => 0;
    public object FromJson(JsonElement elem, TypeConfig config) => elem.GetInt32();
}