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
    }
}
