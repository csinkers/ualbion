namespace UAlbion.Api.Settings;

public interface IVar
{
    string Key { get; }
}

public interface IVar<T> : IVar
{
    T DefaultValue { get; }
    T Read(IVarSet varSet);
    void Write(ISettings varSet, T value);
}