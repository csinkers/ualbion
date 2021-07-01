using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UAlbion.Api;

namespace UAlbion.Core.Visual
{
    public abstract class SpriteBatch : Component, IDisposable
    {
        protected const int MinSize = 4;
        protected const double GrowthFactor = 1.5;
        protected const double ShrinkFactor = 0.3;

        readonly object _syncRoot = new();
        readonly List<SpriteLease> _leases = new();

        protected SpriteBatch(SpriteKey key)
        {
            Key = key;
        }

        protected abstract ReadOnlySpan<SpriteInstanceData> ReadOnlySprites { get; }
        protected abstract Span<SpriteInstanceData> MutableSprites { get; }
        protected abstract void Resize(int instanceCount);

        public SpriteKey Key { get; }
        public string Name => $"Sprite:{Key.Texture.Name}";
        public override string ToString() => $"Multi:{Name} Flags:{Key.Flags} ({ActiveInstances}/{ReadOnlySprites.Length} instances)";
        public int ActiveInstances { get; private set; }

        internal SpriteLease Grow(int length, object caller)
        {
            lock (_syncRoot)
            {
                PerfTracker.IncrementFrameCounter("Sprite Borrows");
                int from = ActiveInstances;
                ActiveInstances += length;
                if (ActiveInstances >= ReadOnlySprites.Length)
                {
                    int newSize = ReadOnlySprites.Length;
                    if (newSize < MinSize) newSize = MinSize;
                    while (newSize <= ActiveInstances)
                        newSize = (int)(newSize * GrowthFactor);

                    Resize(newSize);
                }

                var lease = new SpriteLease(this, from, ActiveInstances) { Owner = caller };
                _leases.Add(lease);
                VerifyConsistency();
                return lease;
            }
        }

        public void Shrink(SpriteLease leaseToRemove)
        {
            if (leaseToRemove == null) throw new ArgumentNullException(nameof(leaseToRemove));
            // TODO: Use a more efficient algorithm, e.g. look for equal sized lease at end of list and swap, use linked list for lease list etc
            lock (_syncRoot)
            {
                var buffer = MutableSprites;
                VerifyConsistency();
                PerfTracker.IncrementFrameCounter("Sprite Returns");
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

        public Span<SpriteInstanceData> Lock(SpriteLease lease, ref bool lockWasTaken)
        {
            if (lease == null) throw new ArgumentNullException(nameof(lease));
            PerfTracker.IncrementFrameCounter("Sprite Accesses");
            Monitor.Enter(_syncRoot, ref lockWasTaken);
            return MutableSprites.Slice(lease.From, lease.Length);
        }

        public void Unlock(SpriteLease _, bool lockWasTaken) // Might need the lease param later if we do more fine grained locking
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
}