using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Labyrinth;
using UAlbion.Formats.Config;
using UAlbion.Formats.MapEvents;
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
            var name = Test(assets => assets.LoadString(item.Name));
            Assert.Equal("Lugh's shield", name);
            Assert.Equal(18, item.Charges);
            Assert.Equal(PlayerClasses.Humans, item.Class);
            Assert.Equal(3, item.EnchantmentCount);
            Assert.Equal(ItemGraphics.ItemSprites, item.Icon);
            Assert.Equal(88, item.IconSubId);
            Assert.Equal(20, item.MaxCharges);
            Assert.Equal(20, item.MaxEnchantmentCount);
            Assert.Equal(15, item.Protection);
            Assert.Equal(ItemSlotId.LeftHand, item.SlotType);
            Assert.Equal(ItemType.Shield, item.TypeId);
            Assert.Equal(15000, item.Value);
            Assert.Equal(4000, item.Weight);
        }

        [Fact]
        public void AutomapGfxTest()
        {
            var tileset = Test(assets => assets.LoadTexture(AutomapTiles.Set1));
            Assert.Equal(632, tileset.SubImageCount);
            Assert.Equal(8, (int)tileset.GetSubImageDetails(0).Size.X);
            Assert.Equal(8, (int)tileset.GetSubImageDetails(0).Size.Y);
            Assert.Equal(16, (int)tileset.GetSubImageDetails(576).Size.X);
            Assert.Equal(16, (int)tileset.GetSubImageDetails(576).Size.Y);
        }

        [Fact]
        public void AutomapTest()
        {
            var automap = Test(assets => assets.LoadAutomap(Automap.Jirinaar));
            automap.Width = 90; // Automaps don't know their own width, it needs to be set based on the map.
            Assert.Equal(70, automap.Height);
            Assert.False(automap[0]);
            Assert.False(automap[89, 69]);
        }

        [Fact]
        public void BlockListTest()
        {
            var blocks = Test(assets => assets.LoadBlockList(BlockList.Toronto));
            Assert.Equal(4095, blocks.Count);
            Assert.Equal(1, blocks[0].Width);
            Assert.Equal(1, blocks[0].Height);
            Assert.Equal(2, blocks[0].GetUnderlay(0));
            Assert.Equal(1, blocks[0].GetOverlay(0));
        }

        [Fact]
        public void ChestTest()
        {
            var chest = Test(assets => assets.LoadInventory(AssetId.From(Chest.Unknown121)));
            Assert.Equal(25, chest.Gold.Amount);
            Assert.Equal(new Gold(), chest.Gold.Item);
            Assert.Equal(1, chest.Slots[0].Amount);
            Assert.Equal(Item.IskaiDagger, chest.Slots[0].ItemId);
        }

        [Fact]
        public void CombatBgTest()
        {
            var bg = Test(assets => assets.LoadTexture(CombatBackground.Toronto));
            Assert.Equal(1, bg.SubImageCount);
            Assert.Equal((uint)360, bg.Width);
            Assert.Equal((uint)192, bg.Height);
        }

        [Fact] public void CombatGfxTest() { Test(assets => assets.LoadTexture(CombatGraphics.Unknown27)); }
        [Fact] public void DungeonBgTest() { Test(assets => assets.LoadTexture(DungeonBackground.EarlyGameL)); }

        [Fact] public void FloorTest() { Test(assets => assets.LoadTexture(Floor.Water)); }
        [Fact] public void FullBodyPictureTest() { Test(assets => assets.LoadTexture(FullBodyPicture.Tom)); } 


        [Fact]
        public void CommonPaletteTest()
        {
            var pal = Test(assets => assets.LoadPalette(Palette.CommonPalette));
            Assert.Equal(AssetId.From(Palette.CommonPalette), AssetId.FromUInt32(pal.Id));
            Assert.False(pal.IsAnimated);
            Assert.Equal(1, pal.Period);
            Assert.Equal("Palette.CommonPalette", pal.Name);
            var at0 = pal.GetPaletteAtTime(0);
            for (int i = 0; i < 192; i++)
                Assert.Equal(0u, at0[i]);
        }

        [Fact]
        public void CoreSpriteTest()
        {
            var cursor = Test(assets => assets.LoadTexture(CoreSprite.Cursor));
            Assert.Equal(1, cursor.SubImageCount);
            Assert.Equal(14u, cursor.Width);
            Assert.Equal(14u, cursor.Height);
        }

        [Fact]
        public void DungeonObjectTest()
        {
            var krondir = Test(assets => assets.LoadTexture(DungeonObject.Krondir));
            Assert.Equal(3, krondir.SubImageCount);
        } 

        [Fact]
        public void EventSetTest()
        {
            var set = Test(assets => assets.LoadEventSet(EventSet.Frill));
            Assert.Equal(10, set.Chains.Count);
            var c = set.Chains[0];
            Assert.Collection(c.Events,
                x =>
                {
                    Assert.Equal(0, x.Id);
                    Assert.Equal(1, x.Next.Id);
                    Assert.IsType<ActionEvent>(x.Event);
                    var e = (ActionEvent)x.Event;
                    Assert.Equal(ActionType.StartDialogue, e.ActionType);
                }, // 0=>1: action StartDialogue Block:0 From:Unknown (1)
                x =>
                {
                    Assert.Equal(1, x.Id);
                    Assert.Equal(2, x.Next.Id);
                    var bn = (IBranchNode)x;
                    Assert.Equal(3, bn.NextIfFalse.Id);
                    var e = (QueryEvent)x.Event;
                    Assert.Equal(QueryType.EventAlreadyUsed, e.QueryType);
                    Assert.Equal(QueryOperation.IsTrue, e.Operation);
                }, // !1?2:3: query EventAlreadyUsed 0 (IsTrue 0)
                x =>
                {
                    Assert.Equal(2, x.Id);
                    Assert.Null(x.Next);
                    var e = (TextEvent)x.Event;
                    Assert.Equal(EventText.Frill, e.TextSourceId);
                    Assert.Equal(7, e.TextId);
                    Assert.Equal(TextLocation.NoPortrait, e.Location);
                    Assert.Equal(CharacterId.None, e.CharacterId);
                }, // 2=>!: text EventText.Frill:7 NoPortrait None (0 0 0 0) 
                x => { Assert.Equal(3, x.Id); Assert.Equal(4, x.Next.Id); }, // 3=>4: text EventText.Frill:1 NoPortrait None (0 0 0 0) 
                x => { Assert.Equal(4, x.Id); Assert.Equal(5, x.Next.Id); }, // 4=>5: text EventText.Frill:2 PortraitLeft Npc.Branagh (0 0 0 0) 
                x => { Assert.Equal(5, x.Id); Assert.Equal(6, x.Next.Id); }, // 5=>6: text EventText.Frill:3 Conversation None (0 0 0 0) 
                x => { Assert.Equal(6, x.Id); Assert.Null(x.Next); } // 6=>!: text EventText.Frill:3 ConversationOptions None (0 0 0 0) 
            );
        }

        [Fact]
        public void EventTextTest()
        {
            Assert.Equal("\"Dsarii-ma, foreign visitors. My name is Frill, and I serve the council as a " +
                         "scholar in history. What horrible events you had to go through! I am sure it is not " +
                         "easy for strangers to be subjected to our laws!\"",
                Test(assets => assets.LoadString(new StringId(EventText.Frill, 1))));

            Assert.Equal("\"I am trying to help them in any way I can, Sebai-Giz Frill!\"",
                Test(assets => assets.LoadString(new StringId(EventText.Frill, 2))));
        }

        [Fact]
        public void FontTest()
        {
            var font = Test(assets => assets.LoadTexture(Font.RegularFont));
            Assert.Equal(111, font.SubImageCount);
        }

        [Fact]
        public void ItemSpriteTest()
        {
            var items = Test(assets => assets.LoadTexture(ItemGraphics.ItemSprites));
            Assert.Equal(468, items.SubImageCount);
        }

        [Fact]
        public void LabyrinthTest()
        {
            var lab = Test(assets => assets.LoadLabyrinthData(LabyrinthData.Jirinaar));
            Assert.Equal(182, lab.BackgroundColour);
            Assert.Equal(DungeonBackground.EarlyGameS, lab.BackgroundId);
            Assert.Equal(12, lab.BackgroundTileAmount);
            Assert.Equal(188, lab.BackgroundYPosition);
            Assert.Equal(25, lab.CameraHeight);
            Assert.Equal(9, lab.WallWidth);
            Assert.Equal(512, lab.EffectiveWallWidth);
            Assert.Equal(512, lab.WallHeight);

            Assert.Equal(224, lab.FogRed);
            Assert.Equal(224, lab.FogGreen);
            Assert.Equal(224, lab.FogBlue);
            Assert.Equal(28, lab.FogDistance);
            Assert.Equal(20, lab.FogMode);
            Assert.Equal(30, lab.MaxVisibleTiles);

            Assert.Equal(0, lab.Lighting);
            Assert.Equal(15, lab.MaxLight);

            Assert.Equal(68, lab.ObjectGroups.Count);
            var og = lab.ObjectGroups[0];
            Assert.Equal(0, og.AutoGraphicsId);

            Assert.Equal(0, og.SubObjects[0].ObjectInfoNumber);
            Assert.Equal(255, og.SubObjects[0].X);
            Assert.Equal(355, og.SubObjects[0].Y);
            Assert.Equal(255, og.SubObjects[0].Z);

            Assert.Equal(1, og.SubObjects[1].ObjectInfoNumber);
            Assert.Equal(255, og.SubObjects[1].X);
            Assert.Equal(0, og.SubObjects[1].Y);
            Assert.Equal(255, og.SubObjects[1].Z);

            Assert.Equal(61, lab.Objects.Count);
            var o = lab.Objects[0];
            Assert.Equal(DungeonObject.LightCover, o.SpriteId);
            Assert.Equal(1, o.AnimationFrames);
            Assert.Equal(0u, o.Collision);
            Assert.Equal(32, o.Width);
            Assert.Equal(32, o.Height);
            Assert.Equal(224, o.MapWidth);
            Assert.Equal(224, o.MapHeight);
            Assert.Equal(LabyrinthObjectFlags.FloorObject, o.Properties);

            o = lab.Objects[1];
            Assert.Equal(DungeonObject.Pylon, o.SpriteId);
            Assert.Equal(1, o.AnimationFrames);
            Assert.Equal(8u, o.Collision);
            Assert.Equal(16, o.Width);
            Assert.Equal(96, o.Height);
            Assert.Equal(58, o.MapWidth);
            Assert.Equal(350, o.MapHeight);
            Assert.Equal((LabyrinthObjectFlags)0, o.Properties);

            Assert.Equal(36, lab.FloorAndCeilings.Count);
            var f = lab.FloorAndCeilings[31];
            Assert.Equal(1, f.AnimationCount);
            Assert.Equal((FloorAndCeiling.FcFlags)0, f.Properties);
            Assert.Equal(Floor.Moss, f.SpriteId);
            Assert.Equal(0, f.Unk1);
            Assert.Equal(0, f.Unk2);
            Assert.Equal(0, f.Unk3);
            Assert.Equal(0, f.Unk5);
            Assert.Equal(512, f.Unk8);

            Assert.Equal(31, lab.Walls.Count);
            var w = lab.Walls[23];
            Assert.Equal(1, w.AnimationFrames);
            Assert.Equal(11, w.AutoGfxType);
            Assert.Equal(8u, w.Collision);
            Assert.Equal(112, w.Width);
            Assert.Equal(112, w.Height);
            Assert.Collection(w.Overlays,
                x =>
                {
                    Assert.Equal(WallOverlay.Unknown114, x.SpriteId);
                    Assert.Equal(1, x.AnimationFrames);
                    Assert.Equal(55, x.Width);
                    Assert.Equal(38, x.Height);
                    Assert.Equal(1, x.WriteZero);
                    Assert.Equal(27, x.XOffset);
                    Assert.Equal(53, x.YOffset);
                },
                x =>
                {
                    Assert.Equal(WallOverlay.JiriExposedRocks, x.SpriteId);
                    Assert.Equal(1, x.AnimationFrames);
                    Assert.Equal(25, x.Width);
                    Assert.Equal(26, x.Height);
                    Assert.Equal(1, x.WriteZero);
                    Assert.Equal(6, x.XOffset);
                    Assert.Equal(19, x.YOffset);
                },
                x =>
                {
                    Assert.Equal(WallOverlay.JiriScuff, x.SpriteId);
                    Assert.Equal(1, x.AnimationFrames);
                    Assert.Equal(9, x.Width);
                    Assert.Equal(6, x.Height);
                    Assert.Equal(1, x.WriteZero);
                    Assert.Equal(0, x.XOffset);
                    Assert.Equal(0, x.YOffset);
                },
                x =>
                {
                    Assert.Equal(WallOverlay.JiriDirt, x.SpriteId);
                    Assert.Equal(1, x.AnimationFrames);
                    Assert.Equal(40, x.Width);
                    Assert.Equal(20, x.Height);
                    Assert.Equal(1, x.WriteZero);
                    Assert.Equal(64, x.XOffset);
                    Assert.Equal(0, x.YOffset);
                });

            Assert.Equal(Formats.Assets.Labyrinth.Wall.WallFlags.WriteOverlay, w.Properties);
            Assert.Equal(Wall.JiriMasonry, w.SpriteId);
            Assert.Equal(0, w.TransparentColour);
            Assert.Equal(0, w.Unk9);
        }

        [Fact] public void LargeNpcTest() { Test(assets => assets.LoadTexture(LargeNpc.Christine)); }

        [Fact]
        public void LargePartyMemberTest()
        {
            Test(assets => assets.LoadTexture(LargePartyMember.Tom));
        }

        [Fact]
        public void Map2DTest()
        {
            Test(assets => assets.LoadMap(Map.TorontoBegin));
        }

        [Fact]
        public void Map3DTest()
        {
            Test(assets => assets.LoadMap(Map.OldFormerBuilding));
        }

        [Fact]
        public void MapTextTest()
        {
            Test(assets => assets.LoadString(MapText.TorontoBegin));
        }

        [Fact]
        public void MerchantTest()
        {
            Test(assets => assets.LoadInventory(AssetId.From(Merchant.Unknown109)));
        }

        [Fact]
        public void MetaFontTest()
        {
            Test(assets => assets.LoadFont(FontColor.White, false));
        }

        [Fact] public void MonsterGfxTest() { Test(assets => assets.LoadTexture(MonsterGraphics.Krondir)); }

        [Fact]
        public void MonsterGroupTest()
        {
            Test(assets => assets.LoadMonsterGroup(MonsterGroup.TwoSkrinn1OneKrondir1));
        }

        [Fact]
        public void MonsterTest()
        {
            Test(assets => assets.LoadSheet(Monster.Krondir1));
        }

        [Fact]
        public void NewStringTest()
        {
            Test(assets => assets.LoadString(UAlbionString.TakeAll));
        }

        [Fact]
        public void NpcTest()
        {
            Test(assets => assets.LoadSheet(Npc.Christine));
        }

        [Fact] public void OverlayTest() { Test(assets => assets.LoadTexture(WallOverlay.JiriWindow)); }

        [Fact]
        public void PaletteTest()
        {
            var pal = Test(assets => assets.LoadPalette(Palette.Toronto2D));
            Assert.Equal(AssetId.From(Palette.Toronto2D), AssetId.FromUInt32(pal.Id));
            Assert.True(pal.IsAnimated);
            Assert.Equal(4, pal.Period);
            Assert.Equal("Palette.Toronto2D", pal.Name);
        }

        [Fact]
        public void PartyMemberTest()
        {
            Test(assets => assets.LoadSheet(PartyMember.Tom));
        }

        [Fact] public void PictureTest() { Test(assets => assets.LoadTexture(Picture.OpenChestWithGold)); }
        [Fact] public void PortraitTest() { Test(assets => assets.LoadTexture(Portrait.Tom)); }

        [Fact]
        public void SampleTest()
        {
            Test(assets => assets.LoadSample(Sample.IllTemperedLlama));
        }

        [Fact]
        public void ScriptTest()
        {
            Test(assets => assets.LoadScript(Script.TomMeetsChristine));
        }

        [Fact]
        public void SlabTest()
        {
            Test(assets => assets.LoadTexture(UiBackground.Slab));
        }

        [Fact] public void SmallNpcTest() { Test(assets => assets.LoadTexture(SmallNpc.Krondir)); } 

        [Fact]
        public void SmallPartyMemberTest()
        {
            Test(assets => assets.LoadTexture(SmallPartyMember.Tom));
        }

        [Fact]
        public void SongTest()
        {
            Test(assets => assets.LoadSong(Song.Toronto));
        }

        [Fact]
        public void SpellTest()
        {
            Test(assets => assets.LoadSpell(Spell.FrostAvalanche));
        }

        [Fact]
        public void SystemTextTest()
        {
            Test(assets => assets.LoadString(SystemText.MainMenu_MainMenu));
        }

        [Fact]
        public void TacticalGfxTest()
        {
            Test(assets => assets.LoadTexture(TacticalGraphics.Unknown1));
        }

        [Fact]
        public void TileGfxTest()
        {
            Test(assets => assets.LoadTexture(TilesetGraphics.Toronto));
        }

        [Fact]
        public void TilesetTest()
        {
            Test(assets => assets.LoadTileData(TilesetData.Toronto));
        }

        [Fact]
        public void VideoTest()
        {
            Test(assets => assets.LoadVideo(Video.MagicDemonstration));
        }

        [Fact]
        public void WallTest()
        {
            Test(assets => assets.LoadTexture(Wall.TorontoPanelling));
        }

        [Fact]
        public void WaveLibTest()
        {
            Test(assets => assets.LoadWaveLib(WaveLibrary.TorontoAmbient));
        }

        [Fact]
        public void WordTest()
        {
            Test(assets => assets.LoadString(Word.Key));
        }
    }
}
