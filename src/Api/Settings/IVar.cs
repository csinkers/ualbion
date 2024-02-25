using System;

namespace UAlbion.Api.Settings;

public interface IVar
{
    string Key { get; }
    object DefaultValueUntyped { get; }
    Type ValueType { get; }
    void WriteFromString(ISettings varSet, string value);
}

public interface IVar<T> : IVar
{
    T DefaultValue { get; }
    T Read(IVarSet varSet);
    void Write(ISettings varSet, T value);
}