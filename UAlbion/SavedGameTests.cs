using System.IO;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;
using UAlbion.Formats.Parsers;
using UAlbion.Game.Assets;

namespace UAlbion
{
    static class SavedGameTests
    {
        public static void Run(string baseDir)
        {
            var loader = AssetLoaderRegistry.GetLoader<SavedGame>(FileFormat.SavedGame);
            foreach (var file in Directory.EnumerateFiles(Path.Combine(baseDir, "re", "TestSaves"), "*.001"))
            {
                using var stream = File.Open(file, FileMode.Open);
                using var br = new BinaryReader(stream);
                var save = loader.Serdes(null, new GenericBinaryReader(br, stream.Length), "TestSave", null);

                using var ms = new MemoryStream();
                using var bw = new BinaryWriter(ms);
                loader.Serdes(save, new GenericBinaryWriter(bw), "TestSave", null);

                // Debug.Assert(stream.Length == ms.Length);
                br.BaseStream.Position = 0;
                var originalBytes = br.ReadBytes((int)stream.Length);
                var roundTripBytes = ms.ToArray();
                // Debug.Assert(originalBytes == roundTripBytes);

                using var ts = new MemoryStream();
                using var tw = new StreamWriter(ts);
                loader.Serdes(save, new AnnotatedFormatWriter(tw), "TestSave", null);
                ts.Position = 0;
                File.WriteAllBytes(file + ".txt", ts.ToArray());
            }
        }
    }
}
