using System.Text;

namespace UAlbion.Formats.Scripting
{
    public interface ICfgNode
    {
        void ToPseudocode(StringBuilder sb, bool isStatement, bool numeric);

        string ToPseudocode()
        {
            var sb = new StringBuilder();
            ToPseudocode(sb, true, false);
            return FormatUtil.PrettyPrintScript(sb.ToString());
        }
    }
}
