using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
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

                using var preStream = new MemoryStream();
                using var preTw = new StreamWriter(preStream);
                using var afr = new AnnotationFacadeSerializer(ar, preTw, FormatUtil.BytesFrom850String);

                var save = SavedGame.Serdes(null, mapping, afr);

                using var ms = new MemoryStream();
                using var bw = new BinaryWriter(ms);
                using var aw = new AlbionWriter(bw);

                using var postStream = new MemoryStream();
                using var postTw = new StreamWriter(postStream);
                using var afw = new AnnotationFacadeSerializer(aw, postTw, FormatUtil.BytesFrom850String);
                SavedGame.Serdes(save, mapping, afw);

                br.BaseStream.Position = 0;
                var originalBytes = br.ReadBytes((int)stream.Length);
                var roundTripBytes = ms.ToArray();

                //* Save round-tripped and annotated text output for debugging
                File.WriteAllBytes(file + ".bin", roundTripBytes);
                preStream.Position = 0;
                postStream.Position = 0;
                File.WriteAllBytes(file + ".pre.txt", preStream.ToArray());
                File.WriteAllBytes(file + ".pst.txt", postStream.ToArray());
                //*/

                ApiUtil.Assert(originalBytes.Length == roundTripBytes.Length);
                ApiUtil.Assert(originalBytes.SequenceEqual(roundTripBytes));

                File.WriteAllText(file + ".json", JsonConvert.SerializeObject(save, Formatting.Indented));
                break;
            }

            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}
