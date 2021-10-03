using System;
using System.Text;

namespace UAlbion.Formats.Scripting
{
    public class BreakNode : ICfgNode
    {
        public BreakNode(ICfgNode body) => Body = body;
        public ICfgNode Body { get; }
        public void ToPseudocode(StringBuilder sb, string indent, bool numeric = false)
        {
            if (sb == null) throw new ArgumentNullException(nameof(sb));
            Body?.ToPseudocode(sb, indent, numeric);
            sb.Append(indent);
            sb.AppendLine("break;");
        }
    }
}