using System;
using System.IO;
using System.Linq;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.MapEvents;
using UAlbion.Formats.Parsers;
using UAlbion.Game.Assets;
using UAlbion.TestCommon;
using Xunit;

namespace UAlbion.Base.Tests;

public class AssetConversionTests
{
    const string BaseAssetMod = "Albion";
    const string UnpackedAssetMod = "Unpacked";
    const string RepackedAssetMod = "Repacked";

    static readonly IJsonUtil JsonUtil = new FormatJsonUtil();
    readonly string _baseDir;
    readonly IFileSystem _disk;
    readonly IModApplier _baseApplier;

    public AssetConversionTests()
    {
        Event.AddEventsFromAssembly(typeof(ActionEvent).Assembly);
        AssetMapping.GlobalIsThreadLocal = true;
        // Hide any assets that have already been unpacked on the actual disk to prevent them interfering in the tests
        _disk = new MockFileSystem(x =>
            !x.Contains(Path.Combine("mods", UnpackedAssetMod, "Assets")) &&
            !x.Contains(Path.Combine("mods", RepackedAssetMod, "Albion")));

        _baseDir = ConfigUtil.FindBasePath(_disk);
        _baseApplier = BuildApplier(BaseAssetMod, AssetMapping.Global);
    }

    IModApplier BuildApplier(string mod, AssetMapping mapping)
    {
        var exchange = AssetSystem.SetupSimple(_disk, mapping, mod);
        return exchange.Resolve<IModApplier>();
    }

    void Test<T>(AssetId id, AssetId[] prerequisites, Asset.SerdesFunc<T> serdes, Func<T, T> canonicalize = null) where T : class
    {
        prerequisites ??= Array.Empty<AssetId>();
        var allIds = prerequisites.Append(id);

        var resultsDir = Path.Combine(_baseDir, "re", "ConversionTests");

        var baseAsset = (T)_baseApplier.LoadAsset(id);
        if (canonicalize != null)
            baseAsset = canonicalize(baseAsset);

        var mapping = AssetMapping.Global;
        var stubDisk = new StubFileSystem();
        var context = new SerdesContext("Test", JsonUtil, mapping, stubDisk);

        var (baseBytes, baseNotes) = Asset.Save(baseAsset, serdes, context);
        var baseJson = Asset.SaveJson(baseAsset, JsonUtil);

        var idStrings = allIds.Select(x => $"{x.Type}.{x.Id}").ToArray();
        var assetTypes = allIds.Select(x => x.Type).Distinct().ToHashSet();

        using (var unpacker = new AssetConverter(
                   mapping,
                   _disk,
                   JsonUtil,
                   new[] { BaseAssetMod },
                   UnpackedAssetMod))
        {
            unpacker.Convert(idStrings, assetTypes, null);
        }

        var unpackedAsset = (T)BuildApplier(UnpackedAssetMod, AssetMapping.Global).LoadAsset(id);
        Assert.NotNull(unpackedAsset);
        var (unpackedBytes, unpackedNotes) = Asset.Save(unpackedAsset, serdes, context);
        var unpackedJson = Asset.SaveJson(unpackedAsset, JsonUtil);

        Asset.Compare(resultsDir,
            id.ToString(),
            baseBytes,
            unpackedBytes,
            new[]
            {
                (".saveBase.txt", baseNotes),
                (".saveUnpacked.txt", unpackedNotes),
                (".Base.json", baseJson),
                (".Unpacked.json", unpackedJson)
            });

        using (var repacker = new AssetConverter(
                   mapping,
                   _disk,
                   JsonUtil,
                   new[] { UnpackedAssetMod },
                   RepackedAssetMod))
        {
            repacker.Convert(idStrings, assetTypes, null);
        }

        var repackedAsset = (T)BuildApplier(RepackedAssetMod, AssetMapping.Global).LoadAsset(id);
        var (repackedBytes, repackedNotes) = Asset.Save(repackedAsset, serdes, context);
        var repackedJson = Asset.SaveJson(repackedAsset, JsonUtil);

        Asset.Compare(resultsDir,
            id.Type.ToString(),
            baseBytes,
            repackedBytes,
            new[]
            {
                (".saveBase.txt", baseNotes),
                (".saveRepacked.txt", repackedNotes),
                (".Base.json", baseJson),
                (".Repacked.json", repackedJson)
            });
    }

