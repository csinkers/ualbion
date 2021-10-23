using System;
using System.Text;

namespace UAlbion.Scripting
{
    public class DummyNode : ICondition // Used for unit tests
    {
        public DummyNode(string text) => Text = text;
        public string Text { get; }
        public override string ToString() => "D:" + Text;

        public void ToPseudocode(StringBuilder sb, bool isStatement, bool numeric)
        {
            if (sb == null) throw new ArgumentNullException(nameof(sb));
            sb.Append(Text);
            if (isStatement)
                sb.Append("; ");
        }

        public int Precedence => int.MaxValue;
    }
}