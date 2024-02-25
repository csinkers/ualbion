using System;
using System.Globalization;
using System.Text.Json;

namespace UAlbion.Api.Settings;

public class IntVar : IVar<int>
{
    public IntVar(VarLibrary library, string key, int defaultValue)
    {
        if (library == null) throw new ArgumentNullException(nameof(library));
        Key = key;
        DefaultValue = defaultValue;
        library.Add(this);
    }

    public string Key { get; }
    public int DefaultValue { get; }
    public object DefaultValueUntyped => DefaultValue;
    public Type ValueType => typeof(int);

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

    public void Write(ISettings varSet, int value)
    {
        if (varSet == null) throw new ArgumentNullException(nameof(varSet));
        varSet.SetValue(Key, value);
    }

    public void WriteFromString(ISettings varSet, string value)
    {
        var n = int.Parse(value, CultureInfo.InvariantCulture);
        Write(varSet, n);
    }

    public override string ToString()
        => $"IntVar({Key}) (default={DefaultValue})";
}