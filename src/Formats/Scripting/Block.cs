using System;
using System.Text;
using UAlbion.Api;

namespace UAlbion.Formats.Scripting
{
    public class Block : ICondition
    {
        public Block(IEvent e) => Event = e ?? throw new ArgumentNullException(nameof(e));
        IEvent Event { get; }

        public override string ToString() => ((ICfgNode)this).ToPseudocode();
        public void ToPseudocode(StringBuilder sb, bool isStatement, bool numeric)
        {
            if (sb == null) throw new ArgumentNullException(nameof(sb));
            sb.Append(numeric ? Event.ToStringNumeric() : Event.ToString());
            if (isStatement)
                sb.Append("; ");
        }

        public int Precedence => 1;
    }
}