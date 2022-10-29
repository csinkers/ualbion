using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Labyrinth;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Ids;
using UAlbion.Formats.MapEvents;
using UAlbion.Formats.ScriptEvents;
using UAlbion.TestCommon;
using Xunit;

namespace UAlbion.Base.Tests;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local
public class AssetLoadTests : IDisposable
{
    static readonly EventExchange Exchange;
    static readonly AssetMapping Mapping = new();
    static int _nextTestNumber;

    readonly int _testNum;

    static AssetLoadTests()
    {
        var disk = new MockFileSystem(true);
        Exchange = AssetSystem.SetupSimple(disk, Mapping);
    }

    public AssetLoadTests()
    {
        Event.AddEventsFromAssembly(typeof(ActionEvent).Assembly);
        AssetMapping.GlobalIsThreadLocal = true;
        AssetMapping.Global.Clear();
        AssetMapping.Global.MergeFrom(Mapping);
        _testNum = Interlocked.Increment(ref _nextTestNumber);
        PerfTracker.StartupEvent($"Start asset test {_testNum}");
    }
    public void Dispose()
    {
        PerfTracker.StartupEvent($"Finish asset test {_testNum}");
    }

    static T Test<T>(Func<IAssetManager, T> func)
    {
        var assets = Exchange.Resolve<IAssetManager>();
        var result = func(assets);
        Assert.NotNull(result);

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
        Assert.Equal(ItemGfx.ItemSprites, item.Icon);
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
        Assert.Equal(632, tileset.Regions.Count);
        Assert.Equal(8, tileset.Regions[0].Width);
        Assert.Equal(8, tileset.Regions[0].Height);
        Assert.Equal(16, tileset.Regions[576].Width);
        Assert.Equal(16, tileset.Regions[576].Height);
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
        Assert.Equal(1, blocks[0].Tiles[0].Underlay);
        Assert.Equal(0, blocks[0].Tiles[0].Overlay);
    }

    [Fact]
    public void ChestTest()
    {
        var chest = Test(assets => assets.LoadInventory(AssetId.From(Chest.HClanCellar_ID_IKn_ILC_StC_LSh_3g)));
        Assert.Equal(25, chest.Gold.Amount);
        Assert.Equal(Gold.Instance, chest.Gold.Item);
        Assert.Equal(1, chest.Slots[0].Amount);
        Assert.Equal(Item.IskaiDagger, chest.Slots[0].ItemId);
    }

    [Fact]
    public void CombatBgTest()
    {
        var bg = Test(assets => assets.LoadTexture(CombatBackground.Toronto));
        Assert.Equal(1, bg.Regions.Count);
        Assert.Equal(360, bg.Width);
        Assert.Equal(192, bg.Height);
    }


    [Fact]
    public void CommonPaletteTest()
    {
        var pal = Test(assets => assets.LoadPalette(Palette.Common));
        Assert.Equal(AssetId.From(Palette.Common), AssetId.FromUInt32(pal.Id));
        Assert.False(pal.IsAnimated);
        Assert.Equal(1, pal.Period);
        Assert.Equal("Palette.Common", pal.Name);
        var at0 = pal.GetPaletteAtTime(0);
        for (int i = 0; i < 192; i++)
            Assert.Equal(0u, at0[i]);
    }

    [Fact]
    public void CoreSpriteTest()
    {
        var cursor = Test(assets => assets.LoadTexture(CoreGfx.Cursor));
        Assert.Equal(1, cursor.Regions.Count);
        Assert.Equal(14, cursor.Width);
        Assert.Equal(14, cursor.Height);
    }

    [Fact]
    public void DungeonObjectTest()
    {
        var krondir = Test(assets => assets.LoadTexture(DungeonObject.Krondir));
        Assert.Equal(3, krondir.Regions.Count);
    }

