using System.Text;

namespace UAlbion.Scripting
{
    public static class ScriptUtil
    {
        public static void PrettyPrintScript(StringBuilder sb, string text, int indent = 0)
        {
            if (text == null)
                return;

            void Indent() { for (int i = 0; i < indent; i++) sb.Append("    "); }

            bool newLine = true;
            foreach (var c in text)
            {
                switch (c)
                {
                    case ' ':
                        if (!newLine)
                            sb.Append(' ');
                        break;
                    case ';':
                        sb.AppendLine(/*";"*/);
                        newLine = true;
                        break;
                    case '{':
                        sb.Append(c);
                        indent++;
                        sb.AppendLine();
                        newLine = true;
                        break;
                    case '}':
                        if (!newLine)
                            sb.AppendLine();
                        indent--;
                        Indent();
                        sb.AppendLine("}");
                        newLine = true;
                        break;
                    default:
                        if (newLine)
                            Indent();
                        sb.Append(c);
                        newLine = false;
                        break;
                }
            }
        }

        public static string StripWhitespaceForScript(string s)
        {
            if (s == null)
                return null;

            var sb = new StringBuilder();
            bool lastWasSpace = false;

            foreach (var c in s)
            {
                switch (c)
                {
                    case ' ': case '\t': case '\n': case '\r': lastWasSpace = true; break;
                    default:
                        if (lastWasSpace)
                            sb.Append(' ');
                        sb.Append(c);
                        lastWasSpace = false;
                        break;
                }
            }

            if (lastWasSpace)
                sb.Append(' ');

            return sb.ToString().Trim();
        }
    }
}