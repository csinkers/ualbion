using System;
using System.IO;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Containers;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Assets;
using UAlbion.Game.Tests;
using UAlbion.TestCommon;
using Xunit;

namespace UAlbion.Base.Tests;

public class RoundtripTests
{
    static readonly IAssetManager DummyAssets = new DummyAssetManager();
    static readonly IJsonUtil JsonUtil = new FormatJsonUtil();
    static readonly GeneralConfig GeneralConfig;
    static readonly AssetMapping Mapping;
    static readonly IFileSystem Disk;
    static readonly string ResultDir;

    static RoundtripTests()
    {
        string baseDir;
        (Disk, baseDir, GeneralConfig, Mapping) = SetupAssets(JsonUtil);
        ResultDir = Path.Combine(baseDir, "re", "RoundTripTests");
    }

    public RoundtripTests()
    {
        AssetMapping.GlobalIsThreadLocal = true;
        AssetMapping.Global.MergeFrom(Mapping);
    }

    static (MockFileSystem disk, string baseDir, GeneralConfig generalConfig, AssetMapping mapping) SetupAssets(IJsonUtil jsonUtil)
    {
        Event.AddEventsFromAssembly(typeof(ActionEvent).Assembly);
        var mapping = new AssetMapping();
        var disk = new MockFileSystem(true);
        var baseDir = ConfigUtil.FindBasePath(disk);
        var baseAssetConfigPath = Path.Combine(baseDir, "mods", "Base", "assets.json");
        var assetConfigPath = Path.Combine(baseDir, "mods", "Albion", "assets.json");
        var baseAssetConfig = AssetConfig.Load(baseAssetConfigPath, null, mapping, disk, jsonUtil);
        var assetConfig = AssetConfig.Load(assetConfigPath, baseAssetConfig, mapping, disk, jsonUtil);

        var generalConfig = AssetSystem.LoadGeneralConfig(baseDir, disk, jsonUtil);

        foreach (var assetType in assetConfig.IdTypes.Values)
        {
            var enumType = Type.GetType(assetType.EnumType);
            if (enumType == null)
                throw new InvalidOperationException(
                    $"Could not load enum type \"{assetType.EnumType}\" defined in \"{assetConfigPath}\"");

            mapping.RegisterAssetType(assetType.EnumType, assetType.AssetType);
        }

        return (disk, baseDir, generalConfig, mapping);
    }

    static T RoundTrip<T>(string testName, byte[] bytes, Asset.SerdesFunc<T> serdes) where T : class
    {
        TResult Wrap<TResult>(Func<TResult> func, string extension) 
        {
            try { return func(); }
            catch (AssetSerializationException ase)
            {
                if (!Directory.Exists(ResultDir))
                    Directory.CreateDirectory(ResultDir);

                var path = Path.Combine(ResultDir, testName);
                if (!string.IsNullOrEmpty(ase.Annotation))
                    File.WriteAllText(path + extension, ase.Annotation);
                throw;
            }
        }

        var context = new LoaderContext(DummyAssets, JsonUtil, AssetMapping.Global);

        var (asset, preTxt)      = Wrap(() => Asset.Load(bytes, serdes, context), ".pre.ex.txt");
        var (postBytes, postTxt) = Wrap(() => Asset.Save(asset, serdes, context), ".post.ex.txt");
        var (_, reloadTxt)       = Wrap(() => Asset.Load(postBytes, serdes, context), ".reload.ex.txt");

        Asset.Compare(ResultDir,
            testName,
            bytes,
            postBytes,
            new[] { (".pre.txt", preTxt), (".post.txt", postTxt), (".reload.txt", reloadTxt) });

        if (asset is IReadOnlyTexture<byte>) // TODO: Png round-trip?
            return asset;

        var json = Asset.SaveJson(asset, JsonUtil);
        var fromJson = Asset.LoadJson<T>(json, JsonUtil);
        var (fromJsonBytes, fromJsonTxt) = Asset.Save(fromJson, serdes, context);
        Asset.Compare(ResultDir,
            testName + ".json",
            bytes,
            fromJsonBytes,
            new[] { (".pre.txt", preTxt), (".post.txt", json), (".reload.txt", fromJsonTxt) });

        return asset;
    }

