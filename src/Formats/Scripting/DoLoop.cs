using System;
using System.Text;

namespace UAlbion.Formats.Scripting
{
    public class DoLoop : ICfgNode
    {
        public DoLoop(ICfgNode condition, ICfgNode body)
        {
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            Body = body;
        }

        public ICfgNode Condition { get; }
        public ICfgNode Body { get; }
        public void ToPseudocode(StringBuilder sb, string indent, bool numeric = false)
        {
            if (sb == null) throw new ArgumentNullException(nameof(sb));
            sb.Append(indent);
            sb.Append("do {");
            Body?.ToPseudocode(sb, indent + "    ", numeric);

            sb.Append(indent);
            sb.Append("} while (");
            Condition.ToPseudocode(sb, indent, numeric);
            sb.AppendLine(")");
        }
    }
}