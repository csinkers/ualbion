using System.Text.Json;

namespace UAlbion.Config;

public interface IAssetProperty
{
    string Name { get; }
    object FromJson(JsonElement elem, TypeConfig config);
}

public interface IAssetProperty<out T> : IAssetProperty
{
    T DefaultValue { get; }
}