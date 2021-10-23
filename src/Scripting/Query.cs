using System;
using System.Text;
using UAlbion.Api;

namespace UAlbion.Scripting
{
    public class Query : ICondition
    {
        public int Precedence => 0;
        public Query(IBranchingEvent @event) => Event = @event ?? throw new ArgumentNullException(nameof(@event));
        public IBranchingEvent Event { get; }
        public override string ToString() => ((ICfgNode)this).ToPseudocode();
        public void ToPseudocode(StringBuilder sb, bool isStatement, bool numeric)
        {
            if (sb == null) throw new ArgumentNullException(nameof(sb));
            sb.Append(Event);
            if (isStatement)
                sb.Append("; ");
        }
    }
}