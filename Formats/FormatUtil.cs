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
            var curDir = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            while (curDir != null && !File.Exists(Path.Combine(curDir.FullName, "data", "assets.json")))
                curDir = curDir.Parent;

            var baseDir = curDir?.FullName;
            return baseDir;
        }
    }
}
