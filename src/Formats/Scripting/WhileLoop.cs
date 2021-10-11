using System;
using System.Text;

namespace UAlbion.Formats.Scripting
{
    public class WhileLoop : ICfgNode
    {
        public WhileLoop(ICondition condition, ICfgNode body)
        {
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            Body = body;
        }

        public ICondition Condition { get; }
        public ICfgNode Body { get; }

        public override string ToString() => ((ICfgNode)this).ToPseudocode();
        public void ToPseudocode(StringBuilder sb, bool isStatemment, bool numeric)
        {
            if (sb == null) throw new ArgumentNullException(nameof(sb));
            sb.Append("while (");
            Condition.ToPseudocode(sb, false, numeric);
            sb.Append(") { ");
            Body?.ToPseudocode(sb, true, numeric);
            sb.Append("} ");
        }
    }
}