namespace UAlbion.Api.Settings;

public interface IVarSet
{
    bool TryGetValue(string key, out object value);
}