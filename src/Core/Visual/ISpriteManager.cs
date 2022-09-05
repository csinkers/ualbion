using System;

namespace UAlbion.Core.Visual;

public interface IBatchManager<TKey, TInstance> : IRenderableSource 
    where TKey : IBatchKey, IEquatable<TKey>
    where TInstance : unmanaged
{
    BatchLease<TKey, TInstance> Borrow(TKey key, int count, object owner);
}