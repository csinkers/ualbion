using UAlbion.Api.Eventing;
using UAlbion.Api.Settings;
using UAlbion.Formats;

namespace UAlbion.Game.Tests;

public class MockSettings : Component, ISettings
{
    readonly VarSet _set = new();
    public bool TryGetValue(string key, out object value) => _set.TryGetValue(key, out value);
    public void SetValue(string key, object value) => _set.SetValue(key, value);
    public void ClearValue(string key) => _set.ClearValue(key);
    public void Save() { }
    protected override void Subscribing()
    {
        Exchange.Register(typeof(ISettings), this, false);
        Exchange.Register(typeof(IVarSet), this, false);
    }

    protected override void Unsubscribed()
    {
        Exchange.Unregister(typeof(IVarSet), this);
        Exchange.Unregister(typeof(ISettings), this);
    }
}