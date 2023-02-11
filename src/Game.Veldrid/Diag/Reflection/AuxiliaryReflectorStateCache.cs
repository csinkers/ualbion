using System.Collections.Generic;

namespace UAlbion.Game.Veldrid.Diag.Reflection;

public class AuxiliaryReflectorStateCache
{
    record struct Key(object Parent, ReflectorMetadata Meta, int Index, string Type);
    readonly Dictionary<Key, object> _cache1 = new();
    readonly Dictionary<Key, object> _cache2 = new();
    bool _cache1Active = true;

    Dictionary<Key, object> Primary => _cache1Active ? _cache1 : _cache2;
    Dictionary<Key, object> Secondary => _cache1Active ? _cache2 : _cache1;

    public void Swap()
    {
        _cache1Active = !_cache1Active;
        Primary.Clear();
    }

    public object Get(in ReflectorState state, string type)
    {
        var key = new Key(state.Parent, state.Meta, state.Index, type);
        if (Primary.TryGetValue(key, out var value))
            return value;

        if (Secondary.TryGetValue(key, out value))
        {
            Primary[key] = value;
            return value;
        }

        return null;
    }

    public void Set(in ReflectorState state, string type, object value)
    {
        var key = new Key(state.Parent, state.Meta, state.Index, type);
        Primary[key] = value;
    }
}