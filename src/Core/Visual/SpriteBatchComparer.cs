using System.Collections.Generic;

namespace UAlbion.Core.Visual;

public class SpriteBatchComparer<TInstance> : IComparer<SpriteBatch<TInstance>>
    where TInstance : unmanaged
{
#pragma warning disable CA1000 // Do not declare visible instance fields
    public static SpriteBatchComparer<TInstance> Instance { get; } = new();
#pragma warning restore CA1000 // Do not declare visible instance fields
    public int Compare(SpriteBatch<TInstance> x, SpriteBatch<TInstance> y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (ReferenceEquals(null, y)) return 1;
        if (ReferenceEquals(null, x)) return -1;
        return x.Key.CompareTo(y.Key);
    }
}