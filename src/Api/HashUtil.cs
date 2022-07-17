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
}