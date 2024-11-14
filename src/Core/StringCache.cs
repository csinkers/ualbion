using System;
using System.Collections.Generic;

namespace UAlbion.Core;

public class StringCache<TKey>
{
    readonly Dictionary<TKey, string> _cachedStrings = [];
    public string Get<T>(TKey key, T context, Func<TKey, T, string> builder)
    {
        if (_cachedStrings.TryGetValue(key, out var result))
            return result;

        result = builder(key, context);
        _cachedStrings[key] = result;
        return result;
    }
}