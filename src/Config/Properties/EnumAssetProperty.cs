using System;
using System.Text.Json;

namespace UAlbion.Config.Properties;

public class EnumAssetProperty<T> : IAssetProperty<T> where T : struct, Enum
{
    public EnumAssetProperty(string name, T defaultValue)
    {
        Name = name;
        DefaultValue = defaultValue;
    }

    public string Name { get; }
    public T DefaultValue { get; }
    public object FromJson(JsonElement elem, TypeConfig config)
    {
        var asString = elem.GetString();
        if (asString == null)
            throw new FormatException($"Null is an invalid value for the \"{Name}\" property (must be {typeof(T).Name})");

        return Enum.Parse<T>(asString);
    }
}