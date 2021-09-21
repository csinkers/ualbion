using System;
using System.Collections.Generic;
using System.Linq;

namespace UAlbion.Formats.Exporters.Tiled
{
    class ScriptableKeyComparer : IEqualityComparer<ScriptableKey>
    {
        ScriptableKeyComparer() { }
        public static IEqualityComparer<ScriptableKey> Instance { get; } = new ScriptableKeyComparer();
        bool BytesEqual(byte[] x, byte[] y) => ReferenceEquals(x, y) || x is { } && y is { } && x.SequenceEqual(y);
        int HashBytes(byte[] data)
        {
            if (data == null)
                return 0;

            unchecked
            {
                const int p = 16777619;
                int hash = (int)2166136261;

                foreach (var t in data)
                    hash = (hash ^ t) * p;

                hash += hash << 13;
                hash ^= hash >> 7;
                hash += hash << 3;
                hash ^= hash >> 17;
                hash += hash << 5;
                return hash;
            }
        }

        public bool Equals(ScriptableKey x, ScriptableKey y) => x.ChainHint == y.ChainHint && BytesEqual(x.EventBytes, y.EventBytes);
        public int GetHashCode(ScriptableKey key) => HashCode.Combine(key.ChainHint, HashBytes(key.EventBytes));
    }
}