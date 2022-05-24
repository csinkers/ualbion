using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UAlbion.Config;

#pragma warning disable CA1710 // Identifiers should have correct suffix
public class FallbackDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
{
    readonly IReadOnlyDictionary<TKey, TValue> _primary;
    readonly IReadOnlyDictionary<TKey, TValue> _fallback;

    public FallbackDictionary(
        IReadOnlyDictionary<TKey, TValue> primary,
        IReadOnlyDictionary<TKey, TValue> fallback)
    {
        _primary = primary ?? throw new ArgumentNullException(nameof(primary));
        _fallback = fallback;
    }

    IEnumerator<KeyValuePair<TKey, TValue>> EnumerationHelper()
    {
        if (_fallback != null)
        {
            foreach (var kvp in _fallback)
                if (!_primary.ContainsKey(kvp.Key))
                    yield return kvp;
        }

        foreach (var kvp in _primary)
            yield return kvp;
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => EnumerationHelper();
    IEnumerator IEnumerable.GetEnumerator() => EnumerationHelper();
    public int Count => _fallback == null ? _primary.Count : _fallback.Keys.Union(_primary.Keys).Count();
    public bool ContainsKey(TKey key) => _primary.ContainsKey(key) || _fallback.ContainsKey(key);
    public bool TryGetValue(TKey key, out TValue value) => _primary.TryGetValue(key, out value) || _fallback.TryGetValue(key, out value);
    public TValue this[TKey key] => TryGetValue(key, out var value) ? value : throw new KeyNotFoundException($"Could not find key {key}");
    public IEnumerable<TKey> Keys => _fallback != null ? _fallback.Keys.Union(_primary.Keys) : _primary.Keys;
    public IEnumerable<TValue> Values => _fallback != null ? _fallback.Values.Union(_primary.Values) : _primary.Values;
}
#pragma warning restore CA1710 // Identifiers should have correct suffix