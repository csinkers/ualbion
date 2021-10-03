using System.Text;

namespace UAlbion.Formats.Scripting
{
    public class EmptyNode : ICfgNode // Used for empty entry / exit nodes
    {
        public void ToPseudocode(StringBuilder sb, string indent, bool numeric = false) { }
    }
}