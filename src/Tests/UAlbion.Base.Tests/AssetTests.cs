using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;
using UAlbion.Game;
using UAlbion.Game.Settings;
using Xunit;

namespace UAlbion.Base.Tests
{
    public class AssetTests
    {
        static int s_testNum;

        static readonly CoreConfig CoreConfig;
        static readonly GeneralConfig GeneralConfig;
        static readonly GameConfig GameConfig;
        static readonly GeneralSettings Settings;

        static AssetTests()
        {
            var baseDir = ConfigUtil.FindBasePath();
            GeneralConfig = AssetSystem.LoadGeneralConfig(baseDir);
            CoreConfig = new CoreConfig();
            GameConfig = AssetSystem.LoadGameConfig(baseDir);
            Settings = new GeneralSettings
            {
                ActiveMods = { "Base" },
                Language = GameLanguage.English
            };
        }

        static T Test<T>(Func<IAssetManager, T> func)
        {
            int num = Interlocked.Increment(ref s_testNum);
            PerfTracker.StartupEvent($"Start test {num}");
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // Required for code page 850 support in .NET Core

            var generalConfigTask = Task.FromResult(GeneralConfig);
            var settingsTask = Task.FromResult(Settings);
            var coreConfigTask = Task.FromResult(CoreConfig);
            var gameConfigTask = Task.FromResult(GameConfig);
            var (exchange, _) = AssetSystem.SetupCore(generalConfigTask, settingsTask, coreConfigTask, gameConfigTask).Result;

            var assets = exchange.Resolve<IAssetManager>();
            var result = func(assets);
            Assert.NotNull(result);
            PerfTracker.StartupEvent($"Finish test {num}");
            return result;
        }

        [Fact]
        public void ItemTest()
        {
            var item = Test(assets => assets.LoadItem(Item.LughsShield));
            Test(assets => assets.LoadString(item.Name));
        }

