﻿using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Api;
using UAlbion.Api.Settings;

namespace UAlbion.Formats;

public class VarSet : IVarSet, IPatch
{
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

    internal IEnumerable<(string, object)> Ordered // Just for debugging
        => _values.OrderBy(x => x.Key).Select(x => (x.Key, x.Value));

    public bool TryGetValue(string key, out object value)
    {
        if (_values.TryGetValue(key, out value))
            return true;

        if (_parent == null)
            return false;

        return _parent.TryGetValue(key, out value);
    }

    public override string ToString() => $"VarSet {_name}";
    public IEnumerable<string> Keys
    {
        get
        {
            if (_parent != null)
                foreach (var key in _parent.Keys)
                    yield return key;

            foreach (var key in _values.Keys)
                yield return key;
        }
    }

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
        if (jsonUtil == null) throw new ArgumentNullException(nameof(jsonUtil));
        return jsonUtil.Serialize(_values);
    }

    public static VarSet FromJsonBytes(string name, byte[] bytes, IJsonUtil json)
    {
        if (bytes == null) throw new ArgumentNullException(nameof(bytes));
        if (json == null) throw new ArgumentNullException(nameof(json));

        var dictionary = json.Deserialize<Dictionary<string, object>>(bytes);
        return new VarSet(name, dictionary);
    }
}