using System;
using System.Collections.Generic;
using System.Text.Json;

namespace UAlbion.Config.Properties;

public class TypeAliasAssetProperty : IAssetProperty<Type>
{
    readonly string _description;
    readonly Func<TypeConfig, IReadOnlyDictionary<string, Type>> _dictSelector;

    public TypeAliasAssetProperty(string name, string description, Func<TypeConfig, IReadOnlyDictionary<string, Type>> dictSelector)
    {
        if (string.IsNullOrEmpty(name)) throw new ArgumentException("Value cannot be null or empty.", nameof(name));
        if (string.IsNullOrEmpty(description)) throw new ArgumentException("Value cannot be null or empty.", nameof(description));

        Name = name;
        _description = description;
        _dictSelector = dictSelector ?? throw new ArgumentNullException(nameof(dictSelector));
    }

    public string Name { get; }
    public Type DefaultValue => null;
    public object FromJson(JsonElement elem, TypeConfig config)
    {
        var dict = _dictSelector(config);
        var s = elem.GetString();
        if (s != null && dict.TryGetValue(s, out var type))
            return type;

        throw new FormatException($"Could not resolve {_description} alias \"{s}\" to a type name");
    }
}