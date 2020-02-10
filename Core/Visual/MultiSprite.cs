using System;
using System.Collections.Generic;
using UAlbion.Api;

namespace UAlbion.Core.Visual
{
    public class MultiSprite : IRenderable
    {
        const int MinSize = 4;
        const double GrowthFactor = 1.5;
        const double ShrinkFactor = 0.3;

        public MultiSprite(SpriteKey key) { Key = key; }
        public override string ToString() => $"Multi:{Name} {RenderOrder} Flags:{Key.Flags} ({ActiveInstances}/{Instances.Length} instances)";

        string _name;
        public string Name
        {
            get => _name ?? Key.Texture.Name;
            set => _name = value;
        }

        public DrawLayer RenderOrder => Key.RenderOrder;
        public Type Renderer => typeof(SpriteRenderer);

        public SpriteKey Key { get; }
        public int BufferId { get; set; }
        public bool InstancesDirty { get; private set; }
        public int ActiveInstances { get; private set; }
        internal SpriteInstanceData[] Instances { get; private set; } = new SpriteInstanceData[MinSize];
        readonly List<SpriteLease> _leases = new List<SpriteLease>();

        internal SpriteLease Grow(int length, object caller)
        {
            PerfTracker.IncrementFrameCounter("Sprite Borrows");
            int from = ActiveInstances;
            ActiveInstances += length;
            if (ActiveInstances >= Instances.Length)
            {
                int newSize = Instances.Length;
                while (newSize <= ActiveInstances)
                    newSize = (int)(newSize * GrowthFactor);

                var newArray = new SpriteInstanceData[newSize];
                for (int i = 0; i < Instances.Length; i++)
                    newArray[i] = Instances[i];
                Instances = newArray;
            }

            var lease = new SpriteLease(this, from, ActiveInstances);
#if DEBUG
            lease.Owner = caller;
#endif
            _leases.Add(lease);
            return lease;
        }

        internal void Shrink(SpriteLease leaseToRemove)
        {
            // TODO: Use a more efficient algorithm, e.g. look for equal sized lease at end of list and swap, use linked list for lease list etc

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
                        Instances[i - leaseToRemove.Length] = Instances[i];
                    lease.From -= leaseToRemove.Length;
                    lease.To -= leaseToRemove.Length;
                }
            }

            if ((double)ActiveInstances / Instances.Length < ShrinkFactor)
            {
                int newSize = Instances.Length;
                while ((double) ActiveInstances / newSize > ShrinkFactor)
                    newSize = (int)(newSize * ShrinkFactor);
                newSize = Math.Max(newSize, ActiveInstances);
                newSize = Math.Max(newSize, MinSize);

                if (newSize != Instances.Length)
                {
                    var newArray = new SpriteInstanceData[newSize];
                    for (int i = 0; i < ActiveInstances; i++)
                        newArray[i] = Instances[i];
                    Instances = newArray;
                }
            }
        }

        internal Span<SpriteInstanceData> GetSpan(SpriteLease lease)
        {
            PerfTracker.IncrementFrameCounter("Sprite Accesses");
            InstancesDirty = true;
            return new Span<SpriteInstanceData>(Instances, lease.From, lease.Length);
        }
    }
}

