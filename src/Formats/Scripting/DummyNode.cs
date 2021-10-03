using System;
using System.Text;

namespace UAlbion.Formats.Scripting
{
    public class DummyNode : ICfgNode // Used for unit tests
    {
        public DummyNode(string text) => Text = text;
        public string Text { get; }
        public override string ToString() => "D:" + Text;

        public void ToPseudocode(StringBuilder sb, string indent, bool numeric)
        {
            if (sb == null) throw new ArgumentNullException(nameof(sb));
            sb.Append(indent);
            sb.AppendLine(Text);
        }
    }
}