using System.Collections.Generic;
using System.Diagnostics;
using UAlbion.Api;

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
            {
                if (dummy.Id >= nodes.Count)
                {
                    ApiUtil.Assert($"Invalid event id: {Id} links to {dummy.Id} when false, but the set only contains {nodes.Count} events");
                    NextIfFalse = null;
                }
                else NextIfFalse = nodes[dummy.Id];

            }
            base.Unswizzle(nodes);
        }
    }
}