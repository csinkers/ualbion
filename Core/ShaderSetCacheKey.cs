using System;
using Veldrid;

namespace UAlbion.Core
{
    public struct ShaderSetCacheKey : IEquatable<ShaderSetCacheKey>
    {
        public string Name { get; }
        public string Hash { get; }
        public SpecializationConstant[] Specializations { get; }

        public ShaderSetCacheKey(string name, string hash, SpecializationConstant[] specializations) : this()
        {
            Name = name;
            Hash = hash;
            Specializations = specializations;
        }

        public bool Equals(ShaderSetCacheKey other)
        {
            return Name.Equals(other.Name) 
                   && Hash.Equals(other.Hash)
                   && ArraysEqual(Specializations, other.Specializations);
        }

        public override int GetHashCode()
        {
            int hash = Name.GetHashCode();
            hash ^= Hash.GetHashCode();
            foreach (var specConst in Specializations)
            {
                hash ^= specConst.GetHashCode();
            }
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
