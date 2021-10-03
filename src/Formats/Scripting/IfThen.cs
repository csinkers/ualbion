using System;
using System.Text;

namespace UAlbion.Formats.Scripting
{
    public class IfThen : ICfgNode
    {
        public IfThen(ICfgNode condition, ICfgNode body)
        {
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            Body = body ?? throw new ArgumentNullException(nameof(body));
        }

        public ICfgNode Condition { get; }
        public ICfgNode Body { get; }
        public void ToPseudocode(StringBuilder sb, string indent, bool numeric = false)
        {
            if (sb == null) throw new ArgumentNullException(nameof(sb));
            sb.Append(indent);
            sb.Append("if (");
            Condition.ToPseudocode(sb, indent, numeric);
            sb.AppendLine(") {");
            Body.ToPseudocode(sb, indent + "    ", numeric);
            sb.AppendLine("}");
        }
    }
}