using System;
using System.IO;
using System.Linq;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Formats;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets.Save;
using UAlbion.Formats.Config;
using UAlbion.Game.Assets;

namespace UAlbion
{
    static class SavedGameTests
    {
        public static void RoundTripTest(string baseDir, IAssetLoaderRegistry assetLoaderRegistry)
        {
            var loader = assetLoaderRegistry.GetLoader<SavedGame>(FileFormat.SavedGame);
            ushort i = 0;
            foreach (var file in Directory.EnumerateFiles(Path.Combine(baseDir, "re", "TestSaves"), "*.001"))
            {
                var key = new AssetKey(AssetType.SavedGame, i++);
                using var stream = File.Open(file, FileMode.Open);
                using var br = new BinaryReader(stream);
                var save = loader.Serdes(null, new AlbionReader(br, stream.Length), key, null);

                using var ms = new MemoryStream();
                using var bw = new BinaryWriter(ms);
                loader.Serdes(save, new AlbionWriter(bw), key, null);

                br.BaseStream.Position = 0;
                var originalBytes = br.ReadBytes((int)stream.Length);
                var roundTripBytes = ms.ToArray();

                //* Save round-tripped and annotated text output for debugging
                File.WriteAllBytes(file + ".bin", roundTripBytes);
                using var ts = new MemoryStream();
                using var tw = new StreamWriter(ts);
                loader.Serdes(save, new AnnotatedFormatWriter(tw), key, null);
                ts.Position = 0;
                File.WriteAllBytes(file + ".txt", ts.ToArray());
                //*/

                ApiUtil.Assert(originalBytes.Length == roundTripBytes.Length);
                ApiUtil.Assert(originalBytes.SequenceEqual(roundTripBytes));

                var sw = new StringWriter();
                loader.Serdes(save, new JsonWriter(sw, true), key, null);
                File.WriteAllText(file + ".json", sw.ToString());
                break;
            }

            Console.ReadLine();
        }
    }
}
