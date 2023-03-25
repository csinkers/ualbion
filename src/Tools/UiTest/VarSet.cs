using UAlbion.Api.Settings;

namespace UiTest;

public class VarSet : IVarSet
{
    readonly Dictionary<string, object> _values;
    readonly string _name;

    public VarSet(string name)
    {
        _name = name;
        _values = new Dictionary<string, object>();
    }

    public bool TryGetValue(string key, out object? value) => _values.TryGetValue(key, out value);
    public override string ToString() => $"VarSet {_name}";
    public void SetValue(string key, object value) => _values[key] = value;
    public void ClearValue(string key) => _values.Remove(key);
}