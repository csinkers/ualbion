using System;
using UAlbion.Api;

namespace UAlbion.Formats.MapEvents
{
    public class DummyEventNode : IEventNode // These should only exist temporarily during deserialisation
    {
        public DummyEventNode(ushort id) => Id = id;
        public ushort Id { get; }
        public IEvent Event => throw new InvalidOperationException("All DummyEventNodes should be removed during the unswizzling process.");
        public IEventNode Next => throw new InvalidOperationException("All DummyEventNodes should be removed during the unswizzling process.");
        public override string ToString() => ToString(0);
        public string ToString(int idOffset) => $"DummyNode {Id - idOffset}";
    }
}
