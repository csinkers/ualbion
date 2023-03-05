using System;
using System.Runtime.CompilerServices;

namespace UAlbion.Core.Visual;

public class BatchLease<TKey, TInstance> : IComparable<BatchLease<TKey, TInstance>>
    where TKey : IBatchKey, IEquatable<TKey>
    where TInstance : unmanaged
{
    readonly RenderableBatch<TKey, TInstance> _batch;
    public TKey Key => _batch.Key;
    public int Length => To - From;
    internal int From { get; set; } // [from..to)
    internal int To { get; set; }
    public bool Disposed { get; private set; }
    internal object Owner { get; set; } // For debugging
    public override string ToString() => $"LEASE [{From}-{To}) {_batch} for {Owner}";

    public TInstance? GetInstance(int index)
    {
        if (index < 0 || index >= Length)
            return null;

        bool taken = false;
        var span = Lock(ref taken);
        var result = span[index];
        Unlock(taken);
        return result;
    }

    public void Dispose()
    {
        if (Disposed) return;
        _batch.Shrink(this);
        Disposed = true;
    }

    public void Update(int index, in TInstance instance)
    {
        bool taken = false;
        var span = Lock(ref taken);
        span[index] = instance;
        Unlock(taken);
    }

    /* ==== Example of using Access: ==== *\
    lease.Access(static (instances, context) => 
    {
        // Do stuff with instances. 
        // This lambda should be static to prevent per-call allocation of a closure,
        // access to members can be done by passing through 'this' as the context. If
        // multiple values are needed then allocations can still be avoided by passing
        // through a value tuple, e.g. (this, somethingElse)
    }, this);
    \* ================================== */

    /* ==== Example of using Lock + Unlock: ==== *\
    bool lockWasTaken = false;
    var instances = lease.Lock(ref lockWasTaken);
    try
    {
        // Do stuff with instances
    }
    finally { lease.Unlock(lockWasTaken); }
    \* ========================================= */

    /// <summary>
    /// A delegate type for sprite instance data mutation functions. Accepts a span of the
    /// lease's instances as well as an arbitrary context object.
    /// </summary>
    /// <typeparam name="T">The type of the context object</typeparam>
    /// <param name="instances">A span pointing at the lease's instance data</param>
    /// <param name="context">The context for the mutator function</param>
    public delegate void LeaseAccessor<in T>(Span<TInstance> instances, T context);

    /// <summary>
    /// Invokes the mutator function with a span of the lease's instance
    /// data to allow modification. An arbitrary context object can also be
    /// passed in to avoid allocation of a closure.
    /// </summary>
    /// <typeparam name="T">The type of the context object</typeparam>
    /// <param name="mutatorFunc">The function used to modify the instance data</param>
    /// <param name="context">The context for the mutator function</param>
    public void Access<T>(LeaseAccessor<T> mutatorFunc, T context)
    {
        if (mutatorFunc == null) throw new ArgumentNullException(nameof(mutatorFunc));
        bool lockWasTaken = false;
        var instances = Lock(ref lockWasTaken);
        try { mutatorFunc(instances, context); }
        finally { Unlock(lockWasTaken); }
    }

    /// <summary>
    /// Locks the sprite lease's underlying memory to ensure that it
    /// won't be moved until Unlock is called. It is the caller's
    /// responsibility to always call Unlock before the Span goes out
    /// of scope (and pass through lockWasTaken as returned by Lock).
    /// Care should be taken that this happens even if an exception is
    /// thrown, e.g. by using a try...finally.
    /// </summary>
    /// <param name="lockWasTaken"></param>
    /// <returns>A span containing the lease's instance data</returns>
    public Span<TInstance> Lock(ref bool lockWasTaken)
    {
        if (Disposed)
            throw new InvalidOperationException("BatchLease used after return");
        return _batch.Lock(this, ref lockWasTaken);
    }

    /// <summary>
    /// Releases the lock on the lease's underlying memory.
    /// </summary>
    /// <param name="lockWasTaken">This should be the value returned as a ref parameter from Lock. It is important for re-entrant calls.</param>
    public void Unlock(bool lockWasTaken) => _batch.Unlock(this, lockWasTaken);

    // Should only be created by RenderableBatch
    internal BatchLease(RenderableBatch<TKey, TInstance> spriteBatch, int from, int to)
    {
        _batch = spriteBatch ?? throw new ArgumentNullException(nameof(spriteBatch));
        From = from;
        To = to;
    }

    public int CompareTo(BatchLease<TKey, TInstance> other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other is null) return 1;
        var fromComparison = From.CompareTo(other.From);
        if (fromComparison != 0) return fromComparison;
        return To.CompareTo(other.To);
    }

    public override bool Equals(object obj) => obj is BatchLease<TKey, TInstance> lease && Equals(Key, lease.Key) && From == lease.From && To == lease.To;
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);
    public static bool operator ==(BatchLease<TKey, TInstance> left, BatchLease<TKey, TInstance> right) => left?.Equals(right) ?? right is null;
    public static bool operator !=(BatchLease<TKey, TInstance> left, BatchLease<TKey, TInstance> right) => !(left == right);
    public static bool operator <(BatchLease<TKey, TInstance> left, BatchLease<TKey, TInstance> right) => left is null ? right is not null : left.CompareTo(right) < 0;
    public static bool operator <=(BatchLease<TKey, TInstance> left, BatchLease<TKey, TInstance> right) => left is null || left.CompareTo(right) <= 0;
    public static bool operator >(BatchLease<TKey, TInstance> left, BatchLease<TKey, TInstance> right) => !(left is null) && left.CompareTo(right) > 0;
    public static bool operator >=(BatchLease<TKey, TInstance> left, BatchLease<TKey, TInstance> right) => left is null ? right is null : left.CompareTo(right) >= 0;
}