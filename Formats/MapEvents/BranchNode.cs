using System.Diagnostics;

namespace UAlbion.Formats.MapEvents
{
    [DebuggerDisplay("{ToString()}")]
    public class BranchNode : EventNode, IBranchNode
    {
        public BranchNode(int id, IMapEvent @event, ushort? falseEventId) : base(id, @event)
        {
            NextEventWhenFalseId = falseEventId;
        }

        public override string ToString() => $"!{Id}?{NextEventId?.ToString() ?? "!"}:{NextEventWhenFalseId?.ToString() ?? "!"}: {Event}";
        public IEventNode NextEventWhenFalse { get; set; }
        public ushort? NextEventWhenFalseId { get; }
    }
}