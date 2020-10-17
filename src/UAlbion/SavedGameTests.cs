using System;
using System.IO;
using System.Linq;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats;
using UAlbion.Formats.Assets.Save;
using UAlbion.Game.Assets;

namespace UAlbion
{
    static class SavedGameTests
    {
        public static void RoundTripTest(string baseDir, IAssetLoaderRegistry assetLoaderRegistry)
        {
            var mapping = AssetMapping.Global; // TODO: Base game mapping.
            foreach (var file in Directory.EnumerateFiles(Path.Combine(baseDir, "re", "TestSaves"), "*.001"))
            {
                using var stream = File.Open(file, FileMode.Open);
                using var br = new BinaryReader(stream);
                var save = SavedGame.Serdes(null, mapping, new AlbionReader(br, stream.Length));

                using var ms = new MemoryStream();
                using var bw = new BinaryWriter(ms);
                SavedGame.Serdes(save, mapping, new AlbionWriter(bw));

                br.BaseStream.Position = 0;
                var originalBytes = br.ReadBytes((int)stream.Length);
                var roundTripBytes = ms.ToArray();

                //* Save round-tripped and annotated text output for debugging
                File.WriteAllBytes(file + ".bin", roundTripBytes);
                using var ts = new MemoryStream();
                using var tw = new StreamWriter(ts);
                SavedGame.Serdes(save, mapping, new AnnotatedFormatWriter(tw));
                ts.Position = 0;
                File.WriteAllBytes(file + ".txt", ts.ToArray());
                //*/

                ApiUtil.Assert(originalBytes.Length == roundTripBytes.Length);
                ApiUtil.Assert(originalBytes.SequenceEqual(roundTripBytes));

                var sw = new StringWriter();
                SavedGame.Serdes(save, mapping, new JsonWriter(sw));
                File.WriteAllText(file + ".json", sw.ToString());
                break;
            }

            Console.ReadLine();
        }
    }
}
