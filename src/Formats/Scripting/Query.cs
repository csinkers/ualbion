using System;
using System.Text;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Formats.Scripting
{
    public class Query : ICondition
    {
        public int Precedence => 0;
        public Query(IBranchingEvent @event) => Event = @event ?? throw new ArgumentNullException(nameof(@event));
        public IBranchingEvent Event { get; }
        public void ToPseudocode(StringBuilder sb, string indent, bool numeric = false)
        {
            if (sb == null) throw new ArgumentNullException(nameof(sb));
            sb.Append(Event);
        }
    }
}