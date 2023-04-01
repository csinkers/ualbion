namespace UAlbion.Api.Settings;

public interface ISettings : IVarSet
{
    void SetValue(string key, object value);
    void ClearValue(string key);
    void Save();
}