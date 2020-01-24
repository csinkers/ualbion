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

        public static uint? Tweak(uint x)
        {
            if (x == 0) return null;
            if (x < 100) return x - 1;
            return x;
        }

        public static ushort? Tweak(ushort x)
        {
            if (x == 0) return null;
            if (x < 100) return (ushort?)(x - 1);
            return x;
        }

        public static byte? Tweak(byte x)
        {
            if (x == 0) return null;
            if (x < 100) return (byte?)(x - 1);
            return x;
        }

        public static int? Tweak(int x)
        {
            if (x == 0) return null;
            if (x < 100) return x - 1;
            return x;
        }

        public static short? Tweak(short x)
        {
            if (x == 0) return null;
            if (x < 100) return (short?)(x - 1);
            return x;
        }

        public static uint Untweak(uint? x)
        {
            if (x == null) return 0;
            if (x < 99) return x.Value + 1;
            return x.Value;
        }

        public static ushort Untweak(ushort? x)
        {
            if (x == null) return 0;
            if (x < 99) return (ushort)(x.Value + 1);
            return x.Value;
        }

        public static byte Untweak(byte? x)
        {
            if (x == null) return 0;
            if (x < 99) return (byte)(x.Value + 1);
            return x.Value;
        }

        public static int Untweak(int? x)
        {
            if (x == null) return 0;
            if (x < 99) return x.Value + 1;
            return x.Value;
        }

        public static short Untweak(short? x)
        {
            if (x == null) return 0;
            if (x < 99) return (short)(x.Value + 1);
            return x.Value;
        }
    }
}
