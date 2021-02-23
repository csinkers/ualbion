using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using UAlbion.Api;
using static System.FormattableString;

namespace UAlbion.Formats.MapEvents
{
    [DebuggerDisplay("{ToString()}")]
    public class BranchNode : EventNode, IBranchNode
    {
        public BranchNode(ushort id, IBranchingEvent @event) : base(id, @event) { }
        public override string ToString() => ToString(0);

        public override string ToString(int idOffset)
        {
            var id = Id - idOffset;
            var ifTrue = (Next?.Id - idOffset)?.ToString(CultureInfo.InvariantCulture) ?? "!";
            var ifFalse = (NextIfFalse?.Id - idOffset)?.ToString(CultureInfo.InvariantCulture) ?? "!";
            return Invariant($"!{id}?{ifTrue}:{ifFalse}: {Event}");
        }

        public IEventNode NextIfFalse { get; set; }
        public override void Unswizzle(IList<EventNode> nodes)
        {
            if (nodes == null) throw new ArgumentNullException(nameof(nodes));
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
