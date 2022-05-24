namespace UAlbion.Api.Settings;

public interface IVarSet
{
    bool TryGetValue(string key, out object value);
    void SetValue(string key, object value);
    void ClearValue(string key);
}