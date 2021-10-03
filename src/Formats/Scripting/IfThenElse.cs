using System;
using System.Text;

namespace UAlbion.Formats.Scripting
{
    public class IfThenElse : ICfgNode
    {
        public IfThenElse(ICfgNode condition, ICfgNode trueBody, ICfgNode falseBody)
        {
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            True = trueBody;
            False = falseBody;
        }

        public ICfgNode Condition { get; }
        public ICfgNode True { get; }
        public ICfgNode False { get; }
        public void ToPseudocode(StringBuilder sb, string indent, bool numeric = false)
        {
            if (sb == null) throw new ArgumentNullException(nameof(sb));
            sb.Append(indent);
            sb.Append("if (");
            Condition.ToPseudocode(sb, indent, numeric);
            sb.AppendLine(") {");

            var extra = indent + "    ";
            True?.ToPseudocode(sb, extra, numeric);

            if (False != null)
            {
                sb.Append(indent);
                sb.AppendLine("} else {");
                False.ToPseudocode(sb, extra, numeric);
            }

            sb.Append(indent);
            sb.AppendLine("}");
        }
    }
}