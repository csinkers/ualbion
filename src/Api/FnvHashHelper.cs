namespace UAlbion.Api;

#pragma warning disable CA1815 // Override equals and operator equals on value types
public readonly struct FnvHashHelper
{
    public uint Hash { get; }

    public FnvHashHelper() => Hash = HashUtil.FnvOffsetBasis;
    FnvHashHelper(uint hash) => Hash = hash;

    public FnvHashHelper Combine(byte value)
    {
        var hash = Hash;
        hash ^= value;
        hash *= HashUtil.FnvPrime;
        return new FnvHashHelper(hash);
    }

    public FnvHashHelper Combine(ushort value)
    {
        var hash = Hash;
        hash ^= (byte)(value & 0xff);
        hash *= HashUtil.FnvPrime;
        hash ^= (byte)(value >> 8);
        hash *= HashUtil.FnvPrime;
        return new FnvHashHelper(hash);
    }

    public FnvHashHelper Combine(uint value)
    {
        var hash = Hash;
        hash ^= (byte)(value & 0xff);
        hash *= HashUtil.FnvPrime;
        hash ^= (byte)((value & 0xff00) >> 8);
        hash *= HashUtil.FnvPrime;
        hash ^= (byte)((value & 0xff0000) >> 16);
        hash *= HashUtil.FnvPrime;
        hash ^= (byte)((value & 0xff000000) >> 24);
        hash *= HashUtil.FnvPrime;
        return new FnvHashHelper(hash);
    }

    public FnvHashHelper Combine(int value) => Combine(unchecked((uint)value));
}
#pragma warning restore CA1815 // Override equals and operator equals on value types