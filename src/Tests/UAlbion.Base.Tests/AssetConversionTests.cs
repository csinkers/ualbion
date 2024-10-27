using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Inv;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Assets.Sheets;
using UAlbion.Formats.MapEvents;
using UAlbion.Formats.Parsers;
using UAlbion.Game.Assets;
using UAlbion.TestCommon;
using Xunit;
using Xunit.Abstractions;

namespace UAlbion.Base.Tests;

public class AssetConversionTests : TestWithLogging
{
    const string BaseAssetMod = "Albion";
    const string UnpackedAssetMod = "Unpacked";
    const string RepackedAssetMod = "Repacked";

    static readonly IJsonUtil JsonUtil = new FormatJsonUtil();
    readonly string _baseDir;
    readonly IFileSystem _disk;
    readonly IModApplier _baseApplier;

    public AssetConversionTests(ITestOutputHelper testOutput) : base(testOutput)
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

    void Test<T>(
        AssetId id,
        AssetId[] prerequisites,
        Asset.SerdesFunc<T> serdes,
        Func<T, T> canonicalize = null) where T : class
    {
        prerequisites ??= Array.Empty<AssetId>();
        var resultsDir = Path.Combine(_baseDir, "re", "ConversionTests");

        var baseAsset = (T)_baseApplier.LoadAsset(id);
        if (canonicalize != null)
            baseAsset = canonicalize(baseAsset);

        var mapping = AssetMapping.Global;
        var stubDisk = new StubFileSystem();
        var modContext = new ModContext("Test", JsonUtil, stubDisk, mapping);
        var node = _baseApplier.GetAssetInfo(id);
        var context = new AssetLoadContext(id, node, modContext);

        var (baseBytes, baseNotes) = Asset.Save(baseAsset, serdes, context);
        var baseJson = Asset.SaveJson(baseAsset, JsonUtil);

        var idSet = new[] { id }.ToHashSet();
        var prereqSet = prerequisites.ToHashSet();
        var assetTypes = prerequisites.Select(x => x.Type).Distinct().ToHashSet();
        assetTypes.Add(id.Type);

        using (var unpacker = new AssetConverter(
                   "ualbion-tests",
                   mapping,
                   _disk,
                   JsonUtil,
                   new[] { BaseAssetMod },
                   UnpackedAssetMod))
        {
            if (prerequisites.Length > 0)
                unpacker.Convert(prereqSet, assetTypes, null);
            unpacker.Convert(idSet, assetTypes, null);
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
                   "ualbion-tests", 
                   mapping,
                   _disk,
                   JsonUtil,
                   new[] { UnpackedAssetMod },
                   RepackedAssetMod))
        {
            if (prerequisites.Length > 0)
                repacker.Convert(prereqSet, assetTypes, null);

            repacker.Convert(idSet, assetTypes, null);
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
        var id = AssetId.From(Item.Knife);
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

        Test<ItemData>(id, null, (x, s, c) => itemDataLoader.Serdes(x, s, c));
    }

    [Fact]
    public void ItemNameTest()
    {
        var id = AssetId.From(Special.ItemNamesMultiLang);
        Test<Dictionary<string, ListStringSet>>(id,
            AssetId.EnumerateAll(AssetType.ItemName).ToArray(),
            (x, s, c) => Loaders.ItemNameLoader.Serdes(x, s, c));
    }

    [Fact]
    public void AutomapTest()
    {
        var id = AssetId.From(Automap.Jirinaar);
        Test<Formats.Assets.Automap>(id, null, (x, s, c) => Loaders.AutomapLoader.Serdes(x, s, c));
    }

    [Fact]
    public void BlockListTest()
    {
        var id = AssetId.From(BlockList.Toronto);
        Test<Formats.Assets.BlockList>(id, null, (x, s, c) => Loaders.BlockListLoader.Serdes(x, s, c));
    }

    [Fact]
    public void ChestTest()
    {
        var id = AssetId.From(Chest.HClanCellar_ID_IKn_ILC_StC_LSh_3g);
        Test<Inventory>(
            id,
            AssetId.EnumerateAll(AssetType.Item).ToArray(),
            (x, s, c) => Loaders.ChestLoader.Serdes(x, s, c));
    }

