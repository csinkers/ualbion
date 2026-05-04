using System;
using System.Text.Json;

namespace UAlbion.Api.Settings;

public class BoolVar : IVar<bool>
{
    public BoolVar(VarLibrary library, string key, bool defaultValue)
    {
        ArgumentNullException.ThrowIfNull(library);
        Key = key;
        DefaultValue = defaultValue;
        library.Add(this);
    }

    public string Key { get; }
    public bool DefaultValue { get; }
    public object DefaultValueUntyped => DefaultValue;
    public Type ValueType => typeof(bool);

    public bool Read(IVarSet varSet)
    {
        ArgumentNullException.ThrowIfNull(varSet);
        if (varSet.TryGetValue(Key, out var objValue))
        {
            if (objValue is bool value) return value;
            if (objValue is JsonElement { ValueKind: JsonValueKind.Number } jsonString) return jsonString.GetBoolean();
            throw new FormatException($"Var {Key} was of unexpected type {objValue.GetType()}, expected bool");
        }

        return DefaultValue;
    }

    public void Write(ISettings varSet, bool value)
    {
        ArgumentNullException.ThrowIfNull(varSet);
        varSet.SetValue(Key, value);
    }

    public void WriteFromString(ISettings varSet, string value)
    {
        var n = bool.Parse(value);
        Write(varSet, n);
    }

    public override string ToString()
        => $"BoolVar({Key}) (default={DefaultValue})";
}