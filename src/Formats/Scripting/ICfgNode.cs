using System.Text;

namespace UAlbion.Formats.Scripting
{
    public interface ICfgNode
    {
        void ToPseudocode(StringBuilder sb, string indent, bool numeric = false);
    }
}
