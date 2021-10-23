using System;
using System.Text;

namespace UAlbion.Scripting
{
    public class IfThenElse : ICfgNode
    {
        public IfThenElse(ICondition condition, ICfgNode trueBody, ICfgNode falseBody)
        {
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            True = trueBody;
            False = falseBody ?? throw new ArgumentNullException(nameof(falseBody));
        }

        public ICondition Condition { get; }
        public ICfgNode True { get; }
        public ICfgNode False { get; }

        public override string ToString() => ((ICfgNode)this).ToPseudocode();
        public void ToPseudocode(StringBuilder sb, bool isStatement, bool numeric)
        {
            if (sb == null) throw new ArgumentNullException(nameof(sb));
            sb.Append("if (");
            Condition.ToPseudocode(sb, false, numeric);
            sb.Append(") { ");
            True?.ToPseudocode(sb, true, numeric);
            sb.Append("} else { ");
            False.ToPseudocode(sb, true, numeric);
            sb.Append("} ");
        }
    }
}