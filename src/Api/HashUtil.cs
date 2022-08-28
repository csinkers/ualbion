using System;

namespace UAlbion.Api;

public static class HashUtil
{
    const uint FnvOffsetBasis = 2166136261;
    const uint FnvPrime = 16777619;

    public static uint FNV1a(Span<byte> bytes)
    {
        uint hash = FnvOffsetBasis;
        foreach (var b in bytes)
        {
            hash ^= b;
            hash *= FnvPrime;
        }

        return hash;
    }
    public static HashHelper FNV1a() => new();
    public readonly struct HashHelper
    {
        public uint Hash { get; }

        public HashHelper() => Hash = FnvOffsetBasis;
        HashHelper(uint hash) => Hash = hash;

        public HashHelper Combine(byte value)
        {
            var hash = Hash;
            hash ^= value;
            hash *= FnvPrime;
            return new HashHelper(hash);
        }

        public HashHelper Combine(ushort value)
        {
            var hash = Hash;
            hash ^= (byte)(value & 0xff);
            hash *= FnvPrime;
            hash ^= (byte)(value >> 8);
            hash *= FnvPrime;
            return new HashHelper(hash);
        }

        public HashHelper Combine(uint value)
        {
            var hash = Hash;
            hash ^= (byte)(value & 0xff);
            hash *= FnvPrime;
            hash ^= (byte)((value & 0xff00) >> 8);
            hash *= FnvPrime;
            hash ^= (byte)((value & 0xff0000) >> 16);
            hash *= FnvPrime;
            hash ^= (byte)((value & 0xff000000) >> 24);
            hash *= FnvPrime;
            return new HashHelper(hash);
        }
        public HashHelper Combine(int value) => Combine(unchecked((uint)value));
    }

}