    [Fact]
    public void CommonPaletteTest()
    {
        var id = AssetId.From(Palette.Common);
        Test<AlbionPalette>(id, null, (x, s, c) => Loaders.PaletteLoader.Serdes(x, s, c));
    }

    [Fact]
    public void EventSetTest()
    {
        var id = AssetId.From(EventSet.Frill);
        Test<Formats.Assets.EventSet>(
            id,
            null,
            (x, s, c) => Loaders.EventSetLoader.Serdes(x, s, c),
            LayoutTestUtil.CanonicalizeEventSet);
    }

    [Fact]
    public void EventTextTest()
    {
        var id = AssetId.From(EventText.Frill);
        Test<ListStringSet>(id, null, (x, s, c) => Loaders.AlbionStringTableLoader.Serdes(x, s, c));
    }

    [Fact]
    public void LabyrinthTest()
    {
        var id = AssetId.From(Labyrinth.Jirinaar);
        Test<Formats.Assets.Labyrinth.LabyrinthData>(id, null, (x, s, c) => Loaders.LabyrinthDataLoader.Serdes(x, s, c));
    }

    [Fact]
    public void Map2DTest_200() // An outdoor level
    {
        var id = AssetId.From(Map.Nakiridaani);
        var palettes = AssetMapping.Global.EnumerateAssetsOfType(AssetType.Palette);
        var prereqs = new[] { AssetId.From(Special.TiledNpcsSmall) }.Concat(palettes).ToArray();

        Test<MapData2D>(
            id,
            prereqs,
            (x, s, c) => MapData2D.Serdes(id, x, c.Mapping, s),
            LayoutTestUtil.CanonicalizeMap);
    }

    [Fact]
    public void Map2DTest_300() // Starting level
    {
        var id = AssetId.From(Map.TorontoBegin);
        var palettes = AssetMapping.Global.EnumerateAssetsOfType(AssetType.Palette);
        var prereqs = new[] { AssetId.From(Special.TiledNpcsLarge) }.Concat(palettes).ToArray();

        Test<MapData2D>(
            id,
            prereqs,
            (x, s, c) => MapData2D.Serdes(id, x, c.Mapping, s),
            LayoutTestUtil.CanonicalizeMap);
    }

    [Fact]
    public void Map3DTest_110() // A town level with automap markers
    {
        var prereqs = new[] { AssetId.From(Labyrinth.Jirinaar) };
        var id = AssetId.From(Map.Jirinaar);
        Test<MapData3D>(
            id,
            prereqs,
            (x, s, c) => MapData3D.Serdes(id, x, c.Mapping, s),
            LayoutTestUtil.CanonicalizeMap);
    }

    [Fact]
    public void Map3DTest_122() // A dungeon level
    {
        var prereqs = new[] { AssetId.From(Labyrinth.Argim) };
        var id = AssetId.From(Map.OldFormerBuilding);
        Test<MapData3D>(
            id,
            prereqs,
            (x, s, c) => MapData3D.Serdes(id, x, c.Mapping, s),
            LayoutTestUtil.CanonicalizeMap);
    }

    [Fact]
    public void MapTextTest()
    {
        var id = AssetId.From(MapText.TorontoBegin);
        Test<ListStringSet>(id, null, (x, s, c) => Loaders.AlbionStringTableLoader.Serdes(x, s, c));
    }

    [Fact]
    public void MerchantTest()
    {
        var id = AssetId.From(Merchant.AltheaSpells);
        Test<Inventory>(
            id,
            AssetId.EnumerateAll(AssetType.Item).ToArray(),
            (x, s, c) => Loaders.MerchantLoader.Serdes(x, s, c));
    }

    [Fact]
    public void BrokenMerchantTest()
    {
        var id = AssetId.From(Merchant.Unknown1);
        Test<Inventory>(
            id,
            AssetId.EnumerateAll(AssetType.Item).ToArray(),
            (x, s, c) => Loaders.MerchantLoader.Serdes(x, s, c));
    }

