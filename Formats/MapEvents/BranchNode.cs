using System.Collections.Generic;
using System.Diagnostics;

namespace UAlbion.Formats.MapEvents
{
    [DebuggerDisplay("{ToString()}")]
    public class BranchNode : EventNode, IBranchNode
    {
        public BranchNode(ushort id, IMapEvent @event) : base(id, @event) { }
        public override string ToString() => $"!{Id}?{Next?.Id.ToString() ?? "!"}:{NextIfFalse?.Id.ToString() ?? "!"}: {Event}";
        public IEventNode NextIfFalse { get; set; }
        public override void Unswizzle(IList<EventNode> nodes)
        {
            if (NextIfFalse is DummyEventNode dummy)
                NextIfFalse = nodes[dummy.Id];
            base.Unswizzle(nodes);
        }
    }
}