using System;
using System.IO;
using System.Text;
using UAlbion.Formats;
using UAlbion.Formats.Assets.Save;

namespace DumpSave
{
    static class Program
    {
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // Required for code page 850 support in .NET Core
            var filename = args[0];
            var stream = File.OpenRead(filename);
            using var br = new BinaryReader(stream, Encoding.GetEncoding(850));
            var save = SavedGame.Serdes(null, new AlbionReader(br, stream.Length));
            foreach(var e in save.VisitedEvents.Contents)
                Console.WriteLine(e);
        }
    }
}
