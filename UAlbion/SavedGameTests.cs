using System.Diagnostics;
using System.IO;
using System.Linq;
using SerdesNet;
using UAlbion.Formats;
using UAlbion.Formats.Assets.Save;
using UAlbion.Formats.Config;
using UAlbion.Game.Assets;

namespace UAlbion
{
    static class SavedGameTests
    {
        public static void RoundTripTest(string baseDir)
        {
            var loader = AssetLoaderRegistry.GetLoader<SavedGame>(FileFormat.SavedGame);
            foreach (var file in Directory.EnumerateFiles(Path.Combine(baseDir, "re", "TestSaves"), "*.001"))
            {
                using var stream = File.Open(file, FileMode.Open);
                using var br = new BinaryReader(stream);
                var save = loader.Serdes(null, new AlbionReader(br, stream.Length), "TestSave", null);

                using var ms = new MemoryStream();
                using var bw = new BinaryWriter(ms);
                loader.Serdes(save, new AlbionWriter(bw), "TestSave", null);

                br.BaseStream.Position = 0;
                var originalBytes = br.ReadBytes((int)stream.Length);
                var roundTripBytes = ms.ToArray();

                //* Save round-tripped and annotated text output for debugging
                File.WriteAllBytes(file + ".bin", roundTripBytes);
                using var ts = new MemoryStream();
                using var tw = new StreamWriter(ts);
                loader.Serdes(save, new AnnotatedFormatWriter(tw), "TestSave", null);
                ts.Position = 0;
                File.WriteAllBytes(file + ".txt", ts.ToArray());
                //*/

                Debug.Assert(originalBytes.Length == roundTripBytes.Length);
                Debug.Assert(originalBytes.SequenceEqual(roundTripBytes));
            }
        }
    }
}
