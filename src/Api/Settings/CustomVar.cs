using System;
using System.Text.Json;

namespace UAlbion.Api.Settings;

public class CustomVar<TLogical, TPersistent> : IVar<TLogical>
{
    readonly Func<TLogical, TPersistent> _castTo;
    readonly Func<TPersistent, TLogical> _castFrom;
    readonly Func<JsonElement, TPersistent> _castJson;

    public CustomVar(VarLibrary library, string key, TLogical defaultValue, Func<TLogical, TPersistent> castTo, Func<TPersistent, TLogical> castFrom, Func<JsonElement, TPersistent> castJson)
    {
        if (library == null) throw new ArgumentNullException(nameof(library));
        Key = key;
        DefaultValue = defaultValue;
        _castTo = castTo ?? throw new ArgumentNullException(nameof(castTo));
        _castFrom = castFrom ?? throw new ArgumentNullException(nameof(castFrom));
        _castJson = castJson;
        library.Add(this);
    }

    public string Key { get; }
    public TLogical DefaultValue { get; }
    public object DefaultValueUntyped => DefaultValue;
    public Type ValueType => typeof(TLogical);

    public TLogical Read(IVarSet varSet)
    {
        if (varSet == null) throw new ArgumentNullException(nameof(varSet));
        if (varSet.TryGetValue(Key, out var objValue))
        {
            if (objValue is TPersistent value) return _castFrom(value);
            if (objValue is JsonElement jsonValue) return _castFrom(_castJson(jsonValue));
            throw new FormatException($"Var {Key} was of unexpected type {objValue.GetType()}, expected {typeof(TPersistent)}");
        }

        return DefaultValue;
    }

    public void Write(ISettings varSet, TLogical value)
    {
        if (varSet == null) throw new ArgumentNullException(nameof(varSet));
        varSet.SetValue(Key, _castTo(value));
    }

    public void WriteFromString(ISettings varSet, string value)
    {
        if (varSet == null) throw new ArgumentNullException(nameof(varSet));
        var doc = JsonDocument.Parse(value);
        var typedValue = _castJson(doc.RootElement);
        varSet.SetValue(Key, typedValue);
    }

    public override string ToString()
        => $"CustomVar<{typeof(TLogical)}, {typeof(TPersistent)}>({Key}) (default={DefaultValue})";
}