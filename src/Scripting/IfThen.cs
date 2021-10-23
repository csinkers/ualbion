using System;
using System.Text;

namespace UAlbion.Scripting
{
    public class IfThen : ICfgNode
    {
        public IfThen(ICondition condition, ICfgNode body)
        {
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            Body = body ?? throw new ArgumentNullException(nameof(body));
        }

        public ICondition Condition { get; }
        public ICfgNode Body { get; }

        public override string ToString() => ((ICfgNode)this).ToPseudocode();
        public void ToPseudocode(StringBuilder sb, bool isStatement, bool numeric = false)
        {
            if (sb == null) throw new ArgumentNullException(nameof(sb));
            sb.Append("if (");
            Condition.ToPseudocode(sb, false, numeric);
            sb.Append(") { ");
            Body.ToPseudocode(sb, true, numeric);
            sb.Append("} ");
        }
    }
}