    void RoundTripXld<T>(string testName, string file, int subId, Asset.SerdesFunc<T> serdes) where T : class
    {
        var info = new AssetInfo { Index = subId };
        var bytes = Asset.BytesFromXld(GeneralConfig, file, info, Disk, JsonUtil);
        RoundTrip(testName, bytes, serdes);
    }

    void RoundTripRaw<T>(string testName, string file, Asset.SerdesFunc<T> serdes) where T : class
    {
        var bytes = File.ReadAllBytes(GeneralConfig.ResolvePath(file));
        RoundTrip(testName, bytes, serdes);
    }

    void RoundTripItem<T>(string testName, string file, int subId, Asset.SerdesFunc<T> serdes) where T : class
    {
        var info = new AssetInfo { Index = subId };
        var loader = new ItemListContainer();
        using var s = loader.Read(GeneralConfig.ResolvePath(file), info, Disk, JsonUtil);
        var bytes = s.Bytes(null, null, (int)s.BytesRemaining);
        RoundTrip(testName, bytes, serdes);
    }

    void RoundTripSpell<T>(string testName, string file, int subId, Asset.SerdesFunc<T> serdes) where T : class
    {
        var info = new AssetInfo { Index = subId };
        var loader = new SpellListContainer();
        using var s = loader.Read(GeneralConfig.ResolvePath(file), info, Disk, JsonUtil);
        var bytes = s.Bytes(null, null, (int)s.BytesRemaining);
        RoundTrip(testName, bytes, serdes);
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

        RoundTripItem<ItemData>(nameof(ItemTest), "$(XLD)/ITEMLIST.DAT", 10,
            (x, s, c) => itemDataLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void ItemNameTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(Special.ItemNames) };
        RoundTripRaw<MultiLanguageStringDictionary>(nameof(ItemNameTest), "$(XLD)/ITEMNAME.DAT",
            (x, s, c) => Loaders.ItemNameLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void AutomapTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(Automap.Jirinaar) };
        RoundTripXld<Formats.Assets.Automap>(nameof(AutomapTest), "$(XLD)/INITIAL/AUTOMAP1.XLD", 10,
            (x, s, c) => Loaders.AutomapLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void BlockListTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(BlockList.Toronto) };
        RoundTripXld<Formats.Assets.BlockList>(nameof(BlockListTest), "$(XLD)/BLKLIST0.XLD", 7,
            (x, s, c) => Loaders.BlockListLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void ChestTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(Chest.Unknown121) };
        RoundTripXld<Inventory>(nameof(ChestTest), "$(XLD)/INITIAL/CHESTDT1.XLD", 21,
            (x, s, c) => Loaders.ChestLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void CommonPaletteTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(Palette.Common) };
        info.Set(AssetProperty.IsCommon, true);
        RoundTripRaw<AlbionPalette>(nameof(CommonPaletteTest), "$(XLD)/PALETTE.000",
            (x, s, c) => Loaders.PaletteLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void EventSetTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(EventSet.Frill) };
        RoundTripXld<Formats.Assets.EventSet>(nameof(EventSetTest), "$(XLD)/EVNTSET1.XLD", 11,
            (x, s, c) => Loaders.EventSetLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void EventTextTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(EventText.Frill) };
        RoundTripXld<ListStringCollection>(nameof(EventTextTest), "$(XLD)/ENGLISH/EVNTTXT1.XLD", 11,
            (x, s, c) => Loaders.AlbionStringTableLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void LabyrinthTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(Labyrinth.Jirinaar) };
        RoundTripXld<Formats.Assets.Labyrinth.LabyrinthData>(nameof(LabyrinthTest), "$(XLD)/LABDATA1.XLD", 9,
            (x, s, c) => Loaders.LabyrinthDataLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void Map2DTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(Map.TorontoBegin) };
        RoundTripXld<MapData2D>(nameof(Map2DTest), "$(XLD)/MAPDATA3.XLD", 0,
            (x, s, c) => MapData2D.Serdes(info, x, c.Mapping, s));
    }

