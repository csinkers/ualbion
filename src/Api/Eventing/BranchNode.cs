using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace UAlbion.Api.Eventing;

[DebuggerDisplay("{ToString()}")]
public class BranchNode : EventNode, IBranchNode
{
    public BranchNode(ushort id, IBranchingEvent @event) : base(id, @event) { }
    public override string ToString()
    {
        var builder = new UnformattedScriptBuilder(false);
        Format(builder, 0);
        return builder.Build();
    }

    public override void Format(IScriptBuilder builder, int idOffset)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));
        var id = Id - idOffset;
        var ifTrue = (Next?.Id - idOffset)?.ToString() ?? "!";
        var ifFalse = (NextIfFalse?.Id - idOffset)?.ToString() ?? "!";

        builder.Append('!');
        builder.Append(id);
        builder.Append('?');
        builder.Append(ifTrue);
        builder.Append(':');
        builder.Append(ifFalse);
        builder.Append(": ");
        Event.Format(builder);
    }

    public IEventNode NextIfFalse { get; set; }
    public override void Unswizzle(IList<EventNode> nodes)
    {
        if (nodes == null) throw new ArgumentNullException(nameof(nodes));
        if (NextIfFalse is DummyEventNode dummy)
        {
            if (dummy.Id >= nodes.Count)
            {
                // ApiUtil.Assert($"Invalid event id: {Id} links to {dummy.Id} when false, but the set only contains {nodes.Count} events");
                NextIfFalse = null;
            }
            else NextIfFalse = nodes[dummy.Id];

        }
        base.Unswizzle(nodes);
    }
}