        [Fact] public void AutomapGfxTest() { Test(assets => assets.LoadTexture(AutomapTiles.Set1)); }
        [Fact] public void AutomapTest() { Test(assets => assets.LoadAutomap(Automap.Jirinaar)); }
        [Fact] public void BlockListTest() { Test(assets => assets.LoadBlockList(BlockList.Toronto)); }
        [Fact] public void ChestTest() { Test(assets => assets.LoadInventory(AssetId.From(Chest.Unknown121))); }
        [Fact] public void CombatBgTest() { Test(assets => assets.LoadTexture(CombatBackground.Toronto)); }
        [Fact] public void CombatGfxTest() { Test(assets => assets.LoadTexture(CombatGraphics.Unknown27)); }
        [Fact] public void CommonPaletteTest() { Test(assets => assets.LoadPalette(Palette.CommonPalette)); }
        [Fact] public void CoreSpriteTest() { Test(assets => assets.LoadTexture(CoreSprite.Cursor)); }
        [Fact] public void DungeonBgTest() { Test(assets => assets.LoadTexture(DungeonBackground.EarlyGameL)); }
        [Fact] public void DungeonObjectTest() { Test(assets => assets.LoadTexture(DungeonObject.Barrel)); }
        [Fact] public void EventSetTest() { Test(assets => assets.LoadEventSet(EventSet.Frill)); }
        [Fact] public void EventTextTest() { Test(assets => assets.LoadString(EventText.Frill)); }
        [Fact] public void FloorTest() { Test(assets => assets.LoadTexture(Floor.Water)); }
        [Fact] public void FontTest() { Test(assets => assets.LoadTexture(Font.RegularFont)); }
        [Fact] public void FullBodyPictureTest() { Test(assets => assets.LoadTexture(FullBodyPicture.Tom)); }
        [Fact] public void ItemSpriteTest() { Test(assets => assets.LoadTexture(ItemGraphics.ItemSprites)); }
        [Fact] public void LabyrinthTest() { Test(assets => assets.LoadLabyrinthData(LabyrinthData.Unknown125)); }
        [Fact] public void LargeNpcTest() { Test(assets => assets.LoadTexture(LargeNpc.Christine)); }
        [Fact] public void LargePartyMemberTest() { Test(assets => assets.LoadTexture(LargePartyMember.Tom)); }
        [Fact] public void Map2DTest() { Test(assets => assets.LoadMap(Map.TorontoBegin)); }
        [Fact] public void Map3DTest() { Test(assets => assets.LoadMap(Map.OldFormerBuilding)); }
        [Fact] public void MapTextTest() { Test(assets => assets.LoadString(MapText.TorontoBegin)); }
        [Fact] public void MerchantTest() { Test(assets => assets.LoadInventory(AssetId.From(Merchant.Unknown109))); }
        [Fact] public void MetaFontTest() { Test(assets => assets.LoadFont(FontColor.White, false)); }
        [Fact] public void MonsterGfxTest() { Test(assets => assets.LoadTexture(MonsterGraphics.Krondir)); }
        [Fact] public void MonsterGroupTest() { Test(assets => assets.LoadMonsterGroup(MonsterGroup.TwoSkrinn1OneKrondir1)); }
        [Fact] public void MonsterTest() { Test(assets => assets.LoadSheet(Monster.Krondir1)); }
        [Fact] public void NewStringTest() { Test(assets => assets.LoadString(UAlbionString.TakeAll)); }
        [Fact] public void NpcTest() { Test(assets => assets.LoadSheet(Npc.Christine)); }
        [Fact] public void OverlayTest() { Test(assets => assets.LoadTexture(WallOverlay.JiriWindow)); }
        [Fact] public void PaletteTest() { Test(assets => assets.LoadPalette(Palette.Toronto2D)); }
        [Fact] public void PartyMemberTest() { Test(assets => assets.LoadSheet(PartyMember.Tom)); }
        [Fact] public void PictureTest() { Test(assets => assets.LoadTexture(Picture.OpenChestWithGold)); }
        [Fact] public void PortraitTest() { Test(assets => assets.LoadTexture(Portrait.Tom)); }
        [Fact] public void SampleTest() { Test(assets => assets.LoadSample(Sample.IllTemperedLlama)); }
        [Fact] public void ScriptTest() { Test(assets => assets.LoadScript(Script.TomMeetsChristine)); }
        [Fact] public void SlabTest() { Test(assets => assets.LoadTexture(UiBackground.Slab)); }
        [Fact] public void SmallNpcTest() { Test(assets => assets.LoadTexture(SmallNpc.Krondir)); }
        [Fact] public void SmallPartyMemberTest() { Test(assets => assets.LoadTexture(SmallPartyMember.Tom)); }
        [Fact] public void SongTest() { Test(assets => assets.LoadSong(Song.Toronto)); }
        [Fact] public void SpellTest() { Test(assets => assets.LoadSpell(Spell.FrostAvalanche)); }
        [Fact] public void SystemTextTest() { Test(assets => assets.LoadString(SystemText.MainMenu_MainMenu)); }
        [Fact] public void TacticalGfxTest() { Test(assets => assets.LoadTexture(TacticalGraphics.Unknown1)); }
        [Fact] public void TileGfxTest() { Test(assets => assets.LoadTexture(TilesetGraphics.Toronto)); }
        [Fact] public void TilesetTest() { Test(assets => assets.LoadTileData(TilesetData.Toronto)); }
        [Fact] public void VideoTest() { Test(assets => assets.LoadVideo(Video.MagicDemonstration)); }
        [Fact] public void WallTest() { Test(assets => assets.LoadTexture(Wall.TorontoPanelling)); }
        [Fact] public void WaveLibTest() { Test(assets => assets.LoadWaveLib(WaveLibrary.TorontoAmbient)); }
        [Fact] public void WordTest() { Test(assets => assets.LoadString(Word.Key)); }
    }
}
