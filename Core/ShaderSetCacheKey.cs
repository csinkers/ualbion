using System;

namespace UAlbion.Core
{
    public struct ShaderSetCacheKey : IEquatable<ShaderSetCacheKey>
    {
        public string Name { get; }
        public string Hash { get; }

        public ShaderSetCacheKey(string name, string hash) : this()
        {
            Name = name;
            Hash = hash;
        }

        public bool Equals(ShaderSetCacheKey other)
        {
            return Name.Equals(other.Name)
                   && Hash.Equals(other.Hash);
        }

        public override int GetHashCode()
        {
            int hash = Name.GetHashCode();
            hash ^= Hash.GetHashCode();
            return hash;
        }

        static bool ArraysEqual<T>(T[] a, T[] b) where T : IEquatable<T>
        {
            if (a.Length != b.Length) { return false; }

            for (int i = 0; i < a.Length; i++)
                if (!a[i].Equals(b[i])) { return false; }

            return true;
        }
    }
}
