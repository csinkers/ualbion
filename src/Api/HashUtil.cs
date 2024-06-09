using System;

namespace UAlbion.Api;

public static class HashUtil
{
    public const uint FnvOffsetBasis = 2166136261;
    public const uint FnvPrime = 16777619;

    public static FnvHashHelper Fnv1A() => new();
    public static uint Fnv1A(Span<byte> bytes)
    {
        uint hash = FnvOffsetBasis;
        foreach (var b in bytes)
        {
            hash ^= b;
            hash *= FnvPrime;
        }

        return hash;
    }
}