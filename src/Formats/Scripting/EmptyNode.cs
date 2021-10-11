using System.Text;

namespace UAlbion.Formats.Scripting
{
    public class EmptyNode : ICfgNode // Used for empty entry / exit nodes
    {
        public override string ToString() => "ø";
        public void ToPseudocode(StringBuilder sb, bool isStatement, bool numeric) { }
    }
}