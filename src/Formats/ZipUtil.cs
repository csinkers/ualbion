using System;
using System.IO;
using System.Runtime.InteropServices;
using ICSharpCode.SharpZipLib.Zip.Compression;

namespace UAlbion.Formats
{
    public static class ZipUtil
    {
        public static int[] Inflate(byte[] compressed)
        {
            if (compressed == null) throw new ArgumentNullException(nameof(compressed));
            var inflater = new Inflater();
            inflater.SetInput(compressed);

            using var ms = new MemoryStream(compressed.Length);
            byte[] buf = new byte[1024];
            while (!inflater.IsFinished)
            {
                int count = inflater.Inflate(buf);
                ms.Write(buf, 0, count);
            }

            ms.Position = 0;

            var results = new int[(int)(ms.Length / 4)];
            using var br = new BinaryReader(ms);
            for (int i = 0; i < ms.Length >> 2; i++)
                results[i] = br.ReadInt32();
            return results;
        }

        public static byte[] Deflate(ReadOnlySpan<int> ints)
        {
            if (ints == null) throw new ArgumentNullException(nameof(ints));
            byte[] input = new byte[ints.Length * sizeof(int)];
            var asBytes = MemoryMarshal.Cast<int, byte>(ints);
            asBytes.CopyTo(input.AsSpan());

            var deflater = new Deflater();
            deflater.SetLevel(Deflater.DEFAULT_COMPRESSION);
            deflater.SetInput(input);
            deflater.Finish();

            using var ms = new MemoryStream(input.Length);
            byte[] buf = new byte[1024];
            while (!deflater.IsFinished)
            {
                int count = deflater.Deflate(buf);
                ms.Write(buf, 0, count);
            }

            return ms.ToArray();
        }
    }
}
