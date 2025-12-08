using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;

namespace UAlbion.Core.Visual;

public abstract class RenderableBatch<TKey, TInstance> : Component, IRenderable, IDisposable
    where TKey : IBatchKey, IEquatable<TKey>
    where TInstance : unmanaged
{
    const double GrowthFactor = 1.5;
    const double ShrinkFactor = 0.3;
    protected const int MinSize = 4;

    readonly object _syncRoot = new();
    readonly Action<Span<TInstance>> _disableInstancesFunc;
    readonly bool _noVerify;

    BatchLease<TKey, TInstance> _head;
    BatchLease<TKey, TInstance> _tail;
    BatchLease<TKey, TInstance> _deadHead;
    BatchLease<TKey, TInstance> _deadTail;

    protected RenderableBatch(TKey key, Action<Span<TInstance>> disableInstancesFunc)
    {
        Key = key;
        Name = $"Batch:{Key}";
        _disableInstancesFunc = disableInstancesFunc ?? throw new ArgumentNullException(nameof(disableInstancesFunc));
        _noVerify = int.Parse("1") == 1;
    }

    protected abstract ReadOnlySpan<TInstance> ReadOnlyInstances { get; }
    protected abstract Span<TInstance> MutableInstances { get; }
    protected abstract void Resize(int instanceCount);

    public TKey Key { get; }
    public string Name { get; }
    public DrawLayer RenderOrder => Key.RenderOrder;
    public override string ToString() => $"Multi:{Name} ({AssignedCount}/{ReadOnlyInstances.Length} instances)";
    public int AssignedCount => _tail?.To ?? 0; // The number of instances that are assigned to leases, either alive or dead.
    public int LiveCount { get; private set; } // The number of instances belonging to live leases.

    internal BatchLease<TKey, TInstance> BorrowLease(int length, object caller)
    {
        lock (_syncRoot)
        {
            PerfTracker.IncrementFrameCounter("Lease Borrows");
            var current = _deadHead;
            while (current != null)
            {
                if (current.Length > length)
                    current = SplitLease(current, length);

                if (current.Length == length)
                {
                    ResurrectLease(current, caller);
                    return current;
                }

                current = current.NextDead;
            }

            return AddLease(length, caller);
        }
    }

    internal void ReturnLease(BatchLease<TKey, TInstance> lease)
    {
        ArgumentNullException.ThrowIfNull(lease);
        lock (_syncRoot)
        {
            PerfTracker.IncrementFrameCounter("Lease Returns");

            int from   = lease.From;
            int length = lease.Length;
            LiveCount -= length;
            lease.Disposed = true;
            lease.Owner = null;
            AddToDeadList(lease);

            // Join with adjacent dead leases
            if (lease.Prev != null && lease.Prev.Disposed)
                lease = JoinLeases(lease.Prev);

            if (lease.Next != null && lease.Next.Disposed)
                lease = JoinLeases(lease);

            if (lease == _tail)
            {
                RemoveLease(lease);
            }
            else
            {
                _disableInstancesFunc(MutableInstances.Slice(from, length));
            }

            VerifyConsistency();
        }
    }

    BatchLease<TKey, TInstance> AddLease(int length, object caller)
    {
        int from = AssignedCount;
        int to = from + length;
        LiveCount += length;

        if (to >= ReadOnlyInstances.Length)
            GrowBuffer(to);

        var lease = new BatchLease<TKey, TInstance>(this, from, to) { Owner = caller };

        if (_tail != null)
            _tail.Next = lease;
        else
            _head = lease;

        lease.Prev = _tail;
        _tail = lease;

        PerfTracker.IncrementFrameCounter("Lease Borrows (fresh)");
        VerifyConsistency();
        return lease;
    }

    void RemoveLease(BatchLease<TKey, TInstance> lease)
    {
        ApiUtil.Assert(lease.Disposed, "Tried to remove a live lease");
        RemoveFromDeadList(lease);
        if (_head == lease) _head = lease.Next;
        if (_tail == lease) _tail = lease.Prev;

        if (lease.Prev != null) lease.Prev.Next = lease.Next;
        if (lease.Next != null) lease.Next.Prev = lease.Prev;
        VerifyConsistency();
    }

    void GrowBuffer(int required)
    {
        int newSize = ReadOnlyInstances.Length;
        if (newSize < MinSize)
            newSize = MinSize;

        while (newSize <= required)
            newSize = (int)(newSize * GrowthFactor);

        PerfTracker.IncrementFrameCounter("Lease Resizes");
        Resize(newSize);
    }

    void ResurrectLease(BatchLease<TKey, TInstance> lease, object caller)
    {
        PerfTracker.IncrementFrameCounter("Lease Borrows (reused)");
        lease.Disposed = false;
        lease.Owner = caller;
        LiveCount += lease.Length;

        RemoveFromDeadList(lease);
        VerifyConsistency();
    }

    BatchLease<TKey, TInstance> SplitLease(BatchLease<TKey, TInstance> existing, int length)
    {
        PerfTracker.IncrementFrameCounter("Lease Splits");
        ApiUtil.Assert(existing.Disposed, "Tried to split a live lease");
        ApiUtil.Assert(existing.Length > length, "Tried to split a lease that was too small");

        var newLease = new BatchLease<TKey, TInstance>(this, existing.From, existing.From + length) { Disposed = true };
        existing.From = newLease.To;

        var prev = existing.Prev;
        if (prev != null)
            prev.Next = newLease;
        else
            _head = newLease;

        newLease.Prev = prev;
        newLease.Next = existing;
        existing.Prev = newLease;

        AddToDeadList(newLease);

        VerifyConsistency();
        return newLease;
    }

    BatchLease<TKey, TInstance> JoinLeases(BatchLease<TKey, TInstance> lease) // Joins lease with the lease after it.
    {
        ApiUtil.Assert(lease.Disposed, "Tried to join a live lease");
        ApiUtil.Assert(lease.Next != null, "Tried to join a lease that had no successor");
        ApiUtil.Assert(lease.Next!.Disposed, "Tried to join a lease to a live successor");

        var toRemove = lease.Next;
        RemoveFromDeadList(toRemove);

        if (_tail == toRemove)
            _tail = lease;

        lease.To = toRemove.To;
        lease.Next = toRemove.Next;

        if (toRemove.Next != null)
            toRemove.Next.Prev = lease;

        toRemove.Prev = null;
        toRemove.Next = null;

        if (_deadHead == null)
        {
            _deadHead = lease;
            _deadTail = lease;
        }

        VerifyConsistency();

        return lease;
    }

    void AddToDeadList(BatchLease<TKey, TInstance> lease)
    {
        lease.PrevDead = _deadTail;

        if (_deadTail != null)
            _deadTail.NextDead = lease;

        _deadTail = lease;

        if (_deadHead == null)
            _deadHead = lease;

        VerifyConsistency();
    }

    void RemoveFromDeadList(BatchLease<TKey, TInstance> lease)
    {
        ApiUtil.Assert(_deadHead?.PrevDead == null, "Dead head had a prev dead link");
        ApiUtil.Assert(_deadTail?.NextDead == null, "Dead tail had a next dead link");

        if (_deadHead == lease) _deadHead = lease.NextDead;
        if (_deadTail == lease) _deadTail = lease.PrevDead;
        ApiUtil.Assert((_deadHead == null) == (_deadTail == null), "Dead list head and tail had differing nullity");
        if (lease.PrevDead != null) lease.PrevDead.NextDead = lease.NextDead;
        if (lease.NextDead != null) lease.NextDead.PrevDead = lease.PrevDead;
    }

    public void ShrinkIfNeeded()
    {
        if (AssignedCount == 0)
            return;

        if (!((float)LiveCount / AssignedCount < ShrinkFactor))
            return;

        lock (_syncRoot)
        {
            PerfTracker.IncrementFrameCounter("Lease Cleanups");

            var buffer = MutableInstances;
            int shift = 0;
            var lease = _head;
            while (lease != null)
            {
                var next = lease.Next;

                if (lease.Disposed)
                {
                    shift += lease.Length;
                    RemoveLease(lease);
                }
                else if (shift > 0)
                {
                    for (int i = lease.From; i < lease.To; i++)
                        buffer[i - shift] = buffer[i];

                    lease.From -= shift;
                    lease.To -= shift;
                }

                lease = next;
            }

            if ((double)AssignedCount / buffer.Length < ShrinkFactor)
            {
                PerfTracker.IncrementFrameCounter("Lease Shrinks");
                int newSize = buffer.Length;
                while ((double)AssignedCount / newSize > ShrinkFactor)
                    newSize = (int)(newSize * ShrinkFactor);

                newSize = Math.Max(newSize, AssignedCount);
                newSize = Math.Max(newSize, MinSize);
                Resize(newSize);
            }
        }
    }

    [Conditional("DEBUG")]
    void VerifyConsistency()
    {
        if (_noVerify)
            return;

        ApiUtil.Assert((_head == null) == (_tail == null), "Head and tail had differing nullity");
        ApiUtil.Assert((_deadHead == null) == (_deadTail == null), "Dead list head and tail had differing nullity");
        ApiUtil.Assert(_deadHead?.PrevDead == null, "Dead head had a prev dead link");
        ApiUtil.Assert(_deadTail?.NextDead == null, "Dead tail had a next dead link");

        if (_head == null || _tail == null)
        {
            ApiUtil.Assert(AssignedCount == 0, "No leases, but assigned count was > 0");
            ApiUtil.Assert(LiveCount == 0, "No leases, but live count was > 0");
            return;
        }

        // Assert that all leases are in order
        ApiUtil.Assert(_head.From == 0);

        int live = 0;
        var lease = _head;
        HashSet<BatchLease<TKey, TInstance>> leases = new();
        while (lease != null)
        {
            leases.Add(lease);
            ApiUtil.Assert(lease.Length > 0);
            if (!lease.Disposed)
                live += lease.Length;

            if (lease.Next != null)
                ApiUtil.Assert(lease.To == lease.Next.From, "Non-contiguous leases detected");

            lease = lease.Next;
        }

        ApiUtil.Assert(_tail.To == AssignedCount, "Mismatch between tail indexes and assigned count");
        ApiUtil.Assert(live == LiveCount, "Live count out of sync");

        lease = _deadHead;
        HashSet<BatchLease<TKey, TInstance>> deadLeases = new();
        while (lease != null)
        {
            deadLeases.Add(lease);
            lease = lease.NextDead;
        }

        ApiUtil.Assert(deadLeases.IsSubsetOf(leases), "Dead lease list was not a subset of the lease list");
        if (_deadTail != null)
            ApiUtil.Assert(deadLeases.Contains(_deadTail), "Dead tail is not in the dead list");
    }

    public Span<TInstance> Lock(BatchLease<TKey, TInstance> lease, ref bool lockWasTaken)
    {
        ArgumentNullException.ThrowIfNull(lease);
        PerfTracker.IncrementFrameCounter("Lease Accesses");
        Monitor.Enter(_syncRoot, ref lockWasTaken);
        return MutableInstances.Slice(lease.From, lease.Length);
    }

    public void Unlock(BatchLease<TKey, TInstance> _, bool lockWasTaken) // Might need the lease param later if we do more fine-grained locking
    {
        if (lockWasTaken)
            Monitor.Exit(_syncRoot);
    }

    internal IEnumerable<BatchLease<TKey, TInstance>> Leases => LeasesInner();
    internal IEnumerable<BatchLease<TKey, TInstance>> DeadLeases => DeadLeasesInner();

    IEnumerable<BatchLease<TKey, TInstance>> LeasesInner()
    {
        var lease = _head;
        while (lease != null)
        {
            yield return lease;
            lease = lease.Next;
        }
    }

    IEnumerable<BatchLease<TKey, TInstance>> DeadLeasesInner()
    {
        var lease = _deadHead;
        while (lease != null)
        {
            yield return lease;
            lease = lease.NextDead;
        }
    }

    protected virtual void Dispose(bool disposing) { }
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
