using System.Collections.Generic;

namespace UAlbion.Core.Visual;

public class SpriteBatchComparer<TInstance> : IComparer<SpriteBatch<TInstance>>
    where TInstance : unmanaged
{
    public static SpriteBatchComparer<TInstance> Instance { get; } = new();
    public int Compare(SpriteBatch<TInstance> x, SpriteBatch<TInstance> y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (ReferenceEquals(null, y)) return 1;
        if (ReferenceEquals(null, x)) return -1;
        return x.Key.CompareTo(y.Key);
    }
}