using System.Collections.Generic;

namespace UAlbion.Api
{
    public static class CoreExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            TKey key
        ) where TValue : class
            => dictionary.TryGetValue(key, out var value) ? value : null;

        public static TValue GetValueOrDefault<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            TKey key, TValue defaultValue)
            => dictionary.TryGetValue(key, out var value) ? value : defaultValue;
    }
}