    [Fact]
    public void MonsterGroupTest()
    {
        var id = AssetId.From(MonsterGroup.TwoSkrinn1OneKrondir1);
        Test<Formats.Assets.MonsterGroup>(id, null, (x, s, c) => Loaders.MonsterGroupLoader.Serdes(x, s, c));
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
        var id = AssetId.From(MonsterSheet.Krondir1);
        var loader = BuildCharacterLoader();
        Test<CharacterSheet>(
            id,
            AssetId.EnumerateAll(AssetType.Item).ToArray(),
            (x, s, c) => loader.Serdes(x, s, c));
    }

    [Fact]
    public void NpcTest()
    {
        var id = AssetId.From(NpcSheet.Christine);
        var loader = BuildCharacterLoader();
        Test<CharacterSheet>(id, null, (x, s, c) => loader.Serdes(x, s, c));
    }

    [Fact]
    public void PaletteTest()
    {
        var id = AssetId.From(Palette.Toronto2D);
        Test<AlbionPalette>(id, new[] { AssetId.From(Palette.Common) }, (x, s, c) => Loaders.PaletteLoader.Serdes(x, s, c));
    }

    [Fact]
    public void PartySheetTest()
    {
        var id = AssetId.From(PartySheet.Tom);
        var loader = BuildCharacterLoader();
        Test<CharacterSheet>(
            id,
            AssetId.EnumerateAll(AssetType.Item).ToArray(),
            (x, s, c) => loader.Serdes(x, s, c));
    }

    [Fact]
    public void SampleTest()
    {
        var id = AssetId.From(Sample.IllTemperedLlama);
        Test<AlbionSample>(id, null, (x, s, c) => Loaders.SampleLoader.Serdes(x, s, c));
    }

    /* They're text anyway so not too bothered - at the moment they don't round trip due to using friendly asset id names
    // Would need to add a ToStringNumeric or something to the relevant events, starts getting ugly.
    [Fact]
    public void ScriptTest()
    {
        var id = AssetId.From(Script.TomMeetsChristine);
        Test<IList<IEvent>>(id, null, (x, s, c) => Loaders.ScriptLoader.Serdes(x, s, c));
    } //*/

    [Fact]
    public void SongTest()
    {
        var id = AssetId.From(Song.Toronto);
        Test<byte[]>(id, null, (x, s, c) => Loaders.SongLoader.Serdes(x, s, c));
    }

    [Fact]
    public void SpellTest()
    {
        var id = AssetId.From(Spell.FrostAvalanche);
        Test<SpellData>(id, null, (x, s, c) => Loaders.SpellLoader.Serdes(x, s, c));
    }

    [Fact]
    public void TilesetTest()
    {
        var id = AssetId.From(Tileset.Outdoors);
        Test<TilesetData>(id, null, (x, s, c) => Loaders.TilesetLoader.Serdes(x, s, c));
    }

    [Fact]
    public void WaveLibTest()
    {
        var id = AssetId.From(WaveLibrary.Unknown5);
        Test<WaveLib>(id, null, (x, s, c) => Loaders.WaveLibLoader.Serdes(x, s, c));
    }

    [Fact]
    public void Words1Test()
    {
        var id = AssetId.From(Special.Words1);
        Test<ListStringSet>(
            id,
            AssetMapping.Global.EnumerateAssetsOfType(AssetType.Word).ToArray(),
            (x, s, c) => Loaders.WordListLoader.Serdes(x, s, c));
    }

    [Fact]
    public void Words2Test()
    {
        var id = AssetId.From(Special.Words2);
        Test<ListStringSet>(
            id,
            AssetMapping.Global.EnumerateAssetsOfType(AssetType.Word).ToArray(),
            (x, s, c) => Loaders.WordListLoader.Serdes(x, s, c));
    }

    [Fact]
    public void Words3Test()
    {
        var id = AssetId.From(Special.Words3);
        Test<ListStringSet>(
            id,
            AssetMapping.Global.EnumerateAssetsOfType(AssetType.Word).ToArray(),
            (x, s, c) => Loaders.WordListLoader.Serdes(x, s, c));
    }