    [Fact]
    public void Map3DTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(Map.OldFormerBuilding) };
        RoundTripXld<MapData3D>(nameof(Map3DTest), "$(XLD)/MAPDATA1.XLD", 22,
            (x, s, c) => MapData3D.Serdes(info, x, c.Mapping, s));
    }

    [Fact]
    public void MapTextTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(MapText.TorontoBegin) };
        RoundTripXld<ListStringCollection>(nameof(MapTextTest), "$(XLD)/ENGLISH/MAPTEXT3.XLD", 0,
            (x, s, c) => Loaders.AlbionStringTableLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void MerchantTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(Merchant.Unknown109) };
        RoundTripXld<Inventory>(nameof(MerchantTest), "$(XLD)/INITIAL/MERCHDT1.XLD", 9,
            (x, s, c) => Loaders.MerchantLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void MonsterGroupTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(MonsterGroup.TwoSkrinn1OneKrondir1) };
        RoundTripXld<Formats.Assets.MonsterGroup>(nameof(MonsterGroupTest), "$(XLD)/MONGRP0.XLD", 9,
            (x, s, c) => Loaders.MonsterGroupLoader.Serdes(x, info, s, c));
    }

    static SpellData BuildMockSpell(SpellId id, SpellClass school, byte number) => new(id, school, number)
    {
        Cost = 1,
        Environments = SpellEnvironments.Combat,
        LevelRequirement = 2,
        Targets = SpellTargets.OneMonster,
    };

    static CharacterSheetLoader BuildCharacterLoader()
    {
        var spellManager = new MockSpellManager()
                .Add(BuildMockSpell(Spell.ThornSnare, SpellClass.DjiKas, 0))
                .Add(BuildMockSpell(Spell.Fireball, SpellClass.OquloKamulos, 0))
                .Add(BuildMockSpell(Spell.LightningStrike, SpellClass.OquloKamulos, 1))
                .Add(BuildMockSpell(Spell.FireRain, SpellClass.OquloKamulos, 2))
                .Add(BuildMockSpell(Spell.RemoveTrapKK, SpellClass.OquloKamulos, 14))
                .Add(BuildMockSpell(Spell.Unused106, SpellClass.OquloKamulos, 15))
            ;

        CharacterSheetLoader characterSheetLoader = new();
        new EventExchange()
            .Attach(spellManager)
            .Attach(characterSheetLoader);

        return characterSheetLoader;
    }

    [Fact]
    public void MonsterTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(Monster.Krondir1) };
        var loader = BuildCharacterLoader();
        RoundTripXld<CharacterSheet>(nameof(MonsterTest), "$(XLD)/MONCHAR0.XLD", 9,
            (x, s, c) => loader.Serdes(x, info, s, c));
    }

    [Fact]
    public void NpcTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(Npc.Christine) };
        var loader = BuildCharacterLoader();
        RoundTripXld<CharacterSheet>(nameof(NpcTest), "$(XLD)/INITIAL/NPCCHAR1.XLD", 83,
            (x, s, c) => loader.Serdes(x, info, s, c));
    }

    [Fact]
    public void PaletteTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(Palette.Toronto2D) };
        RoundTripXld<AlbionPalette>(nameof(PaletteTest), "$(XLD)/PALETTE0.XLD", 25,
            (x, s, c) => Loaders.PaletteLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void PartyMemberTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(PartyMember.Tom) };
        var loader = BuildCharacterLoader();
        RoundTripXld<CharacterSheet>(nameof(PartyMemberTest), "$(XLD)/INITIAL/PRTCHAR0.XLD", 0,
            (x, s, c) => loader.Serdes(x, info, s, c));
    }

    [Fact]
    public void SampleTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(Sample.IllTemperedLlama) };
        RoundTripXld<AlbionSample>(nameof(SampleTest), "$(XLD)/SAMPLES0.XLD", 47,
            (x, s, c) => Loaders.SampleLoader.Serdes(x, info, s, c));
    }

    /* They're text anyway so not too bothered - at the moment they don't round trip due to using friendly asset id names
    // Would need to add a ToStringNumeric or something to the relevant events, starts getting ugly.
    [Fact]
    public void ScriptTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(Script.TomMeetsChristine) };
        RoundTripXld<IList<IEvent>>(nameof(ScriptTest), "$(XLD)/SCRIPT0.XLD", 1,
            (x, s, c) => Loaders.ScriptLoader.Serdes(x, info, s, c));
    } //*/

    [Fact]
    public void SongTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(Song.Toronto) };
        RoundTripXld<byte[]>(nameof(SongTest), "$(XLD)/SONGS0.XLD", 3,
            (x, s, c) => Loaders.SongLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void SpellTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(Spell.FrostAvalanche) };
        RoundTripSpell<SpellData>(nameof(SpellTest), "$(XLD)/SPELLDAT.DAT", 7,
            (x, s, c) => Loaders.SpellLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void TilesetTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(Tileset.Toronto) };
        RoundTripXld<TilesetData>(nameof(TilesetTest), "$(XLD)/ICONDAT0.XLD", 7,
            (x, s, c) => Loaders.TilesetLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void TiledTilesetTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(Tileset.Toronto), Index = 7 };
        var gfxInfo = new AssetInfo {AssetId = AssetId.From(TilesetGraphics.Toronto), Index = 7};
        gfxInfo.Set(AssetProperty.PaletteId, 26);

        var modApplier = new MockModApplier();
        modApplier.AddInfo(gfxInfo.AssetId, gfxInfo);
        var exchange = new EventExchange()
            .Attach(modApplier)
            .Attach(new AssetManager());

        var bytes = Asset.BytesFromXld(GeneralConfig, "$(XLD)/ICONDAT0.XLD", info, Disk, JsonUtil);
        var loaderContext = new LoaderContext(DummyAssets, JsonUtil, AssetMapping.Global);

        TilesetData Serdes(TilesetData x, ISerializer s, LoaderContext context) => Loaders.TilesetLoader.Serdes(x, info, s, context);
        var (asset, preTxt) = Asset.Load<TilesetData>(bytes, Serdes, loaderContext);

        var loader = new TiledTilesetLoader();
        exchange.Attach(loader);
        var (tiledBytes, tiledTxt) = Asset.Save(asset, (x, s, c) => loader.Serdes(x, info, s, c), loaderContext);

        var (fromTiled, _) = Asset.Load<TilesetData>(tiledBytes,
            (x, s, c) => loader.Serdes(x, info, s, c), loaderContext);

        var (roundTripped, roundTripTxt) = Asset.Save(fromTiled, Serdes, loaderContext);
        Asset.Compare(ResultDir,
            nameof(TiledTilesetTest),
            bytes,
            roundTripped,
            new[] { (".pre.txt", preTxt), (".post.txt", tiledTxt), (".reload.txt", roundTripTxt)});
    }

    [Fact]
    public void TiledStampTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(BlockList.Toronto), Index = 7 };
        var bytes = Asset.BytesFromXld(GeneralConfig, "$(XLD)/BLKLIST0.XLD", info, Disk, JsonUtil);
        var loaderContext = new LoaderContext(DummyAssets, JsonUtil, AssetMapping.Global);

        Formats.Assets.BlockList Serdes(Formats.Assets.BlockList x, ISerializer s, LoaderContext context) => Loaders.BlockListLoader.Serdes(x, info, s, context);
        var (asset, preTxt) = Asset.Load<Formats.Assets.BlockList>(bytes, Serdes, loaderContext);

        var loader = new Formats.Exporters.Tiled.StampLoader();
        var (tiledBytes, tiledTxt) = Asset.Save(asset, (x, s, c) => loader.Serdes(x, info, s, c), loaderContext);

        var (fromTiled, _) = Asset.Load<Formats.Assets.BlockList>(tiledBytes,
            (x, s, c) => loader.Serdes(x, info, s, c),
            loaderContext);

        var (roundTripped, roundTripTxt) = Asset.Save(fromTiled, Serdes, loaderContext);
        Asset.Compare(ResultDir,
            nameof(TiledStampTest),
            bytes,
            roundTripped,
            new[] { (".pre.txt", preTxt), (".post.txt", tiledTxt), (".reload.txt", roundTripTxt)});
    }

    [Fact]
    public void TiledMap2dTest()
    {
    }

    [Fact]
    public void WaveLibTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(WaveLibrary.TorontoAmbient) };
        RoundTripXld<WaveLib>(nameof(WaveLibTest), "$(XLD)/WAVELIB0.XLD", 4,
            (x, s, c) => Loaders.WaveLibLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void WordTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(Special.Words1) };
        RoundTripXld<ListStringCollection>(nameof(WordTest), "$(XLD)/ENGLISH/WORDLIS0.XLD", 0,
            (x, s, c) => Loaders.WordListLoader.Serdes(x, info, s, c));
    }
