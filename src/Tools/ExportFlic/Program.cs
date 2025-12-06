using System;
using System.IO;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets.Flic;
using UAlbion.Formats;

namespace UAlbion.Tools.ExportFlic;

class Program
{
    const string RelativePath = @"data\Exported\ENGLISH\FLICS0.XLD";
    static void Main()
    {
        var disk = new FileSystem(Directory.GetCurrentDirectory());
        var baseDir = ConfigUtil.FindBasePath(disk);
        if (baseDir == null)
            throw new InvalidOperationException("No base directory could be found.");

        var dir = Path.Combine(baseDir, RelativePath);

        var files = Directory.EnumerateFiles(dir, "*.bin");
        foreach (var file in files)
        {
            using var stream = File.OpenRead(file);
            using var s = AlbionSerdes.CreateReader(stream);
            var flic = new FlicFile(s);
            var buffer = new byte[flic.Width * flic.Height];

            AviFile.Write(
                Path.ChangeExtension(file, "avi"),
                flic.Speed,
                flic.Width,
                flic.Height,
                flic.Play(() => buffer).AllFrames32());
            // break;
        }
    }
/*
    static IEnumerable<byte[]> FrameGenerator(int width, int height)
    {
        byte[] frame = new byte[width * height * 3];
        for (byte i = 0; i < 255; i++)
        {
            Array.Fill(frame, i);
            yield return frame;
        }
    }
*/
}