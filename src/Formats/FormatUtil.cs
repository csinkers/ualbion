using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace UAlbion.Formats
{
    public static class FormatUtil
    {
        public static readonly Encoding AlbionEncoding = Encoding.GetEncoding(850);
        public static string BytesTo850String(byte[] bytes) =>
            AlbionEncoding
                .GetString(bytes)
                .Replace("×", "ß")
                .TrimEnd((char) 0);

        public static byte[] BytesFrom850String(string str)
        {
            if (str == null) throw new ArgumentNullException(nameof(str));
            return AlbionEncoding.GetBytes(str.Replace("ß", "×"));
        }

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
            if (s == null || s.Length <= maxLine)
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
