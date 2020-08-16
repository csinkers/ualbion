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

        public bool Equals(ShaderSetCacheKey other) => Name.Equals(other.Name, StringComparison.Ordinal) && Hash.Equals(other.Hash, StringComparison.Ordinal);
        public override int GetHashCode() => Name.GetHashCode(StringComparison.Ordinal) ^ Hash.GetHashCode(StringComparison.Ordinal);
        public override bool Equals(object obj) => obj is ShaderSetCacheKey key && Equals(key);
        public static bool operator ==(ShaderSetCacheKey left, ShaderSetCacheKey right) => left.Equals(right);
        public static bool operator !=(ShaderSetCacheKey left, ShaderSetCacheKey right) => !(left == right);
    }
}
