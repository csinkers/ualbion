using System;
using System.Collections.Generic;
using UAlbion.Api.Settings;

namespace UAlbion.Formats;

public class VarSet : IVarSet, IPatch
{
    readonly Dictionary<string, object> _values;
    VarSet _parent;
    public VarSet() => _values = new Dictionary<string, object>();
    public VarSet(Dictionary<string, object> values) => _values = values ?? throw new ArgumentNullException(nameof(values));
    public bool TryGetValue(string key, out object value)
    {
        if (_values.TryGetValue(key, out value))
            return true;

        if (_parent == null)
            return false;

        return _parent.TryGetValue(key, out value);
    }

    public void SetValue(string key, object value) => _values[key] = value;
    public void ClearValue(string key) => _values.Remove(key);
    public object Apply(object asset)
    {
        if (asset is VarSet parent)
            _parent = parent;
        return this;
    }
}