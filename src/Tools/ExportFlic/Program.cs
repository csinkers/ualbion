using System;
using System.IO;
using System.Collections.Generic;
using UAlbion.Config;
using UAlbion.Formats.Assets.Flic;
using UAlbion.Formats;

namespace UAlbion.Tools.ExportFlic
{
    class Program
    {
        const string RelativePath = @"data\Exported\ENGLISH\FLICS0.XLD";
        static void Main()
        {
            var baseDir = ConfigUtil.FindBasePath();
            if (baseDir == null)
                throw new InvalidOperationException("No base directory could be found.");

            var dir = Path.Combine(baseDir, RelativePath);

            var files = Directory.EnumerateFiles(dir, "*.bin");
            foreach (var file in files)
            {
                using var stream = File.OpenRead(file);
                using var br = new BinaryReader(stream);
                using var s = new AlbionReader(br);
                var flic = new FlicFile(s);

                AviFile.Write(
                    Path.ChangeExtension(file, "avi"),
                    flic.Speed,
                    flic.Width,
                    flic.Height,
                    flic.Play(new byte[flic.Width * flic.Height]).AllFrames32());
                // break;
            }
        }

        static IEnumerable<byte[]> FrameGenerator(int width, int height)
        {
            byte[] frame = new byte[width * height * 3];
            for (byte i = 0; i < 255; i++)
            {
                Array.Fill(frame, i);
                yield return frame;
            }
        }
    }
}
