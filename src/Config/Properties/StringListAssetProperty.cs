using System;
using System.Collections.Generic;
using System.Text.Json;

namespace UAlbion.Config.Properties;

public class StringListAssetProperty : IAssetProperty<List<string>>
{
    public StringListAssetProperty(string name) => Name = name;
    public string Name { get; }
    public List<string> DefaultValue => [];
    public object FromJson(JsonElement elem, TypeConfig config)
    {
        if (elem.ValueKind != JsonValueKind.Array)
            throw new FormatException($"Property \"{Name}\" expects an array of strings");

        var list = new List<string>();
        foreach (var s in elem.EnumerateArray())
            list.Add(s.GetString());

        return list;
    }

}