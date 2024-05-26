using System;
using System.Text.Json;

namespace UAlbion.Api.Settings;

public class StringVar : IVar<string>
{
    public StringVar(VarLibrary library, string key, string defaultValue)
    {
        ArgumentNullException.ThrowIfNull(library);
        Key = key;
        DefaultValue = defaultValue;
        library.Add(this);
    }

    public string Key { get; }
    public string DefaultValue { get; }
    public object DefaultValueUntyped => DefaultValue;
    public Type ValueType => typeof(string);

    public string Read(IVarSet varSet)
    {
        ArgumentNullException.ThrowIfNull(varSet);
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
        ArgumentNullException.ThrowIfNull(varSet);
        varSet.SetValue(Key, value);
    }

    public void WriteFromString(ISettings varSet, string value) => Write(varSet, value);

    public override string ToString()
        => $"StringVar({Key}) (default={DefaultValue})";
}