    //*
    [Fact]
    public void AutomapGfxTest()
    {
        var id = AssetId.From(AutomapTiles.Set1);
        Test<IReadOnlyTexture<byte>>(id,
            new[] { AssetId.From(Palette.Common), AssetId.From(Palette.Unknown11) },
            (x, s, c) => Loaders.AmorphousSpriteLoader.Serdes(x, s, c));
    }

    [Fact]
    public void CombatBgTest()
    {
        var id = AssetId.From(CombatBackground.Toronto);
        Test<IReadOnlyTexture<byte>>(id,
            new[] { AssetId.From(Palette.TorontoCombat), AssetId.From(Palette.Common) },
            (x, s, c) => Loaders.FixedSizeSpriteLoader.Serdes(x, s, c));
    }

    [Fact]
    public void DungeonObjectTest()
    {
        var id = AssetId.From(DungeonObject.Krondir);
        Test<IReadOnlyTexture<byte>>(id,
            new[] { AssetId.From(Palette.JirinaarDay), AssetId.From(Palette.Common) },
            (x, s, c) => Loaders.FixedSizeSpriteLoader.Serdes(x, s, c));
    }

    [Fact]
    public void FontGfxTest()
    {
        var id = AssetId.From(FontGfx.Regular);
        Test<IReadOnlyTexture<byte>>(id,
            new[] { AssetId.From(Palette.Common), AssetId.From(FontGfx.Bold) },
            (x, s, c) => Loaders.FixedSizeSpriteLoader.Serdes(x, s, c));
    }

    [Fact]
    public void ItemSpriteTest()
    {
        var id = AssetId.From(ItemGfx.ItemSprites);
        Test<IReadOnlyTexture<byte>>(id,
            new[] { AssetId.From(Palette.Common) },
            (x, s, c) => Loaders.FixedSizeSpriteLoader.Serdes(x, s, c));
    }

    [Fact]
    public void SlabTest()
    {
        var id = AssetId.From(UiBackground.Slab); 
        Test<IReadOnlyTexture<byte>>(id,
            new[] { AssetId.From(Palette.Common) },
            (x, s, c) => Loaders.SlabLoader.Serdes(x, s, c));
    }

    [Fact]
    public void TileGfxTest()
    {
        var id = AssetId.From(TilesetGfx.Toronto);

        var palettes = AssetMapping.Global.EnumerateAssetsOfType(AssetType.Palette);
        var prereqs = palettes.Append(AssetId.From(Palette.Toronto2D)).ToArray();

        Test<ITileGraphics>(id, prereqs, (x, s, c) => Loaders.TilesetGraphicsLoader.Serdes(x, s, c));
    }

    [Fact]
    public void CombatGfxTest()
    {
        var id = AssetId.From(CombatGfx.SplashYellow);
        Test<IReadOnlyTexture<byte>>(id,
            new[] { AssetId.From(Palette.PlainsCombat), AssetId.From(Palette.Common) },
            (x, s, c) => Loaders.MultiHeaderSpriteLoader.Serdes(x, s, c));
    }

    [Fact]
    public void DungeonBgTest()
    {
        var id = AssetId.From(DungeonBackground.EarlyGameL);
        Test<IReadOnlyTexture<byte>>(id,
            new[] { AssetId.From(Palette.JirinaarDay), AssetId.From(Palette.Common) },
            (x, s, c) => Loaders.SingleHeaderSpriteLoader.Serdes(x, s, c));
    }

    [Fact]
    public void FloorTest()
    {
        var id = AssetId.From(Floor.Water);

        Test<IReadOnlyTexture<byte>>(id,
            new[] { AssetId.From(Palette.JirinaarDay), AssetId.From(Palette.Common) },
            (x, s, c) => Loaders.FixedSizeSpriteLoader.Serdes(x, s, c));
    }

    [Fact]
    public void FullBodyPictureTest()
    {
        var id = AssetId.From(PartyInventoryGfx.Tom);
        Test<IReadOnlyTexture<byte>>(id,
            new[] { AssetId.From(Palette.Inventory), AssetId.From(Palette.Common) },
            (x, s, c) => Loaders.SingleHeaderSpriteLoader.Serdes(x, s, c));
    }

