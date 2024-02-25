using System;
using System.Globalization;
using System.Text.Json;

namespace UAlbion.Api.Settings;

public class FloatVar : IVar<float>
{
    public FloatVar(VarLibrary library, string key, float defaultValue)
    {
        if (library == null) throw new ArgumentNullException(nameof(library));
        Key = key;
        DefaultValue = defaultValue;
        library.Add(this);
    }

    public string Key { get; }
    public float DefaultValue { get; }
    public object DefaultValueUntyped => DefaultValue;
    public Type ValueType => typeof(float);

    public float Read(IVarSet varSet)
    {
        if (varSet == null) throw new ArgumentNullException(nameof(varSet));
        if (varSet.TryGetValue(Key, out var objValue))
        {
            if (objValue is float value) return value;
            if (objValue is int intValue) return intValue;
            if (objValue is JsonElement { ValueKind: JsonValueKind.Number } jsonString) return jsonString.GetSingle();
            throw new FormatException($"Var {Key} was of unexpected type {objValue.GetType()}, expected float");
        }

        return DefaultValue;
    }

    public void Write(ISettings varSet, float value)
    {
        if (varSet == null) throw new ArgumentNullException(nameof(varSet));
        varSet.SetValue(Key, value);
    }

    public void WriteFromString(ISettings varSet, string value)
    {
        var n = float.Parse(value, CultureInfo.InvariantCulture);
        Write(varSet, n);
    }
}