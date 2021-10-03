using System;
using System.Text;

namespace UAlbion.Formats.Scripting
{
    public class Negation : ICondition
    {
        public int Precedence => 1;
        public Negation(ICondition condition) => Condition = condition ?? throw new ArgumentNullException(nameof(condition));
        public ICondition Condition { get; }
        public void ToPseudocode(StringBuilder sb, string indent, bool numeric = false)
        {
            if (sb == null) throw new ArgumentNullException(nameof(sb));
            sb.Append('!');
            if (Condition.Precedence < Precedence) sb.Append('(');
            Condition.ToPseudocode(sb, indent, numeric);
            if (Condition.Precedence < Precedence) sb.Append(')');
        }
    }
}