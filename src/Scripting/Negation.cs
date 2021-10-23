using System;
using System.Text;

namespace UAlbion.Scripting
{
    public class Negation : ICondition
    {
        public int Precedence => 1;
        public Negation(ICondition condition) => Condition = condition ?? throw new ArgumentNullException(nameof(condition));
        public ICondition Condition { get; }
        public override string ToString() => ((ICfgNode)this).ToPseudocode();
        public void ToPseudocode(StringBuilder sb, bool isStatement, bool numeric)
        {
            if (sb == null) throw new ArgumentNullException(nameof(sb));
            sb.Append('!');
            if (Condition.Precedence < Precedence) sb.Append('(');
            Condition.ToPseudocode(sb, false, numeric);
            if (Condition.Precedence < Precedence) sb.Append(')');
        }
    }
}