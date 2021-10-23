using System;
using System.Text;

namespace UAlbion.Scripting
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
        public override string ToString() => ((ICfgNode)this).ToPseudocode();
        public void ToPseudocode(StringBuilder sb, bool isStatement, bool numeric)
        {
            if (sb == null) throw new ArgumentNullException(nameof(sb));
            if (Left.Precedence < Precedence) sb.Append('(');
            Left.ToPseudocode(sb, false, numeric);
            if (Left.Precedence < Precedence) sb.Append(')');
            sb.Append(" && ");
            if (Right.Precedence < Precedence) sb.Append('(');
            Right.ToPseudocode(sb, false, numeric);
            if (Right.Precedence < Precedence) sb.Append(')');
        }
    }
}