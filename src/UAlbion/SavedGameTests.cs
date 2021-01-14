using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats;
using UAlbion.Formats.Assets.Save;

namespace UAlbion
{
    static class SavedGameTests
    {
        public static void RoundTripTest(string baseDir)
        {
            var mapping = AssetMapping.Global; // TODO: Base game mapping.
            var saveDir = Path.Combine(baseDir, "re", "TestSaves");
            var regex = new Regex(@"\.[0-9][0-9][0-9]$");
            foreach (var file in Directory.EnumerateFiles(saveDir))
            {
                if (!regex.IsMatch(file))
                    continue;
                Console.WriteLine("Round-trip testing " + file);
                using var stream = File.Open(file, FileMode.Open);
                using var br = new BinaryReader(stream);
                using var ar = new AlbionReader(br, stream.Length);
                var save = SavedGame.Serdes(null, mapping, ar);

                using var ms = new MemoryStream();
                using var bw = new BinaryWriter(ms);
                using var aw = new AlbionWriter(bw);
                SavedGame.Serdes(save, mapping, aw);

                br.BaseStream.Position = 0;
                var originalBytes = br.ReadBytes((int)stream.Length);
                var roundTripBytes = ms.ToArray();

                //* Save round-tripped and annotated text output for debugging
                File.WriteAllBytes(file + ".bin", roundTripBytes);
                using var ts = new MemoryStream();
                using var tw = new StreamWriter(ts);
                using var afw = new AnnotatedFormatWriter(tw);
                SavedGame.Serdes(save, mapping, afw);
                ts.Position = 0;
                File.WriteAllBytes(file + ".txt", ts.ToArray());
                //*/

                ApiUtil.Assert(originalBytes.Length == roundTripBytes.Length);
                ApiUtil.Assert(originalBytes.SequenceEqual(roundTripBytes));

                var sw = new StringWriter();
                using var jw = new JsonWriter(sw);
                SavedGame.Serdes(save, mapping, jw);
                File.WriteAllText(file + ".json", sw.ToString());
                break;
            }

            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}
