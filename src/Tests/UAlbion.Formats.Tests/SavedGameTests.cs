using System.IO;
using System.Linq;
using System.Text;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets.Save;
using Xunit;

namespace UAlbion.Formats.Tests
{
    public class SavedGameTests
    {
        static void RoundTrip(string file)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var mapping = AssetMapping.Global;
            mapping.RegisterAssetType(typeof(Base.Automap), AssetType.Automap);
            mapping.RegisterAssetType(typeof(Base.Chest), AssetType.Chest);
            mapping.RegisterAssetType(typeof(Base.EventSet), AssetType.EventSet);
            mapping.RegisterAssetType(typeof(Base.Item), AssetType.Item);
            mapping.RegisterAssetType(typeof(Base.LargeNpc), AssetType.BigNpcGraphics);
            mapping.RegisterAssetType(typeof(Base.LargePartyMember), AssetType.BigPartyGraphics);
            mapping.RegisterAssetType(typeof(Base.Map), AssetType.Map);
            mapping.RegisterAssetType(typeof(Base.Merchant), AssetType.Merchant);
            mapping.RegisterAssetType(typeof(Base.Npc), AssetType.Npc);
            mapping.RegisterAssetType(typeof(Base.PartyMember), AssetType.PartyMember);
            mapping.RegisterAssetType(typeof(Base.Portrait), AssetType.Portrait);
            mapping.RegisterAssetType(typeof(Base.SmallNpc), AssetType.SmallNpcGraphics);
            mapping.RegisterAssetType(typeof(Base.SmallPartyMember), AssetType.SmallPartyGraphics);
            mapping.RegisterAssetType(typeof(Base.Spell), AssetType.Spell);
            mapping.RegisterAssetType(typeof(Base.Switch), AssetType.Switch);
            mapping.RegisterAssetType(typeof(Base.Ticker), AssetType.Ticker);

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

            /* Save round-tripped and annotated text output for debugging
            File.WriteAllBytes(file + ".bin", roundTripBytes);

            annotationReadStream.Position = 0;
            annotationWriteStream.Position = 0;
            using var trPre = new StreamReader(annotationReadStream, null, true, -1, true);
            using var trPost = new StreamReader(annotationWriteStream, null, true, -1, true);
            var annotatedPre = trPre.ReadToEnd();
            var annotatedPost = trPost.ReadToEnd();

            File.WriteAllText(file + ".pre.txt", annotatedPre);
            File.WriteAllText(file + ".post.txt", annotatedPost);
            //*/

            ApiUtil.Assert(originalBytes.Length == roundTripBytes.Length, $"Save game size changed after round trip (delta {roundTripBytes.Length - originalBytes.Length})");
            ApiUtil.Assert(originalBytes.SequenceEqual(roundTripBytes));

            /* Save JSON for debugging
            {
                var settings = new JsonSerializerSettings
                {
                    DefaultValueHandling = DefaultValueHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented
                };
                File.WriteAllText(file + ".json", JsonConvert.SerializeObject(save, settings));
            }
            //*/

            var diffs = XDelta.Compare(originalBytes, roundTripBytes).ToArray();
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
            var baseDir = ConfigUtil.FindBasePath();
            RoundTrip(Path.Combine(baseDir, "mods", "UATest", "Saves", "NewGame.001"));
        }

        [Fact]
        public void LateGameRoundTrip()
        {
            var baseDir = ConfigUtil.FindBasePath();
            RoundTrip(Path.Combine(baseDir, "mods", "UATest", "Saves", "LateGame.001"));
        }
    }
}
