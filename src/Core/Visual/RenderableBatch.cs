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
    protected const int MinSize = 4;
    protected const double GrowthFactor = 1.5;
    protected const double ShrinkFactor = 0.3;

    readonly object _syncRoot = new();
    readonly List<BatchLease<TKey, TInstance>> _leases = new();

    protected RenderableBatch(TKey key)
    {
        Key = key;
        Name = $"Batch:{Key}";
    }

    protected abstract ReadOnlySpan<TInstance> ReadOnlyInstances { get; }
    protected abstract Span<TInstance> MutableInstances { get; }
    protected abstract void Resize(int instanceCount);

    public TKey Key { get; }
    public string Name { get; }
    public DrawLayer RenderOrder => Key.RenderOrder;
    public override string ToString() => $"Multi:{Name} ({ActiveInstances}/{ReadOnlyInstances.Length} instances)";
    public int ActiveInstances { get; private set; }

    internal BatchLease<TKey, TInstance> Grow(int length, object caller)
    {
        lock (_syncRoot)
        {
            PerfTracker.IncrementFrameCounter("Lease Borrows");
            int from = ActiveInstances;
            ActiveInstances += length;
            if (ActiveInstances >= ReadOnlyInstances.Length)
            {
                int newSize = ReadOnlyInstances.Length;
                if (newSize < MinSize) newSize = MinSize;
                while (newSize <= ActiveInstances)
                    newSize = (int)(newSize * GrowthFactor);

                Resize(newSize);
            }

            var lease = new BatchLease<TKey, TInstance>(this, from, ActiveInstances) { Owner = caller };
            _leases.Add(lease);
            VerifyConsistency();
            return lease;
        }
    }

    public void Shrink(BatchLease<TKey, TInstance> leaseToRemove)
    {
        if (leaseToRemove == null) throw new ArgumentNullException(nameof(leaseToRemove));
        // TODO: Use a more efficient algorithm, e.g. look for equal sized lease at end of list and swap, use linked list for lease list etc
        lock (_syncRoot)
        {
            var buffer = MutableInstances;
            VerifyConsistency();
            PerfTracker.IncrementFrameCounter("Lease Returns");
            bool shifting = false;
            for (int n = 0; n < _leases.Count; n++)
            {
                if (!shifting && _leases[n].From == leaseToRemove.From)
                {
                    _leases.RemoveAt(n);
                    ActiveInstances -= leaseToRemove.Length;
                    shifting = true;
                }

                if (shifting && n < _leases.Count)
                {
                    var lease = _leases[n];
                    for (int i = lease.From; i < lease.To; i++)
                        buffer[i - leaseToRemove.Length] = buffer[i];
                    lease.From -= leaseToRemove.Length;
                    lease.To -= leaseToRemove.Length;
                }
            }
            VerifyConsistency();

            if ((double)ActiveInstances / buffer.Length < ShrinkFactor)
            {
                int newSize = buffer.Length;
                while ((double)ActiveInstances / newSize > ShrinkFactor)
                    newSize = (int)(newSize * ShrinkFactor);
                newSize = Math.Max(newSize, ActiveInstances);
                newSize = Math.Max(newSize, MinSize);
                Resize(newSize);
            }
        }
    }

    [Conditional("DEBUG")]
    void VerifyConsistency()
    {
        if (_leases.Count > 0)
        {
            // Assert that all leases are in order
            foreach (var lease in _leases)
                ApiUtil.Assert(lease.Length > 0);
            ApiUtil.Assert(_leases[0].From == 0);
            for (int i = 1; i < _leases.Count; i++)
                ApiUtil.Assert(_leases[i - 1].To == _leases[i].From);
            ApiUtil.Assert(_leases[^1].To == ActiveInstances);
        }
        else ApiUtil.Assert(ActiveInstances == 0);
    }

    public Span<TInstance> Lock(BatchLease<TKey, TInstance> lease, ref bool lockWasTaken)
    {
        if (lease == null) throw new ArgumentNullException(nameof(lease));
        PerfTracker.IncrementFrameCounter("Lease Accesses");
        Monitor.Enter(_syncRoot, ref lockWasTaken);
        return MutableInstances.Slice(lease.From, lease.Length);
    }

    public void Unlock(BatchLease<TKey, TInstance> _, bool lockWasTaken) // Might need the lease param later if we do more fine grained locking
    {
        if (lockWasTaken)
            Monitor.Exit(_syncRoot);
    }

    protected virtual void Dispose(bool disposing) { }
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}