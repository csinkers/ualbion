using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UAlbion.Api;
using UAlbion.Api.Visual;

namespace UAlbion.Core.Visual
{
    public class MultiSprite : IRenderable
    {
        const int MinSize = 4;
        const double GrowthFactor = 1.5;
        const double ShrinkFactor = 0.3;

        public MultiSprite(SpriteKey key) { Key = key; }
        public override string ToString() => $"Multi:{Name} {RenderOrder} Flags:{Key.Flags} ({ActiveInstances}/{_instances.Length} instances)";

        string _name;
        public string Name
        {
            get => _name ?? $"Sprite:{Key.Texture.Name}:{Key.RenderOrder}";
            set => _name = value;
        }

        public DrawLayer RenderOrder => Key.RenderOrder;
        public int PipelineId { get; set; }
        public SpriteKey Key { get; }
        public bool InstancesDirty { get; set; }
        public int ActiveInstances { get; private set; }
        public ReadOnlySpan<SpriteInstanceData> Instances => _instances;
        readonly List<SpriteLease> _leases = new List<SpriteLease>();
        readonly object _syncRoot = new object();
        SpriteInstanceData[] _instances = new SpriteInstanceData[MinSize];

        internal SpriteLease Grow(int length, object caller)
        {
            lock (_syncRoot)
            {
                PerfTracker.IncrementFrameCounter("Sprite Borrows");
                int from = ActiveInstances;
                ActiveInstances += length;
                if (ActiveInstances >= _instances.Length)
                {
                    int newSize = _instances.Length;
                    while (newSize <= ActiveInstances)
                        newSize = (int) (newSize * GrowthFactor);

                    var newArray = new SpriteInstanceData[newSize];
                    for (int i = 0; i < _instances.Length; i++)
                        newArray[i] = _instances[i];
                    _instances = newArray;
                }

                var lease = new SpriteLease(this, from, ActiveInstances) { Owner = caller };
                _leases.Add(lease);
                VerifyConsistency();
                return lease;
            }
        }

        internal void Shrink(SpriteLease leaseToRemove)
        {
            // TODO: Use a more efficient algorithm, e.g. look for equal sized lease at end of list and swap, use linked list for lease list etc
            lock (_syncRoot)
            {
                VerifyConsistency();
                PerfTracker.IncrementFrameCounter("Sprite Returns");
                bool shifting = false;
                for (int n = 0; n < _leases.Count; n++)
                {
                    if (!shifting && _leases[n].From == leaseToRemove.From)
                    {
                        _leases.RemoveAt(n);
                        ActiveInstances -= leaseToRemove.Length;
                        InstancesDirty = true;
                        shifting = true;
                    }

                    if (shifting && n < _leases.Count)
                    {
                        var lease = _leases[n];
                        for (int i = lease.From; i < lease.To; i++)
                            _instances[i - leaseToRemove.Length] = _instances[i];
                        lease.From -= leaseToRemove.Length;
                        lease.To -= leaseToRemove.Length;
                    }
                }
                VerifyConsistency();

                if ((double)ActiveInstances / _instances.Length < ShrinkFactor)
                {
                    int newSize = _instances.Length;
                    while ((double)ActiveInstances / newSize > ShrinkFactor)
                        newSize = (int)(newSize * ShrinkFactor);
                    newSize = Math.Max(newSize, ActiveInstances);
                    newSize = Math.Max(newSize, MinSize);

                    if (newSize != _instances.Length)
                    {
                        var newArray = new SpriteInstanceData[newSize];
                        for (int i = 0; i < ActiveInstances; i++)
                            newArray[i] = _instances[i];
                        _instances = newArray;
                    }
                }
            }
        }

        [Conditional("DEBUG")]
        void VerifyConsistency()
        {
            if (_leases.Count > 0)
            {
                // Assert that all leases are in order
                foreach(var lease in _leases)
                    ApiUtil.Assert(lease.Length > 0);
                ApiUtil.Assert(_leases[0].From == 0);
                for (int i = 1; i < _leases.Count; i++)
                    ApiUtil.Assert(_leases[i - 1].To == _leases[i].From);
                ApiUtil.Assert(_leases[^1].To == ActiveInstances);
            }
            else ApiUtil.Assert(ActiveInstances == 0);
        }

        internal Span<SpriteInstanceData> Lock(SpriteLease lease, ref bool lockWasTaken)
        {
            PerfTracker.IncrementFrameCounter("Sprite Accesses");
            Monitor.Enter(_syncRoot, ref lockWasTaken);
            InstancesDirty = true;
            return new Span<SpriteInstanceData>(_instances, lease.From, lease.Length);
        }

        internal void Unlock(SpriteLease _, bool lockWasTaken) // Might need the lease param later if we do more fine grained locking
        {
            if (lockWasTaken)
                Monitor.Exit(_syncRoot);
        }
    }
}