//*
    [Fact]
    public void AutomapGfxTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(AutomapTiles.Set1) };
        info.Set(AssetProperty.SubSprites, "(8,8,576) (16,16)");
        RoundTripXld<IReadOnlyTexture<byte>>(nameof(AutomapGfxTest), "$(XLD)/AUTOGFX0.XLD", 0,
            (x, s, c) => Loaders.AmorphousSpriteLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void AmorphousTest()
    {
        var bytes = new byte[]
        {
            0,
            1,
            2,

            3, 4, 
            5, 6, 

            7, 8, 
            9, 10
        };
        var info = new AssetInfo();
        info.Set(AssetProperty.SubSprites, "(1,1,3) (2,2)");
        var sprite = RoundTrip<IReadOnlyTexture<byte>>(
            nameof(AmorphousTest),
            bytes,
            (x, s, c) => Loaders.AmorphousSpriteLoader.Serdes(x, info, s, c));

        Assert.Equal(2, sprite.Width);
        Assert.Equal(7, sprite.Height);
        Assert.Equal(5, sprite.Regions.Count);
        Assert.Collection(sprite.PixelData.ToArray(),
            x => Assert.Equal(0, x), x => Assert.Equal(0, x),
            x => Assert.Equal(1, x), x => Assert.Equal(0, x),
            x => Assert.Equal(2, x), x => Assert.Equal(0, x),
            x => Assert.Equal(3, x), x => Assert.Equal(4, x),
            x => Assert.Equal(5, x), x => Assert.Equal(6, x),
            x => Assert.Equal(7, x), x => Assert.Equal(8, x),
            x => Assert.Equal(9, x), x => Assert.Equal(10, x));
    }

    [Fact]
    public void CombatBgTest()
    {
        var info = new AssetInfo
        {
            AssetId = AssetId.From(CombatBackground.Toronto),
            Width = 360
        };
        RoundTripXld<IReadOnlyTexture<byte>>(nameof(CombatBgTest), "$(XLD)/COMBACK0.XLD", 0,
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
        RoundTripXld<IReadOnlyTexture<byte>>(nameof(DungeonObjectTest), "$(XLD)/3DOBJEC2.XLD", 81,
            (x, s, c) => Loaders.FixedSizeSpriteLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void FontTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(Font.RegularFont), Width = 8, Height = 8 };
        RoundTripXld<IReadOnlyTexture<byte>>(nameof(FontTest), "$(XLD)/FONTS0.XLD", 0,
            (x, s, c) => Loaders.FontSpriteLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void ItemSpriteTest()
    {
        var info = new AssetInfo
        {
            AssetId = AssetId.From(ItemGraphics.ItemSprites),
            Width = 16,
            Height = 16
        };
        RoundTripRaw<IReadOnlyTexture<byte>>(nameof(ItemSpriteTest), "$(XLD)/ITEMGFX",
            (x, s, c) => Loaders.FixedSizeSpriteLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void SlabTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(UiBackground.Slab), Width = 360 };
        RoundTripRaw<IReadOnlyTexture<byte>>(nameof(SlabTest), "$(XLD)/SLAB",
            (x, s, c) => Loaders.SlabLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void TileGfxTest()
    {
        var info = new AssetInfo
        {
            AssetId = AssetId.From(TilesetGraphics.Toronto),
            Width = 16,
            Height = 16
        };
        RoundTripXld<IReadOnlyTexture<byte>>(nameof(TileGfxTest), "$(XLD)/ICONGFX0.XLD", 7,
            (x, s, c) => Loaders.FixedSizeSpriteLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void CombatGfxTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(CombatGraphics.Unknown27) };
        RoundTripXld<IReadOnlyTexture<byte>>(nameof(CombatGfxTest), "$(XLD)/COMGFX0.XLD", 26,
            (x, s, c) => Loaders.MultiHeaderSpriteLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void DungeonBgTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(DungeonBackground.EarlyGameL) };
        RoundTripXld<IReadOnlyTexture<byte>>(nameof(DungeonBgTest), "$(XLD)/3DBCKGR0.XLD", 0,
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
        RoundTripXld<IReadOnlyTexture<byte>>(nameof(FloorTest), "$(XLD)/3DFLOOR0.XLD", 2,
            (x, s, c) => Loaders.FixedSizeSpriteLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void FullBodyPictureTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(FullBodyPicture.Tom) };
        RoundTripXld<IReadOnlyTexture<byte>>(nameof(FullBodyPictureTest), "$(XLD)/FBODPIX0.XLD", 0,
            (x, s, c) => Loaders.SingleHeaderSpriteLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void LargeNpcTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(LargeNpc.Christine) };
        RoundTripXld<IReadOnlyTexture<byte>>(nameof(LargeNpcTest), "$(XLD)/NPCGR0.XLD", 20,
            (x, s, c) => Loaders.SingleHeaderSpriteLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void LargePartyMemberTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(LargePartyMember.Tom) };
        RoundTripXld<IReadOnlyTexture<byte>>(nameof(LargePartyMemberTest), "$(XLD)/PARTGR0.XLD", 0,
            (x, s, c) => Loaders.SingleHeaderSpriteLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void MonsterGfxTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(MonsterGraphics.Krondir) };
        RoundTripXld<IReadOnlyTexture<byte>>(nameof(MonsterGfxTest), "$(XLD)/MONGFX0.XLD", 9,
            (x, s, c) => Loaders.MultiHeaderSpriteLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void OverlayTest()
    {
        var info = new AssetInfo
        {
            AssetId = AssetId.From(WallOverlay.JiriFrameL),
            Width = 112,
            File = new AssetFileInfo()
        };
        info.File.Set(AssetProperty.Transposed, true);
        RoundTripXld<IReadOnlyTexture<byte>>(nameof(OverlayTest), "$(XLD)/3DOVERL0.XLD", 17,
            (x, s, c) => Loaders.FixedSizeSpriteLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void OverlayMultiFrameTest()
    {
        var info = new AssetInfo
        {
            AssetId = AssetId.From(WallOverlay.Unknown201),
            Width = 6,
            Height = 20,
            File = new AssetFileInfo()
        };
        info.File.Set(AssetProperty.Transposed, true);
        RoundTripXld<IReadOnlyTexture<byte>>(nameof(OverlayTest), "$(XLD)/3DOVERL2.XLD", 1,
            (x, s, c) => Loaders.FixedSizeSpriteLoader.Serdes(x, info, s, c));
    }

    // 201

    /* No code to write these atm, if anyone wants to mod them or add new ones they can still use ImageMagick or something to convert to ILBM
    [Fact]
    public void PictureTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(Picture.OpenChestWithGold) };
        RoundTripXld<InterlacedBitmap>(nameof(PictureTest), "$(XLD)/PICTURE0.XLD", 11,
            (x, s, c) => Loaders.InterlacedBitmapLoader.Serdes(x, info, s, c));
    } //*/

    [Fact]
    public void PortraitTest()
    {
        var info = new AssetInfo
        {
            AssetId = AssetId.From(Portrait.Tom),
            Width = 34
        };
        RoundTripXld<IReadOnlyTexture<byte>>(nameof(PortraitTest), "$(XLD)/SMLPORT0.XLD", 0,
            (x, s, c) => Loaders.FixedSizeSpriteLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void SmallNpcTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(SmallNpc.Krondir) };
        RoundTripXld<IReadOnlyTexture<byte>>(nameof(SmallNpcTest), "$(XLD)/NPCKL0.XLD", 22,
            (x, s, c) => Loaders.SingleHeaderSpriteLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void SmallPartyMemberTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(SmallPartyMember.Tom) };
        RoundTripXld<IReadOnlyTexture<byte>>(nameof(SmallPartyMemberTest), "$(XLD)/PARTKL0.XLD", 0,
            (x, s, c) => Loaders.SingleHeaderSpriteLoader.Serdes(x, info, s, c));
    }

    [Fact]
    public void TacticalGfxTest()
    {
        var info = new AssetInfo
        {
            AssetId = AssetId.From(TacticalGraphics.Tom),
            Width = 32
        };
        RoundTripXld<IReadOnlyTexture<byte>>(nameof(TacticalGfxTest), "$(XLD)/TACTICO0.XLD", 0,
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
        RoundTripXld<IReadOnlyTexture<byte>>(nameof(WallTest), "$(XLD)/3DWALLS0.XLD", 11,
            (x, s, c) => Loaders.FixedSizeSpriteLoader.Serdes(x, info, s, c));
    }
// */
}