    [Fact]
    public void LargeNpcTest()
    {
        var id = AssetId.From(NpcLargeGfx.Christine);
        Test<IReadOnlyTexture<byte>>(id,
            new[] { AssetId.From(Palette.Toronto2D), AssetId.From(Palette.Common) },
            (x, s, c) => Loaders.SingleHeaderSpriteLoader.Serdes(x, s, c));
    }

    [Fact]
    public void LargePartyMemberTest()
    {
        var id = AssetId.From(PartyLargeGfx.Tom);
        Test<IReadOnlyTexture<byte>>(id,
            new[] { AssetId.From(Palette.IskaiIndoorDark), AssetId.From(Palette.Common) },
            (x, s, c) => Loaders.SingleHeaderSpriteLoader.Serdes(x, s, c));
    }

    [Fact]
    public void MonsterGfxTest()
    {
        var id = AssetId.From(MonsterGfx.Krondir1);
        Test<IReadOnlyTexture<byte>>(id,
            new[] { AssetId.From(Palette.DungeonCombat), AssetId.From(Palette.Common) },
            (x, s, c) => Loaders.MultiHeaderSpriteLoader.Serdes(x, s, c));
    }

    [Fact]
    public void OverlayTest()
    {
        var id = AssetId.From(WallOverlay.JiriWindow);

        Test<IReadOnlyTexture<byte>>(id,
            new[] { AssetId.From(Palette.JirinaarDay), AssetId.From(Palette.Common) },
            (x, s, c) => Loaders.FixedSizeSpriteLoader.Serdes(x, s, c));
    }

    /* No code to write these atm, if anyone wants to mod them or add new ones they can still use ImageMagick or something to convert to ILBM
    [Fact]
    public void PictureTest()
    {
        var id = AssetId.From(Picture.OpenChestWithGold);
        Test<InterlacedBitmap>(id, null, (x, s, c) => Loaders.InterlacedBitmapLoader.Serdes(x, s, c));
    } //*/

    [Fact]
    public void PortraitTest()
    {
        var id = AssetId.From(Portrait.Tom);
        Test<IReadOnlyTexture<byte>>(id,
            new[] { AssetId.From(Palette.Common) },
            (x, s, c) => Loaders.FixedSizeSpriteLoader.Serdes(x, s, c));
    }

    [Fact]
    public void SmallNpcTest()
    {
        var id = AssetId.From(NpcSmallGfx.Krondir);
        Test<IReadOnlyTexture<byte>>(id,
            new[] { AssetId.From(Palette.FirstIslandDay), AssetId.From(Palette.Common) },
            (x, s, c) => Loaders.SingleHeaderSpriteLoader.Serdes(x, s, c));
    }

    [Fact]
    public void SmallPartyMemberTest()
    {
        var id = AssetId.From(PartySmallGfx.Tom);
        Test<IReadOnlyTexture<byte>>(id,
            new[] { AssetId.From(Palette.FirstIslandDay), AssetId.From(Palette.Common) },
            (x, s, c) => Loaders.SingleHeaderSpriteLoader.Serdes(x, s, c));
    }

    [Fact]
    public void TacticalGfxTest()
    {
        var id = AssetId.From(TacticalGfx.Tom);
        Test<IReadOnlyTexture<byte>>(id,
            new[] { AssetId.From(Palette.Common) },
            (x, s, c) => Loaders.FixedSizeSpriteLoader.Serdes(x, s, c));
    }

    [Fact]
    public void WallTest()
    {
        var id = AssetId.From(Wall.TorontoPanelling);
        Test<IReadOnlyTexture<byte>>(id,
            new[] { AssetId.From(Palette.Toronto3D), AssetId.From(Palette.Common) },
            (x, s, c) => Loaders.FixedSizeSpriteLoader.Serdes(x, s, c));
    }
    // */
}

public abstract class TestWithLogging : IDisposable
{
    protected ITestOutputHelper TestOutput { get; }

    protected TestWithLogging([NotNull] ITestOutputHelper testOutput) => TestOutput = testOutput ?? throw new ArgumentNullException(nameof(testOutput));

    protected virtual void Dispose(bool disposing)
    {
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~TestWithLogging() => Dispose(false);
}