using System;
using System.Collections.Generic;
using UAlbion.Api;
using UAlbion.Api.Settings;

namespace UAlbion.Formats;

public class VarSet : IVarSet, IPatch
{
    const string VersionKey = "ConfigVersion";
    const int ConfigVersion = 1;
    readonly Dictionary<string, object> _values;
    readonly string _name;
    VarSet _parent;

    public VarSet(string name)
    {
        _name = name;
        _values = new Dictionary<string, object>();
    }

    public VarSet(string name, Dictionary<string, object> values)
    {
        _name = name;
        _values = values ?? throw new ArgumentNullException(nameof(values));
    }

    public bool TryGetValue(string key, out object value)
    {
        if (_values.TryGetValue(key, out value))
            return true;

        if (_parent == null)
            return false;

        return _parent.TryGetValue(key, out value);
    }

    public override string ToString() => $"VarSet {_name}";
    public void SetValue(string key, object value) => _values[key] = value;
    public void ClearValue(string key) => _values.Remove(key);

    public object Apply(object asset)
    {
        if (asset is VarSet parent)
            _parent = parent;
        return this;
    }

    public string ToJson(IJsonUtil jsonUtil)
    {
        _values[VersionKey] = ConfigVersion;
        var result = jsonUtil.Serialize(_values);
        _values.Remove(VersionKey);
        return result;
    }

    public static VarSet FromJsonBytes(string name, byte[] bytes, IJsonUtil json)
    {
        // Note: If version doesn't match discard old config and go back to defaults.
        // This is just to clear out any bad entries from before the format was stabilised,
        // if future changes are made to the format we can implement an actual upgrade process.
        var dictionary = json.Deserialize<Dictionary<string, object>>(bytes);
        return dictionary.TryGetValue(VersionKey, out var version) && version is ConfigVersion
            ? new VarSet(name, dictionary)
            : new VarSet(name, new Dictionary<string, object>());
    }
}