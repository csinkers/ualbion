using System;
using System.Text.Json;

namespace UAlbion.Api.Settings;

public class StringVar : IVar<string>
{
    public StringVar(string key, string defaultValue)
    {
        Key = key;
        DefaultValue = defaultValue;
    }

    public string Key { get; }
    public string DefaultValue { get; }

    public string Read(IVarSet varSet)
    {
        if (varSet == null) throw new ArgumentNullException(nameof(varSet));
        if (varSet.TryGetValue(Key, out var objValue))
        {
            if (objValue is string value) return value;
            if (objValue is JsonElement { ValueKind: JsonValueKind.String } jsonString) return jsonString.GetString();
            throw new FormatException($"Var {Key} was of unexpected type {objValue.GetType()}, expected string");
        }

        return DefaultValue;
    }

    public void Write(ISettings varSet, string value)
    {
        if (varSet == null) throw new ArgumentNullException(nameof(varSet));
        varSet.SetValue(Key, value);
    }
}