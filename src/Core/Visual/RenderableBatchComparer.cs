using System.Collections.Generic;

namespace UAlbion.Core.Visual;

public class RenderableBatchComparer<TInstance> : IComparer<RenderableBatch<SpriteKey, TInstance>>
    where TInstance : unmanaged
{
#pragma warning disable CA1000 // Do not declare visible instance fields
    public static RenderableBatchComparer<TInstance> Instance { get; } = new();
#pragma warning restore CA1000 // Do not declare visible instance fields
    public int Compare(RenderableBatch<SpriteKey, TInstance> x, RenderableBatch<SpriteKey, TInstance> y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (ReferenceEquals(null, y)) return 1;
        if (ReferenceEquals(null, x)) return -1;
        return x.Key.CompareTo(y.Key);
    }
}