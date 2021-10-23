using System.Text;

namespace UAlbion.Scripting
{
    public interface ICfgNode
    {
        void ToPseudocode(StringBuilder sb, bool isStatement, bool numeric);

        string ToPseudocode()
        {
            var sb = new StringBuilder();
            ToPseudocode(sb, true, false);
            var pseudo = sb.ToString();
            sb.Clear();
            ScriptUtil.PrettyPrintScript(sb, pseudo);
            return sb.ToString();
        }
    }
}
