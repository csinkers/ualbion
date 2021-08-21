using System;
using System.IO;
using System.Linq;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets.Save;
using UAlbion.TestCommon;
using Xunit;

namespace UAlbion.Formats.Tests
{
    public class SavedGameTests
    {
        static void RoundTrip(string file)
        {
            AssetMapping.GlobalIsThreadLocal = true;
            AssetMapping.Global.Clear()
                .RegisterAssetType(typeof(Base.Automap), AssetType.Automap)
                .RegisterAssetType(typeof(Base.Chest), AssetType.Chest)
                .RegisterAssetType(typeof(Base.EventSet), AssetType.EventSet)
                .RegisterAssetType(typeof(Base.Item), AssetType.Item)
                .RegisterAssetType(typeof(Base.LargeNpc), AssetType.LargeNpcGraphics)
                .RegisterAssetType(typeof(Base.LargePartyMember), AssetType.LargePartyGraphics)
                .RegisterAssetType(typeof(Base.Map), AssetType.Map)
                .RegisterAssetType(typeof(Base.Merchant), AssetType.Merchant)
                .RegisterAssetType(typeof(Base.Npc), AssetType.Npc)
                .RegisterAssetType(typeof(Base.PartyMember), AssetType.PartyMember)
                .RegisterAssetType(typeof(Base.Portrait), AssetType.Portrait)
                .RegisterAssetType(typeof(Base.SmallNpc), AssetType.SmallNpcGraphics)
                .RegisterAssetType(typeof(Base.SmallPartyMember), AssetType.SmallPartyGraphics)
                .RegisterAssetType(typeof(Base.Spell), AssetType.Spell)
                .RegisterAssetType(typeof(Base.Switch), AssetType.Switch)
                .RegisterAssetType(typeof(Base.Ticker), AssetType.Ticker);
            var mapping = AssetMapping.Global;

            // === Load ===
            using var stream = File.Open(file, FileMode.Open, FileAccess.Read);
            using var br = new BinaryReader(stream);
            using var annotationReadStream = new MemoryStream();
            using var annotationReader = new StreamWriter(annotationReadStream);
            using var ar = new AnnotationFacadeSerializer(new AlbionReader(br, stream.Length), annotationReader, FormatUtil.BytesFrom850String);
            var save = SavedGame.Serdes(null, mapping, ar);

            // === Save ===
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);
            using var annotationWriteStream = new MemoryStream();
            using var annotationWriter = new StreamWriter(annotationWriteStream);
            using var aw = new AnnotationFacadeSerializer(new AlbionWriter(bw), annotationWriter, FormatUtil.BytesFrom850String);
            SavedGame.Serdes(save, mapping, aw);

            // write out debugging files and compare round-tripped data
            br.BaseStream.Position = 0;
            var originalBytes = br.ReadBytes((int)stream.Length);
            var roundTripBytes = ms.ToArray();

            //* Save round-tripped and annotated text output for debugging

            static string ReadToEnd(Stream stream)
            {
                stream.Position = 0;
                using var reader = new StreamReader(stream, null, true, -1, true);
                return reader.ReadToEnd();
            }

            ms.Position = 0;
            using var reloadBr = new BinaryReader(ms);
            using var reloadAnnotationStream = new MemoryStream();
            using var reloadAnnotationReader = new StreamWriter(reloadAnnotationStream);
            using var reloadFacade = new AnnotationFacadeSerializer(new AlbionReader(reloadBr, stream.Length), reloadAnnotationReader, FormatUtil.BytesFrom850String);
            SavedGame.Serdes(null, mapping, reloadFacade);

            File.WriteAllBytes(file + ".bin", roundTripBytes);
            File.WriteAllText(file + ".pre.txt", ReadToEnd(annotationReadStream));
            File.WriteAllText(file + ".post.txt", ReadToEnd(annotationWriteStream));
            File.WriteAllText(file + ".reload.txt", ReadToEnd(reloadAnnotationStream));
            //*/

            //* Save JSON for debugging
            {
                File.WriteAllText(file + ".json", JsonUtil.Serialize(save));
            }
            //*/

            ApiUtil.Assert(originalBytes.Length == roundTripBytes.Length, $"Save game size changed after round trip (delta {roundTripBytes.Length - originalBytes.Length})");
            ApiUtil.Assert(originalBytes.SequenceEqual(roundTripBytes));

            var diffs = XDelta.Compare(originalBytes, roundTripBytes).ToArray();
            if (diffs.Length != 1)
            {
                Console.WriteLine($"===== {file}.pre.txt =====");
                Console.WriteLine(File.ReadAllText($"{file}.pre.txt"));
                Console.WriteLine($"===== {file}.post.txt =====");
                Console.WriteLine(File.ReadAllText($"{file}.post.txt"));
                Console.WriteLine($"===== {file}.reload.txt =====");
                Console.WriteLine(File.ReadAllText($"{file}.reload.txt"));
                Console.WriteLine($"===== {file}.json =====");
                Console.WriteLine(File.ReadAllText($"{file}.json"));
            }

            Assert.Collection(diffs,
                d =>
                {
                    Assert.True(d.IsCopy);
                    Assert.Equal(0, d.Offset);
                    Assert.Equal(originalBytes.Length, d.Length);
                });
        }

        [Fact]
        public void NewGameRoundTrip()
        {
            var disk = new MockFileSystem(true);
            var baseDir = ConfigUtil.FindBasePath(disk);
            RoundTrip(Path.Combine(baseDir, "mods", "UATest", "Saves", "NewGame.001"));
        }

        [Fact]
        public void LateGameRoundTrip()
        {
            var disk = new MockFileSystem(true);
            var baseDir = ConfigUtil.FindBasePath(disk);
            RoundTrip(Path.Combine(baseDir, "mods", "UATest", "Saves", "LateGame.001"));
        }
    }
}
