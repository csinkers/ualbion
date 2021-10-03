using System;
using System.Text;

namespace UAlbion.Formats.Scripting
{
    public class AndCondition : ICondition
    {
        public int Precedence => 2;
        public AndCondition(ICondition left, ICondition right)
        {
            Left = left ?? throw new ArgumentNullException(nameof(left));
            Right = right ?? throw new ArgumentNullException(nameof(right));
        }

        public ICondition Left { get; }
        public ICondition Right { get; }
        public void ToPseudocode(StringBuilder sb, string indent, bool numeric = false)
        {
            if (sb == null) throw new ArgumentNullException(nameof(sb));
            if (Left.Precedence < Precedence) sb.Append('(');
            Left.ToPseudocode(sb, indent, numeric);
            if (Left.Precedence < Precedence) sb.Append(')');
            sb.Append(" && ");
            if (Right.Precedence < Precedence) sb.Append('(');
            Right.ToPseudocode(sb, indent, numeric);
            if (Right.Precedence < Precedence) sb.Append(')');
        }
    }
}