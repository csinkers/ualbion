using UAlbion.Api.Eventing;
using UAlbion.Api.Settings;
using UAlbion.Formats;

namespace UAlbion.Game.Settings;

public class VarSetContainer :  ServiceComponent<IVarSet>, IVarSet
{
    public IVarSet Set { get; set; } = new VarSet();
    public bool TryGetValue(string key, out object value) => Set.TryGetValue(key, out value);
    public void SetValue(string key, object value) => Set.SetValue(key, value);
    public void ClearValue(string key) => Set.ClearValue(key);
}