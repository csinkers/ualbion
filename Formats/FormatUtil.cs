using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace UAlbion.Formats
{
    public static class FormatUtil
    {
        public static string BytesTo850String(byte[] bytes) =>
            Encoding.GetEncoding(850)
                .GetString(bytes)
                .Replace("×", "ß")
                .TrimEnd((char) 0);

        public static byte[] BytesFrom850String(string str) =>
            Encoding.GetEncoding(850)
                .GetBytes(str.Replace("ß", "×"));

        public static string FindBasePath()
        {
            var exeLocation = Assembly.GetExecutingAssembly().Location;
            var curDir = new DirectoryInfo(Path.GetDirectoryName(exeLocation) ?? throw new InvalidOperationException());
            while (curDir != null && !File.Exists(Path.Combine(curDir.FullName, "data", "assets.json")))
                curDir = curDir.Parent;

            var baseDir = curDir?.FullName;
            return baseDir;
        }

        public static string WordWrap(string s, int maxLine)
        {
            if (s.Length <= maxLine)
                return s;

            int n = 0;
            var sb = new StringBuilder();
            foreach (var c in s)
            {
                n = c == '\n' ? 0 : n + 1;

                sb.Append(c);
                if (n == maxLine)
                {
                    sb.AppendLine();
                    n = 0;
                }
            }
            return sb.ToString();
        }
    }
}