    [Fact]
    public void EventSetTest()
    {
        var set = Test(assets => assets.LoadEventSet(EventSet.Frill));
        Assert.Equal(10, set.Chains.Count);
        Assert.Collection(set.Events.Take(7),
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
                var e = (QueryEventUsedEvent)x.Event;
                Assert.Equal(QueryType.EventUsed, e.QueryType);
            }, // !1?2:3: query EventAlreadyUsed 0 (IsTrue 0)
            x =>
            {
                Assert.Equal(2, x.Id);
                Assert.Null(x.Next);
                var e = (TextEvent)x.Event;
                Assert.Equal(7, e.SubId);
                Assert.Equal(TextLocation.NoPortrait, e.Location);
                Assert.Equal(SheetId.None, e.Speaker);
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
        var font = Test(assets => assets.LoadTexture(FontGfx.Regular));
        Assert.Equal(111, font.Regions.Count);
    }

    [Fact]
    public void ItemSpriteTest()
    {
        var items = Test(assets => assets.LoadTexture(ItemGfx.ItemSprites));
        Assert.Equal(468, items.Regions.Count);
    }

    [Fact]
    public void LabyrinthTest()
    {
        var lab = Test(assets => assets.LoadLabyrinthData(Labyrinth.Jirinaar));
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
        Assert.Equal(DungeonObject.LightCover, o.Id);
        Assert.Equal(1, o.FrameCount);
        Assert.Equal(0u, o.Collision);
        Assert.Equal(32, o.Width);
        Assert.Equal(32, o.Height);
        Assert.Equal(224, o.MapWidth);
        Assert.Equal(224, o.MapHeight);
        Assert.Equal(LabyrinthObjectFlags.FloorObject, o.Properties);

        o = lab.Objects[1];
        Assert.Equal(DungeonObject.Pylon, o.Id);
        Assert.Equal(1, o.FrameCount);
        Assert.Equal(8u, o.Collision);
        Assert.Equal(16, o.Width);
        Assert.Equal(96, o.Height);
        Assert.Equal(58, o.MapWidth);
        Assert.Equal(350, o.MapHeight);
        Assert.Equal((LabyrinthObjectFlags)0, o.Properties);

        Assert.Equal(36, lab.FloorAndCeilings.Count);
        var f = lab.FloorAndCeilings[31];
        Assert.Equal(1, f.FrameCount);
        Assert.Equal((FloorAndCeiling.FcFlags)0, f.Properties);
        Assert.Equal(Floor.Moss, f.SpriteId);
        Assert.Equal(0, f.Unk1);
        Assert.Equal(0, f.Unk2);
        Assert.Equal(0, f.Unk3);
        Assert.Equal(0, f.Unk5);
        Assert.Equal(512, f.Unk8);

        Assert.Equal(31, lab.Walls.Count);
        var w = lab.Walls[23];
        Assert.Equal(1, w.FrameCount);
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

    [Fact]
    public void Map2DTest()
    {
        var map = (MapData2D)Test(assets => assets.LoadMap(Map.TorontoBegin));
        Assert.Equal(MapType.TwoD, map.MapType);
        Assert.Equal(12, map.FrameRate);
        Assert.Equal(0, map.Sound);
        Assert.Equal(217, map.Width);
        Assert.Equal(82, map.Height);
        Assert.Equal(SpriteId.None, map.CombatBackgroundId);
        Assert.Equal(Song.Toronto, map.SongId);
        Assert.Equal(Tileset.Toronto, map.TilesetId);
        Assert.Equal(Palette.Toronto2D, map.PaletteId);
        Assert.Equal(MapFlags.RestMode1 | MapFlags.RestMode2 | MapFlags.TorontoAutomap | MapFlags.V2NpcData | MapFlags.ExtraNpcs, map.Flags);
        Assert.Equal(MapSubMode.Unk0, map.SubMode);
        Assert.Equal(RestMode.NoResting, map.RestMode);
        Assert.Equal(map.Width * map.Height, map.Tiles.Length);
        Assert.Equal(871, map.Tiles[map.Width + 1].Underlay);
        Assert.Equal(349, map.Tiles[map.Width + 76].Underlay);
        Assert.Equal(map.Width * map.Height, map.Tiles.Length);
        Assert.Equal(0, map.Tiles[map.Width + 1].Overlay);
        Assert.Equal(2495, map.Tiles[4 * map.Width + 72].Overlay);

        Assert.Equal(657, map.Events.Count);
        var en = map.Events[0];
        Assert.Equal(0, en.Id);
        Assert.Equal(1, en.Next.Id);
        Assert.IsType<ChangeIconEvent>(en.Event);
        var e = (ChangeIconEvent)en.Event;
        Assert.Equal(IconChangeType.BlockSoft, e.ChangeType);
        Assert.Equal(EventScope.RelPerm, e.Scope);
        Assert.Equal(-1, e.X);
        Assert.Equal(-3, e.Y);
        Assert.Equal(302, e.Value);

        Assert.Equal(250, map.Chains.Count);

        Assert.Equal(96, map.Npcs.Count);
        var n = map.Npcs[2];
        Assert.Equal(AssetId.From(NpcSheet.Christine), n.Id);
        Assert.Equal(AssetId.From(NpcLargeGfx.Christine), n.SpriteOrGroup);
        Assert.Equal(NpcType.Npc, n.Type);
        Assert.Equal(TriggerTypes.Normal, n.Triggers);
        Assert.Equal(0xffff, n.Chain);
        Assert.Equal(NpcMovement.Waypoints, n.Movement);
        Assert.Equal(1152, n.Waypoints.Length);

        var initZones = map.GetZonesOfType(TriggerTypes.MapInit);
        var z = initZones.First();
        Assert.Equal(24, z.Chain);
        Assert.True(z.Global);
        Assert.Equal(TriggerTypes.MapInit, z.Trigger);
        Assert.Equal(0, z.Unk1);
        Assert.Equal(0, z.X);
        Assert.Equal(0, z.Y);
    }

    [Fact]
    public void Map3DTest()
    {
        var map = (MapData3D)Test(assets => assets.LoadMap(Map.OldFormerBuilding));
        Assert.Equal(MapType.ThreeD, map.MapType);
        Assert.Equal(101, map.Width);
        Assert.Equal(51, map.Height);
        Assert.Equal(SpriteId.None, map.CombatBackgroundId);
        Assert.Equal(Song.Ethereal, map.SongId);
        Assert.Equal(Palette.Argim, map.PaletteId);
        Assert.Equal(Labyrinth.Argim, map.LabDataId);
        Assert.Equal(64, map.AutomapGraphics.Length);
        Assert.Equal(Song.DungeonAmbient, map.AmbientSongId);
        Assert.Equal(MapFlags.SubMode1 | MapFlags.RestMode1 | MapFlags.V2NpcData | MapFlags.ExtraNpcs, map.Flags);
        Assert.Equal(MapSubMode.Unk1, map.SubMode);
        Assert.Equal(RestMode.RestEightHours, map.RestMode);
        Assert.Equal(map.Width * map.Height, map.Ceilings.Length);
        Assert.Equal(2, map.Ceilings[2 * map.Width + 2]);
        Assert.Equal(map.Width * map.Height, map.Floors.Length);
        Assert.Equal(1, map.Floors[2 * map.Width + 2]);
        Assert.Equal(map.Width * map.Height, map.Contents.Length);
        Assert.Equal(117, map.Contents[3 * map.Width + 13]);

        Assert.Empty(map.Automap);

        Assert.Equal(452, map.Events.Count);
        var en = map.Events[0];
        Assert.Equal(0, en.Id);
        Assert.Equal(1, en.Next.Id);
        Assert.IsType<SoundEvent>(en.Event);
        var e = (SoundEvent)en.Event;
        Assert.Equal(0, e.FrequencyOverride);
        Assert.Equal(SoundMode.LocalLoop, e.Mode);
        Assert.Equal(0, e.RestartProbability);
        Assert.Equal(0, e.Unk3);
        Assert.Equal(0, e.Unk3);
        Assert.Equal(SampleId.None, e.SoundId);

        Assert.Equal(64, map.Chains.Count);

        Assert.Equal(96, map.Npcs.Count);
        var n = map.Npcs[2];
        Assert.Equal(new AssetId(AssetType.MapTextIndex, 1), n.Id);
        Assert.Equal(new AssetId(AssetType.ObjectGroup, 68), n.SpriteOrGroup);
        Assert.Equal(NpcType.Prop, n.Type);
        Assert.Equal(MapNpcFlags.SimpleMsg | MapNpcFlags.Unk5, n.Flags);
        Assert.Equal(TriggerTypes.Normal | TriggerTypes.Examine, n.Triggers);
        Assert.Equal(23, n.Chain);
        Assert.Equal(NpcMovement.RandomWander, n.Movement);
        Assert.Collection(n.Waypoints,
            x =>
            {
                Assert.Equal(86, x.X);
                Assert.Equal(12, x.Y);
            });

        var z = map.GetZone(73, 4);
        Assert.Equal(15, z.Chain);
        Assert.False(z.Global);
        Assert.Equal(TriggerTypes.Normal | TriggerTypes.Examine | TriggerTypes.UseItem, z.Trigger);
        Assert.Equal(0, z.Unk1);
        Assert.Equal(73, z.X);
        Assert.Equal(4, z.Y);
    }

    [Fact]
    public void MapTextTest()
    {
        Assert.Equal("A fuse box.", Test(assets => assets.LoadString(new StringId(MapText.TorontoBegin, 1))));
        Assert.Equal("An armchair.", Test(assets => assets.LoadString(new StringId(MapText.TorontoBegin, 2))));
    }

    [Fact]
    public void MerchantTest()
    {
        var i = Test(assets => assets.LoadInventory(AssetId.From(Merchant.AltheaSpells)));
        Assert.Equal(Item.Fireball, i.Slots[0].ItemId);
        Assert.Equal(25, i.Slots[0].Amount);
        Assert.Equal(1, i.Slots[0].Charges);
        Assert.Equal(Item.BanishDemons, i.Slots[1].ItemId);
        Assert.Equal(34, i.Slots[1].Amount);
        Assert.Equal(1, i.Slots[1].Charges);
    }

    [Fact]
    public void MetaFontTest()
    {
        var font = Test(assets => assets.LoadFont(Font.Regular, Ink.White));
        Assert.Equal(111, font.Texture.Regions.Count);
    }

    [Fact]
    public void MonsterGroupTest()
    {
        var g = Test(assets => assets.LoadMonsterGroup(MonsterGroup.TwoSkrinn1OneKrondir1));
        Assert.Equal(18, g.Grid.Length);
        Assert.Equal(MonsterSheet.Krondir1, g.Grid[2]);
        Assert.Equal(MonsterSheet.Skrinn1, g.Grid[7]);
        Assert.Equal(MonsterSheet.Skrinn1, g.Grid[16]);
    }

    [Fact]
    public void MonsterTest()
    {
        var m = Test(assets => assets.LoadSheet(MonsterSheet.Krondir1));
        Assert.Equal(990, m.Magic.SpellPoints.Max);
        Assert.Empty(m.Magic.SpellStrengths);
        Assert.Equal(36, m.Attributes.Strength.Current);
        Assert.Equal(50, m.Attributes.Intelligence.Current);
        Assert.Equal(15, m.Attributes.Dexterity.Current);
        Assert.Equal(15, m.Attributes.Speed.Current);
        Assert.Equal(99, m.Attributes.Strength.Max);
        Assert.Equal(99, m.Attributes.Intelligence.Max);
        Assert.Equal(99, m.Attributes.Dexterity.Max);
        Assert.Equal(99, m.Attributes.Speed.Max);
        Assert.Equal(99, m.Attributes.MagicResistance.Max);
        Assert.Equal(99, m.Attributes.MagicTalent.Max);
        Assert.Equal(65, m.Skills.CloseCombat.Current);
        Assert.Equal(99, m.Skills.CloseCombat.Max);
        Assert.Equal(99, m.Skills.RangedCombat.Max);
        Assert.Equal(99, m.Skills.CriticalChance.Max);
        Assert.Equal(32, m.Combat.LifePoints.Current);
        Assert.Equal(990, m.Combat.LifePoints.Max);
        Assert.Equal(1, m.Combat.ActionPoints);
        Assert.Equal(MonsterSheet.Krondir1, m.Id);
        Assert.Equal("", m.EnglishName);
        Assert.Equal("Krondir 1", m.GermanName);
        Assert.Equal("", m.FrenchName);
        Assert.Equal(CharacterType.Monster, m.Type);
        Assert.Equal(PlayerRace.Monster, m.Race);
        Assert.Equal(PlayerClass.Monster, m.PlayerClass);
        Assert.Equal(PlayerLanguages.None, m.Languages);
        Assert.Equal(10, m.Level);
        Assert.Equal(MonsterGfx.Krondir, m.MonsterGfxId);
        Assert.Equal(1, m.UnkownC);
        Assert.Equal(1, m.UnkownD);
        Assert.Equal(1, m.UnknownE);
        Assert.Equal(75, m.Morale);
        Assert.Equal(180, m.ExperienceReward);
        Assert.Equal(21, m.UnknownDA);
        Assert.Equal(8, m.LevelsPerActionPoint);
        Assert.Equal(99, m.LifePointsPerLevel);
        Assert.Equal(99u, m.SpellPointsPerLevel);
        Assert.Equal(480, m.Weight);
    }

    [Fact]
    public void NewStringTest()
    {
        Assert.Equal("Take all", Test(assets => assets.LoadString(UAlbionString.TakeAll)));
    }

    [Fact]
    public void NpcTest()
    {
        var n = Test(assets => assets.LoadSheet(NpcSheet.Christine));
        Assert.Equal("", n.EnglishName);
        Assert.Equal("Christine", n.GermanName);
        Assert.Equal("", n.FrenchName);
        Assert.Equal(NpcSheet.Christine, n.Id);
        Assert.Equal(Portrait.Christine, n.PortraitId);
        Assert.Equal(EventSet.Christine, n.EventSetId);
        Assert.Equal(EventSet.TorontoWordSet, n.WordSetId);
        Assert.Equal(CharacterType.Npc, n.Type);
        Assert.Equal(PlayerRace.Terran, n.Race);
        Assert.Equal(PlayerClass.Pilot, n.PlayerClass);
        Assert.Equal(PlayerLanguages.Terran, n.Languages);

        Assert.Equal(0, n.Magic.SpellPoints.Max);
        Assert.Empty(n.Magic.SpellStrengths);
        Assert.Equal(0, n.Attributes.Strength.Current);
        Assert.Equal(0, n.Attributes.Intelligence.Current);
        Assert.Equal(0, n.Attributes.Dexterity.Current);
        Assert.Equal(0, n.Attributes.Speed.Current);
        Assert.Equal(0, n.Attributes.Strength.Max);
        Assert.Equal(0, n.Attributes.Intelligence.Max);
        Assert.Equal(0, n.Attributes.Dexterity.Max);
        Assert.Equal(0, n.Attributes.Speed.Max);
        Assert.Equal(0, n.Attributes.MagicResistance.Max);
        Assert.Equal(0, n.Attributes.MagicTalent.Max);
        Assert.Equal(0, n.Skills.CloseCombat.Current);
        Assert.Equal(0, n.Skills.CloseCombat.Max);
        Assert.Equal(0, n.Skills.RangedCombat.Max);
        Assert.Equal(0, n.Skills.CriticalChance.Max);
        Assert.Equal(0, n.Combat.LifePoints.Current);
        Assert.Equal(0, n.Combat.LifePoints.Max);
        Assert.Equal(0, n.Combat.ActionPoints);
        Assert.Equal(0, n.Level);
        Assert.Equal(SpriteId.None, n.MonsterGfxId);
        Assert.Equal(0, n.UnkownC);
        Assert.Equal(0, n.UnkownD);
        Assert.Equal(0, n.UnknownE);
        Assert.Equal(0, n.Morale);
        Assert.Equal(0, n.ExperienceReward);
        Assert.Equal(0, n.UnknownDA);
        Assert.Equal(0, n.LevelsPerActionPoint);
        Assert.Equal(0, n.LifePointsPerLevel);
        Assert.Equal(0u, n.SpellPointsPerLevel);
        Assert.Equal(0, n.Weight);
    }

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
        var t = Test(assets => assets.LoadSheet(PartySheet.Tom));
        Assert.Equal(0, t.Magic.SpellPoints.Max);
        Assert.Empty(t.Magic.SpellStrengths);

        Assert.Equal(0, t.Inventory.Gold.Amount);
        Assert.Equal(5, t.Inventory.Rations.Amount);
        Assert.Equal(Item.Overall, t.Inventory.Chest.ItemId);
        Assert.Equal(1, t.Inventory.Chest.Amount);
        Assert.Equal(Item.Shoes, t.Inventory.Feet.ItemId);
        Assert.Equal(1, t.Inventory.Feet.Amount);

        Assert.Equal(42, t.Attributes.Strength.Current);
        Assert.Equal(50, t.Attributes.Intelligence.Current);
        Assert.Equal(40, t.Attributes.Dexterity.Current);
        Assert.Equal(20, t.Attributes.Speed.Current);
        Assert.Equal(32, t.Attributes.Stamina.Current);
        Assert.Equal(10, t.Attributes.Luck.Current);
        Assert.Equal(70, t.Attributes.Strength.Max);
        Assert.Equal(90, t.Attributes.Intelligence.Max);
        Assert.Equal(90, t.Attributes.Dexterity.Max);
        Assert.Equal(50, t.Attributes.Speed.Max);
        Assert.Equal(65, t.Attributes.Stamina.Max);
        Assert.Equal(25, t.Attributes.Luck.Max);
        Assert.Equal(20, t.Attributes.MagicResistance.Max);
        Assert.Equal(35, t.Skills.CloseCombat.Current);
        Assert.Equal(25, t.Skills.RangedCombat.Current);
        Assert.Equal(15, t.Skills.LockPicking.Current);
        Assert.Equal(80, t.Skills.CloseCombat.Max);
        Assert.Equal(70, t.Skills.RangedCombat.Max);
        Assert.Equal(8, t.Skills.CriticalChance.Max);
        Assert.Equal(80, t.Skills.LockPicking.Max);
        Assert.Equal(150, t.Combat.ExperiencePoints);
        Assert.Equal(9, t.Combat.TrainingPoints);
        Assert.Equal(12, t.Combat.LifePoints.Current);
        Assert.Equal(12, t.Combat.LifePoints.Max);
        Assert.Equal(1, t.Combat.ActionPoints);
        Assert.Equal(25, t.Combat.UnknownD8);
        Assert.Equal(PartySheet.Tom, t.Id);
        Assert.Equal("", t.EnglishName);
        Assert.Equal("Tom", t.GermanName);
        Assert.Equal("", t.FrenchName);
        Assert.Equal(28, t.Age.Current);
        Assert.Equal(3, t.Level);
        Assert.Equal(PlayerLanguages.Terran, t.Languages);
        Assert.Equal(PartyLargeGfx.Tom, t.SpriteId);
        Assert.Equal(Portrait.Tom, t.PortraitId);
        Assert.Equal(EventSet.Tom, t.EventSetId);
        Assert.Equal(1, t.Unknown6);
        Assert.Equal(1, t.MonsterGfxId.Id);
        Assert.Equal(0, t.UnkownC);
        Assert.Equal(2, t.UnknownE);
        Assert.Equal(0, t.UnknownDE);
        Assert.Equal(10, t.LevelsPerActionPoint);
        Assert.Equal(4, t.LifePointsPerLevel);
        Assert.Equal(3, t.TrainingPointsPerLevel);
        Assert.Equal(2690, t.Weight);
    }

    [Fact]
    public void SampleTest()
    {
        var wav = Test(assets => assets.LoadSample(Sample.IllTemperedLlama));
        Assert.Equal(1, wav.BytesPerSample);
        Assert.Equal(1, wav.Channels);
        Assert.Equal(11025, wav.SampleRate);
        Assert.Equal(10861, wav.Samples.Length);
    }

    [Fact]
    public void ScriptTest()
    {
        var s = Test(assets => assets.LoadScript(Script.TomMeetsChristine_300));
        Assert.Equal(106, s.Count);
        Assert.IsType<CommentEvent>(s[0]);
        Assert.IsType<CommentEvent>(s[1]);
        Assert.IsType<CommentEvent>(s[2]);
        Assert.IsType<ShowMapEvent>(s[3]);
        Assert.IsType<CommentEvent>(s[4]);
        Assert.IsType<CommentEvent>(s[5]);
        Assert.IsType<TextEvent>(s[6]);
        Assert.IsType<CommentEvent>(s[7]);
        Assert.IsType<CommentEvent>(s[8]);
        Assert.IsType<PartyMoveEvent>(s[9]);
        Assert.IsType<GameUpdateEvent>(s[10]);
    }

    [Fact]
    public void SlabTest()
    {
        var slab = Test(assets => assets.LoadTexture(UiBackground.Slab));
        // Postprocessor creates the sub-images.
        // One is the full background, the other is just the status bar part.
        Assert.Equal(2, slab.Regions.Count);
    }

    [Fact]
    public void SongTest()
    {
        var song = Test(assets => assets.LoadSong(Song.Toronto));
        Assert.Equal(1928, song.Length);
    }

    [Fact]
    public void SpellTest()
    {
        var s = Test(assets => assets.LoadSpell(Spell.FrostAvalanche));
        Assert.Equal(Spell.FrostAvalanche, s.Id);
        Assert.Equal(30, s.Cost);
        Assert.Equal(9, s.LevelRequirement);
        Assert.Equal(SpellEnvironments.Combat, s.Environments);
        Assert.Equal(SpellTargets.AllMonsters, s.Targets);
        Assert.Equal(SpellClass.DjiKas, s.Class);
        Assert.Equal(7, s.OffsetInClass);
        Assert.Equal((TextId)SystemText.Spell0_7_FrostAvalanche, s.Name);
    }

    [Fact]
    public void SystemTextTest()
    {
        Assert.Equal("Main menu", Test(assets => assets.LoadString(SystemText.MainMenu_MainMenu)));
    }

    [Fact]
    public void TileGfxTest()
    {
        var tiles = Test(assets => assets.LoadTileGraphics(TilesetGfx.Toronto));
        Assert.Equal(2014, tiles.Texture.Regions.Count);
    }

    [Fact]
    public void TilesetTest()
    {
        var ts = Test(assets => assets.LoadTileData(Tileset.Toronto));
        Assert.Equal(Tileset.Toronto, ts.Id);
        Assert.False(ts.UseSmallGraphics);
        Assert.Equal(4097, ts.Tiles.Count);

        var t = ts.Tiles[0];
        Assert.Equal(TileLayer.Normal, t.Layer);
        Assert.Equal(TileType.Unk0, t.Type);
        Assert.Equal(Passability.Open, t.Collision);
        Assert.Equal(0xffff, t.ImageNumber);
        Assert.Equal(1, t.FrameCount);
        Assert.Equal(0, t.Unk7);

        t = ts.Tiles[452];
        Assert.Equal(TileLayer.Layer1, t.Layer);
        Assert.Equal(TileType.Unk0, t.Type);
        Assert.Equal(Passability.Solid, t.Collision);
        Assert.Equal(SitMode.Sleep2, t.SitMode);
        Assert.Equal(451, t.ImageNumber);
        Assert.Equal(1, t.FrameCount);
        Assert.Equal(7, t.Unk7);
    }

    [Fact]
    public void VideoTest()
    {
        var v = Test(assets => assets.LoadVideo(Video.MagicDemonstration));
        Assert.Equal(320, v.Width);
        Assert.Equal(200, v.Height);
        Assert.Equal(42, v.Frames);
        Assert.Equal(43, v.Chunks.Count);
        Assert.Equal(134096u, v.Size);
        Assert.Equal(100u, v.Speed);
    }

    [Fact]
    public void WaveLibTest()
    {
        var w = Test(assets => assets.LoadWaveLib(WaveLibrary.Unknown5));
        Assert.Equal(512, w.Samples.Length);
        var s = w[121];
        Assert.Equal(1, s.BytesPerSample);
        Assert.Equal(1, s.Channels);
        Assert.Equal(11025, s.SampleRate);
        Assert.Equal(26666, s.Samples.Length);
    }

    [Fact]
    public void WordTest()
    {
        Assert.Equal("key", Test(assets => assets.LoadString(Word.Key)));
    }

    [Fact] public void CombatGfxTest() { Test(assets => assets.LoadTexture(CombatGfx.Unknown27)); }
    [Fact] public void DungeonBgTest() { Test(assets => assets.LoadTexture(DungeonBackground.EarlyGameL)); }
    [Fact] public void FloorTest() { Test(assets => assets.LoadTexture(Floor.Water)); }
    [Fact] public void FullBodyPictureTest() { Test(assets => assets.LoadTexture(PartyInventoryGfx.Tom)); }
    [Fact] public void LargeNpcTest() { Test(assets => assets.LoadTexture(NpcLargeGfx.Christine)); }
    [Fact] public void LargePartyMemberTest() { Test(assets => assets.LoadTexture(PartyLargeGfx.Tom)); }
    [Fact] public void MonsterGfxTest() { Test(assets => assets.LoadTexture(MonsterGfx.Krondir)); }
    [Fact] public void OverlayTest() { Test(assets => assets.LoadTexture(WallOverlay.JiriWindow)); }
    [Fact] public void PictureTest() { Test(assets => assets.LoadTexture(Picture.OpenChestWithGold)); }
    [Fact] public void PortraitTest() { Test(assets => assets.LoadTexture(Portrait.Tom)); }
    [Fact] public void SmallNpcTest() { Test(assets => assets.LoadTexture(NpcSmallGfx.Krondir)); }
    [Fact] public void SmallPartyMemberTest() { Test(assets => assets.LoadTexture(PartySmallGfx.Tom)); }
    [Fact] public void TacticalGfxTest() { Test(assets => assets.LoadTexture(TacticalGfx.Tom)); }
    [Fact] public void WallTest() { Test(assets => assets.LoadTexture(Wall.TorontoPanelling)); }

    [Fact]
    public void AssetIdRoundTripping()
    {
        var failed = new List<(AssetId, string, string)>();
        foreach (var assetType in Enum.GetValues(typeof(AssetType)).OfType<AssetType>())
        {
            foreach (var id in AssetMapping.Global.EnumerateAssetsOfType(assetType))
            {
                var asString = id.ToString();
                try
                {
                    var roundTripped = AssetId.Parse(asString);
                    if (roundTripped != id)
                        failed.Add((id, asString, roundTripped.ToString()));
                }
                catch (Exception e) { failed.Add((id, asString, e.ToString())); }
            }
        }
        Assert.Empty(failed);

        Assert.Equal("Gold.0", new AssetId(AssetType.Gold).ToString());
        Assert.Equal("Rations.0", new AssetId(AssetType.Rations).ToString());
        Assert.Equal(new AssetId(AssetType.Gold), AssetId.Parse("Gold.0"));
        Assert.Equal(new AssetId(AssetType.Rations), AssetId.Parse("Rations.0"));
    }
}
