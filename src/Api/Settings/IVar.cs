namespace UAlbion.Api.Settings;

public interface IVar<T>
{
    string Key { get; }
    T Read(IVarSet varSet);
    void Write(IVarSet varSet, T value);
}