    [Fact]
    public void ItemTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(Item.Knife) };
        var spell = new SpellData(Spell.ThornSnare, SpellClass.DjiKas, 0)
        {
            Cost = 1,
            Environments = SpellEnvironments.Combat,
            LevelRequirement = 2,
            Targets = SpellTargets.OneMonster,
        };

        var spellManager = new MockSpellManager().Add(spell);
        ItemDataLoader itemDataLoader = new();
        new EventExchange()
            .Attach(spellManager)
            .Attach(itemDataLoader);

        Test<ItemData>(info.AssetId, null, (x, s, c) => itemDataLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void ItemNameTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(Special.ItemNames) };
        Test<MultiLanguageStringDictionary>(info.AssetId,
            AssetMapping.Global.EnumerateAssetsOfType(AssetType.ItemName).ToArray(),
            (x, s, c) => Loaders.ItemNameLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void AutomapTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(Automap.Jirinaar) };
        Test<Formats.Assets.Automap>(info.AssetId, null, (x, s, c) => Loaders.AutomapLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void BlockListTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(BlockList.Toronto) };
        Test<Formats.Assets.BlockList>(info.AssetId, null, (x, s, c) => Loaders.BlockListLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void ChestTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(Chest.Unknown121) };
        Test<Inventory>(
            info.AssetId,
            AssetId.EnumerateAll(AssetType.Item).ToArray(),
            (x, s, c) => Loaders.ChestLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void CommonPaletteTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(Palette.Common) };
        info.Set(AssetProperty.IsCommon, true);
        Test<AlbionPalette>(info.AssetId, null, (x, s, c) => Loaders.PaletteLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void EventSetTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(EventSet.Frill) };
        Test<Formats.Assets.EventSet>(
            info.AssetId,
            null,
            (x, s, c) => Loaders.EventSetLoader.Serdes(x, info, s, c),
            LayoutTestUtil.CanonicalizeEventSet);
    }

    [Fact]
    public void EventTextTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(EventText.Frill) };
        Test<ListStringCollection>(info.AssetId, null, (x, s, c) => Loaders.AlbionStringTableLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void LabyrinthTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(Labyrinth.Jirinaar) };
        Test<Formats.Assets.Labyrinth.LabyrinthData>(info.AssetId, null, (x, s, c) => Loaders.LabyrinthDataLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void Map2DTest_200() // An outdoor level
    {
        var info = new AssetInfo { AssetId = AssetId.From(Map.Nakiridaani) };

        var small = AssetMapping.Global.EnumerateAssetsOfType(AssetType.NpcSmallGfx);
        var large = AssetMapping.Global.EnumerateAssetsOfType(AssetType.NpcLargeGfx);
        var palettes = AssetMapping.Global.EnumerateAssetsOfType(AssetType.Palette);
        var prereqs =
            new[] { AssetId.From(Special.DummyObject) }
                .Concat(small)
                .Concat(large)
                .Concat(palettes)
                .ToArray();

        Test<MapData2D>(
            info.AssetId,
            prereqs,
            (x, s, c) => MapData2D.Serdes(info, x, c.Mapping, s),
            LayoutTestUtil.CanonicalizeMap);
    }

    [Fact]
    public void Map2DTest_300() // Starting level
    {
        var info = new AssetInfo { AssetId = AssetId.From(Map.TorontoBegin) };

        var small = AssetMapping.Global.EnumerateAssetsOfType(AssetType.NpcSmallGfx);
        var large = AssetMapping.Global.EnumerateAssetsOfType(AssetType.NpcLargeGfx);
        var palettes = AssetMapping.Global.EnumerateAssetsOfType(AssetType.Palette);
        var prereqs =
            new[] { AssetId.From(Special.DummyObject) }
                .Concat(small)
                .Concat(large)
                .Concat(palettes)
                .ToArray();

        Test<MapData2D>(
            info.AssetId,
            prereqs,
            (x, s, c) => MapData2D.Serdes(info, x, c.Mapping, s),
            LayoutTestUtil.CanonicalizeMap);
    }

    [Fact]
    public void Map3DTest_110() // A town level with automap markers
    {
        var prereqs = new[] { AssetId.From(Labyrinth.Jirinaar) };
        var info = new AssetInfo { AssetId = AssetId.From(Map.Jirinaar) };
        Test<MapData3D>(
            info.AssetId,
            prereqs,
            (x, s, c) => MapData3D.Serdes(info, x, c.Mapping, s),
            LayoutTestUtil.CanonicalizeMap);
    }

    [Fact]
    public void Map3DTest_122() // A dungeon level
    {
        var prereqs = new[] { AssetId.From(Labyrinth.Argim) };
        var info = new AssetInfo { AssetId = AssetId.From(Map.OldFormerBuilding) };
        Test<MapData3D>(
            info.AssetId,
            prereqs,
            (x, s, c) => MapData3D.Serdes(info, x, c.Mapping, s),
            LayoutTestUtil.CanonicalizeMap);
    }

    [Fact]
    public void MapTextTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(MapText.TorontoBegin) };
        Test<ListStringCollection>(info.AssetId, null, (x, s, c) => Loaders.AlbionStringTableLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void MerchantTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(Merchant.Unknown109) };
        Test<Inventory>(
            info.AssetId,
            AssetId.EnumerateAll(AssetType.Item).ToArray(),
            (x, s, c) => Loaders.MerchantLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void BrokenMerchantTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(Merchant.Unknown1) };
        Test<Inventory>(
            info.AssetId,
            AssetId.EnumerateAll(AssetType.Item).ToArray(),
            (x, s, c) => Loaders.MerchantLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void MonsterGroupTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(MonsterGroup.TwoSkrinn1OneKrondir1) };
        Test<Formats.Assets.MonsterGroup>(info.AssetId, null, (x, s, c) => Loaders.MonsterGroupLoader.Serdes(x, info, s, c));
    }

    static CharacterSheetLoader BuildCharacterLoader()
    {
        var spell = new SpellData(Spell.ThornSnare, SpellClass.DjiKas, 0)
        {
            Cost = 1,
            Environments = SpellEnvironments.Combat,
            LevelRequirement = 2,
            Targets = SpellTargets.OneMonster,
        };

        var spellManager = new MockSpellManager().Add(spell);
        CharacterSheetLoader characterSheetLoader = new();
        new EventExchange()
            .Attach(spellManager)
            .Attach(characterSheetLoader);

        return characterSheetLoader;
    }

    [Fact]
    public void MonsterTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(MonsterSheet.Krondir1) };
        var loader = BuildCharacterLoader();
        Test<CharacterSheet>(
            info.AssetId,
            AssetId.EnumerateAll(AssetType.Item).ToArray(),
            (x, s, c) => loader.Serdes(x, info, s, c));
    }

    [Fact]
    public void NpcTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(NpcSheet.Christine) };
        var loader = BuildCharacterLoader();
        Test<CharacterSheet>(info.AssetId, null, (x, s, c) => loader.Serdes(x, info, s, c));
    }

    [Fact]
    public void PaletteTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(Palette.Toronto2D) };
        Test<AlbionPalette>(info.AssetId, new[] { AssetId.From(Palette.Common) }, (x, s, c) => Loaders.PaletteLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void PartySheetTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(PartySheet.Tom) };
        var loader = BuildCharacterLoader();
        Test<CharacterSheet>(
            info.AssetId,
            AssetId.EnumerateAll(AssetType.Item).ToArray(),
            (x, s, c) => loader.Serdes(x, info, s, c));
    }

    [Fact]
    public void SampleTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(Sample.IllTemperedLlama) };
        Test<AlbionSample>(info.AssetId, null, (x, s, c) => Loaders.SampleLoader.Serdes(x, info, s, c));
    }

    /* They're text anyway so not too bothered - at the moment they don't round trip due to using friendly asset id names
    // Would need to add a ToStringNumeric or something to the relevant events, starts getting ugly.
    [Fact]
    public void ScriptTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(Script.TomMeetsChristine) };
        Test<IList<IEvent>>(info.AssetId, null, (x, s, c) => Loaders.ScriptLoader.Serdes(x, info, s, c));
    } //*/

    [Fact]
    public void SongTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(Song.Toronto) };
        Test<byte[]>(info.AssetId, null, (x, s, c) => Loaders.SongLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void SpellTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(Spell.FrostAvalanche) };
        Test<SpellData>(info.AssetId, null, (x, s, c) => Loaders.SpellLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void TilesetTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(Tileset.Outdoors) };
        Test<TilesetData>(info.AssetId, null, (x, s, c) => Loaders.TilesetLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void WaveLibTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(WaveLibrary.Unknown5) };
        Test<WaveLib>(info.AssetId, null, (x, s, c) => Loaders.WaveLibLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void WordTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(Special.Words1) };
        Test<ListStringCollection>(
            info.AssetId,
            AssetMapping.Global.EnumerateAssetsOfType(AssetType.Word).ToArray(),
            (x, s, c) => Loaders.WordListLoader.Serdes(x, info, s, c));
    }
    //*
    [Fact]
    public void AutomapGfxTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(AutomapTiles.Set1) };
        info.Set(AssetProperty.SubSprites, "(8,8,576) (16,16)");
        Test<IReadOnlyTexture<byte>>(info.AssetId,
            new[] { AssetId.From(Palette.Common), AssetId.From(Palette.Unknown11) },
            (x, s, c) => Loaders.AmorphousSpriteLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void CombatBgTest()
    {
        var info = new AssetInfo
        {
            AssetId = AssetId.From(CombatBackground.Toronto),
            Width = 360
        };
        Test<IReadOnlyTexture<byte>>(info.AssetId,
            new[] { AssetId.From(Palette.TorontoCombat), AssetId.From(Palette.Common) },
            (x, s, c) => Loaders.FixedSizeSpriteLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void DungeonObjectTest()
    {
        var info = new AssetInfo
        {
            AssetId = AssetId.From(DungeonObject.Krondir),
            Width = 145,
            Height = 165
        };
        Test<IReadOnlyTexture<byte>>(info.AssetId,
            new[] { AssetId.From(Palette.JirinaarDay), AssetId.From(Palette.Common) },
            (x, s, c) => Loaders.FixedSizeSpriteLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void FontGfxTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(FontGfx.RegularFont), Width = 8, Height = 8 };
        Test<IReadOnlyTexture<byte>>(info.AssetId,
            new[] { AssetId.From(Palette.Common) },
            (x, s, c) => Loaders.FixedSizeSpriteLoader.Serdes(x, info, s, c));
    }


    [Fact]
    public void ItemSpriteTest()
    {
        var info = new AssetInfo
        {
            AssetId = AssetId.From(ItemGfx.ItemSprites),
            Width = 16,
            Height = 16
        };
        Test<IReadOnlyTexture<byte>>(info.AssetId,
            new[] { AssetId.From(Palette.Common) },
            (x, s, c) => Loaders.FixedSizeSpriteLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void SlabTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(UiBackground.Slab), Width = 360 };
        Test<IReadOnlyTexture<byte>>(info.AssetId,
            new[] { AssetId.From(Palette.Common) },
            (x, s, c) => Loaders.SlabLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void TileGfxTest()
    {
        var info = new AssetInfo
        {
            AssetId = AssetId.From(TilesetGfx.Toronto),
            Width = 16,
            Height = 16
        };

        var palettes = AssetMapping.Global.EnumerateAssetsOfType(AssetType.Palette);
        var prereqs = palettes.Append(AssetId.From(Palette.Toronto2D)).ToArray();

        Test<ITileGraphics>(info.AssetId, prereqs, (x, s, c) => Loaders.TilesetGraphicsLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void CombatGfxTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(CombatGfx.Unknown27) };
        Test<IReadOnlyTexture<byte>>(info.AssetId,
            new[] { AssetId.From(Palette.PlainsCombat), AssetId.From(Palette.Common) },
            (x, s, c) => Loaders.MultiHeaderSpriteLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void DungeonBgTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(DungeonBackground.EarlyGameL) };
        Test<IReadOnlyTexture<byte>>(info.AssetId,
            new[] { AssetId.From(Palette.JirinaarDay), AssetId.From(Palette.Common) },
            (x, s, c) => Loaders.SingleHeaderSpriteLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void FloorTest()
    {
        var info = new AssetInfo
        {
            AssetId = AssetId.From(Floor.Water),
            Width = 64,
            Height = 64
        };
        Test<IReadOnlyTexture<byte>>(info.AssetId,
            new[] { AssetId.From(Palette.JirinaarDay), AssetId.From(Palette.Common) },
            (x, s, c) => Loaders.FixedSizeSpriteLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void FullBodyPictureTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(PartyInventoryGfx.Tom) };
        Test<IReadOnlyTexture<byte>>(info.AssetId,
            new[] { AssetId.From(Palette.Inventory), AssetId.From(Palette.Common) },
            (x, s, c) => Loaders.SingleHeaderSpriteLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void LargeNpcTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(NpcLargeGfx.Christine) };
        Test<IReadOnlyTexture<byte>>(info.AssetId,
            new[] { AssetId.From(Palette.Toronto2D), AssetId.From(Palette.Common) },
            (x, s, c) => Loaders.SingleHeaderSpriteLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void LargePartyMemberTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(PartyLargeGfx.Tom) };
        Test<IReadOnlyTexture<byte>>(info.AssetId,
            new[] { AssetId.From(Palette.IskaiIndoorDark), AssetId.From(Palette.Common) },
            (x, s, c) => Loaders.SingleHeaderSpriteLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void MonsterGfxTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(MonsterGfx.Krondir) };
        Test<IReadOnlyTexture<byte>>(info.AssetId,
            new[] { AssetId.From(Palette.DungeonCombat), AssetId.From(Palette.Common) },
            (x, s, c) => Loaders.MultiHeaderSpriteLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void OverlayTest()
    {
        var info = new AssetInfo
        {
            AssetId = AssetId.From(WallOverlay.JiriWindow),
            Width = 44,
            File = new AssetFileInfo()
        };
        info.File.Set(AssetProperty.Transposed, true);
        Test<IReadOnlyTexture<byte>>(info.AssetId,
            new[] { AssetId.From(Palette.JirinaarDay), AssetId.From(Palette.Common) },
            (x, s, c) => Loaders.FixedSizeSpriteLoader.Serdes(x, info, s, c));
    }

    /* No code to write these atm, if anyone wants to mod them or add new ones they can still use ImageMagick or something to convert to ILBM
    [Fact]
    public void PictureTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(Picture.OpenChestWithGold) };
        Test<InterlacedBitmap>(info.AssetId, null, (x, s, c) => Loaders.InterlacedBitmapLoader.Serdes(x, info, s, c));
    } //*/

    [Fact]
    public void PortraitTest()
    {
        var info = new AssetInfo
        {
            AssetId = AssetId.From(Portrait.Tom),
            Width = 34
        };
        Test<IReadOnlyTexture<byte>>(info.AssetId,
            new[] { AssetId.From(Palette.Common) },
            (x, s, c) => Loaders.FixedSizeSpriteLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void SmallNpcTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(NpcSmallGfx.Krondir) };
        Test<IReadOnlyTexture<byte>>(info.AssetId,
            new[] { AssetId.From(Palette.FirstIslandDay), AssetId.From(Palette.Common) },
            (x, s, c) => Loaders.SingleHeaderSpriteLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void SmallPartyMemberTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(PartySmallGfx.Tom) };
        Test<IReadOnlyTexture<byte>>(info.AssetId,
            new[] { AssetId.From(Palette.FirstIslandDay), AssetId.From(Palette.Common) },
            (x, s, c) => Loaders.SingleHeaderSpriteLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void TacticalGfxTest()
    {
        var info = new AssetInfo
        {
            AssetId = AssetId.From(TacticalGfx.Tom),
            Width = 32
        };
        Test<IReadOnlyTexture<byte>>(info.AssetId,
            new[] { AssetId.From(Palette.Common) },
            (x, s, c) => Loaders.FixedSizeSpriteLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void WallTest()
    {
        var info = new AssetInfo
        {
            AssetId = AssetId.From(Wall.TorontoPanelling),
            Width = 80
        };
        Test<IReadOnlyTexture<byte>>(info.AssetId,
            new[] { AssetId.From(Palette.Toronto3D), AssetId.From(Palette.Common) },
            (x, s, c) => Loaders.FixedSizeSpriteLoader.Serdes(x, info, s, c));
    }
    // */
}