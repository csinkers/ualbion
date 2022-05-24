using System;
using System.Text.Json;

namespace UAlbion.Api.Settings;

public class IntVar : IVar<int>
{
    public IntVar(string key, int defaultValue)
    {
        Key = key;
        DefaultValue = defaultValue;
    }

    public string Key { get; }
    public int DefaultValue { get; }

    public int Read(IVarSet varSet)
    {
        if (varSet == null) throw new ArgumentNullException(nameof(varSet));
        if (varSet.TryGetValue(Key, out var objValue))
        {
            if (objValue is int value) return value;
            if (objValue is JsonElement { ValueKind: JsonValueKind.Number } jsonString) return jsonString.GetInt32();
            throw new FormatException($"Var {Key} was of unexpected type {objValue.GetType()}, expected int");
        }

        return DefaultValue;
    }

    public void Write(IVarSet varSet, int value)
    {
        if (varSet == null) throw new ArgumentNullException(nameof(varSet));
        varSet.SetValue(Key, value);
    }
}