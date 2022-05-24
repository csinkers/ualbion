using System;
using System.Text.Json;

namespace UAlbion.Api.Settings;

public class StringVar : IVar<string>
{
    public StringVar(string key, string defaultValue)
    {
        Key = key;
        _defaultValue = defaultValue;
    }

    public string Key { get; }
    readonly string _defaultValue;

    public string Read(IVarSet varSet)
    {
        if (varSet == null) throw new ArgumentNullException(nameof(varSet));
        if (varSet.TryGetValue(Key, out var objValue))
        {
            if (objValue is string value) return value;
            if (objValue is JsonElement { ValueKind: JsonValueKind.String } jsonString) return jsonString.GetString();
            throw new FormatException($"Var {Key} was of unexpected type {objValue.GetType()}, expected string");
        }

        return _defaultValue;
    }

    public void Write(IVarSet varSet, string value)
    {
        if (varSet == null) throw new ArgumentNullException(nameof(varSet));
        varSet.SetValue(Key, value);
    }
}