using System;
using System.Collections.Generic;
using System.IO;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Config.Properties;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Labyrinth;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Containers;
using UAlbion.Formats.Exporters.Tiled;
using UAlbion.Formats.Ids;
using UAlbion.Formats.MapEvents;
using UAlbion.Formats.Parsers;
using UAlbion.Game.Assets;
using UAlbion.Game.Tests;
using UAlbion.TestCommon;
using Xunit;

namespace UAlbion.Base.Tests;

public class RoundtripTests
{
    static readonly IJsonUtil JsonUtil = new FormatJsonUtil();
    static readonly PathResolver PathResolver;
    static readonly AssetMapping Mapping;
    static readonly IFileSystem Disk;
    static readonly string ResultDir;

    static RoundtripTests()
    {
        string baseDir;
        (Disk, baseDir, PathResolver, Mapping) = SetupAssets(JsonUtil);
        ResultDir = Path.Combine(baseDir, "re", "RoundTripTests");
    }

    public RoundtripTests()
    {
        AssetMapping.GlobalIsThreadLocal = true;
        AssetMapping.Global.MergeFrom(Mapping);
    }

    static (MockFileSystem disk, string baseDir, PathResolver generalConfig, AssetMapping mapping) SetupAssets(IJsonUtil jsonUtil)
    {
        Event.AddEventsFromAssembly(typeof(ActionEvent).Assembly);
        var mapping = new AssetMapping();
        var disk = new MockFileSystem(true);
        var baseDir = ConfigUtil.FindBasePath(disk);
        var pathResolver = new PathResolver(baseDir, "ualbion-tests");
        pathResolver.RegisterPath("ALBION", Path.Combine(baseDir, "Albion"));
        var typeConfigPath = Path.Combine(baseDir, "mods", "Base", "types.json");
        var assetConfigPath = Path.Combine(baseDir, "mods", "Albion", "alb_assets.json");

        var tcl = new TypeConfigLoader(jsonUtil);
        var typeConfig = tcl.Load(typeConfigPath, "RoundtripTests", null, mapping, disk);

        foreach (var assetType in typeConfig.IdTypes.Values)
        {
            var enumType = Type.GetType(assetType.EnumType);
            if (enumType == null)
                throw new InvalidOperationException(
                    $"Could not load enum type \"{assetType.EnumType}\" defined in \"{assetConfigPath}\"");

            mapping.RegisterAssetType(assetType.EnumType, assetType.AssetType);
        }

        return (disk, baseDir, pathResolver, mapping);
    }

    static T RoundTrip<T>(string testName, byte[] bytes, Asset.SerdesFunc<T> serdes, AssetId id, AssetNode node, string language = null) where T : class
    {
        node ??= new AssetNode(id);
        var modContext = new ModContext("Test", JsonUtil, Disk, AssetMapping.Global);
        var context = new AssetLoadContext(id, node, modContext, language);
        return RoundTrip(testName, bytes, serdes, context);
    }

    static T RoundTrip<T>(string testName, byte[] bytes, Asset.SerdesFunc<T> serdes, AssetLoadContext context) where T : class
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

    static void RoundTripXld<T>(
        string testName,
        string file,
        AssetId firstId,
        AssetId id,
        Asset.SerdesFunc<T> serdes,
        Action<AssetNode> propertySetter = null) where T : class
    {
        var node = new AssetNode(firstId);
        if (propertySetter != null)
            propertySetter(node);

        var modContext = new ModContext("Test", JsonUtil, Disk, AssetMapping.Global);
        var context = new AssetLoadContext(id, node, modContext);
        var bytes = Asset.BytesFromXld(PathResolver, file, context);
        RoundTrip(testName, bytes, serdes, context);
    }

    static void RoundTripRaw<T>(string testName, string file, Asset.SerdesFunc<T> serdes, string language) where T : class
    {
        var bytes = File.ReadAllBytes(PathResolver.ResolvePath(file));
        RoundTrip(testName, bytes, serdes, AssetId.None, null, language);
    }

    static void RoundTripRaw<T>(string testName, string file, Asset.SerdesFunc<T> serdes, AssetId id, AssetNode node) where T : class
    {
        var bytes = File.ReadAllBytes(PathResolver.ResolvePath(file));
        RoundTrip(testName, bytes, serdes, id, node);
    }

    static void RoundTripItem<T>(string testName, string file, ItemId id, Asset.SerdesFunc<T> serdes) where T : class
    {
        var loader = new ItemListContainer();
        var node = new AssetNode((ItemId)Item.Knife);
        var modContext = new ModContext("Test", JsonUtil, Disk, AssetMapping.Global);
        var context = new AssetLoadContext(id, node, modContext);

        using var s = loader.Read(PathResolver.ResolvePath(file), context);
        var bytes = s.Bytes(null, null, (int)s.BytesRemaining);
        RoundTrip(testName, bytes, serdes, context);
    }

    static void RoundTripSpell<T>(string testName, string file, SpellId id, Asset.SerdesFunc<T> serdes) where T : class
    {
        var loader = new SpellListContainer();
        var node = new AssetNode((SpellId)Spell.ThornSnare);
        var modContext = new ModContext("Test", JsonUtil, Disk, AssetMapping.Global);
        var context = new AssetLoadContext(id, node, modContext);

        using var s = loader.Read(PathResolver.ResolvePath(file), context);
        var bytes = s.Bytes(null, null, (int)s.BytesRemaining);
        RoundTrip(testName, bytes, serdes, context);
    }

    [Fact]
    public void ItemTest()
    {
        var id = (ItemId)Item.DanusLight;
        var spell = new SpellData(Spell.Lifebringer, SpellClass.DjiKantos, 2)
        {
            Cost = 60,
            Environments = SpellEnvironments.Indoors
                         | SpellEnvironments.Outdoors
                         | SpellEnvironments.Dungeon
                         | SpellEnvironments.Inventory,

            LevelRequirement = 13,
            Targets = SpellTargets.DeadParty,
        };

        var spellManager = new MockSpellManager().Add(spell);
        ItemDataLoader itemDataLoader = new();
        new EventExchange()
            .Attach(spellManager)
            .Attach(itemDataLoader);

        RoundTripItem<ItemData>(nameof(ItemTest),
            "$(ALBION)/CD/XLDLIBS/ITEMLIST.DAT", id, (x, s, c) => itemDataLoader.Serdes(x, s, c));
    }

    [Fact]
    public void ItemNameTest()
    {
        RoundTripRaw<Dictionary<string, ListStringSet>>(nameof(ItemNameTest),
            "$(ALBION)/CD/XLDLIBS/ITEMNAME.DAT",
            (x, s, c) => Loaders.ItemNameLoader.Serdes(x, s, c),
            Language.English);
    }

    [Fact]
    public void AutomapTest()
    {
        RoundTripXld<Formats.Assets.Automap>(
            nameof(AutomapTest),
            "$(ALBION)/CD/XLDLIBS/INITIAL/AUTOMAP1.XLD",
            (AutomapId)(Automap)100,
            (AutomapId)Automap.Jirinaar,
            (x, s, c) => Loaders.AutomapLoader.Serdes(x, s, c));
    }

    [Fact]
    public void BlockListTest()
    {
        RoundTripXld<Formats.Assets.BlockList>(nameof(BlockListTest),
            "$(ALBION)/CD/XLDLIBS/BLKLIST0.XLD",
            (BlockListId)(BlockList)1,
            (BlockListId)BlockList.Toronto,
            (x, s, c) => Loaders.BlockListLoader.Serdes(x, s, c));
    }

    [Fact]
    public void ChestTest()
    {
        RoundTripXld<Inventory>(nameof(ChestTest),
            "$(ALBION)/CD/XLDLIBS/INITIAL/CHESTDT1.XLD",
            (ChestId)(Chest)100,
            (ChestId)Chest.HClanCellar_ID_IKn_ILC_StC_LSh_3g,
            (x, s, c) => Loaders.ChestLoader.Serdes(x, s, c));
    }

    [Fact]
    public void CommonPaletteTest()
    {
        var node = new AssetNode((PaletteId)Palette.Common);
        node.SetProperty(PaletteLoader.IsCommon, true);

        RoundTripRaw<AlbionPalette>(nameof(CommonPaletteTest),
            "$(ALBION)/CD/XLDLIBS/PALETTE.000",
            (x, s, c) => Loaders.PaletteLoader.Serdes(x, s, c),
            (PaletteId)Palette.Common,
            node);
    }

    [Fact]
    public void EventSetTest()
    {
        RoundTripXld<Formats.Assets.EventSet>(nameof(EventSetTest),
            "$(ALBION)/CD/XLDLIBS/EVNTSET1.XLD", 
            (EventSetId)(EventSet)100,
            (EventSetId)EventSet.Frill,
            (x, s, c) => Loaders.EventSetLoader.Serdes(x, s, c));
    }

    [Fact]
    public void EventTextTest()
    {
        RoundTripXld<ListStringSet>(nameof(EventTextTest),
            "$(ALBION)/CD/XLDLIBS/ENGLISH/EVNTTXT1.XLD",
            (StringSetId)(EventText)100,
            (StringSetId)EventText.Frill,
            (x, s, c) => Loaders.AlbionStringTableLoader.Serdes(x, s, c));
    }

    [Fact]
    public void LabyrinthTest()
    {
        RoundTripXld<LabyrinthData>(nameof(LabyrinthTest),
            "$(ALBION)/CD/XLDLIBS/LABDATA1.XLD",
            (LabyrinthId)(Labyrinth)100,
            (LabyrinthId)Labyrinth.Jirinaar,
            (x, s, c) => Loaders.LabyrinthDataLoader.Serdes(x, s, c));
    }

    [Fact]
    public void Map2DTest()
    {
        RoundTripXld<MapData2D>(nameof(Map2DTest),
            "$(ALBION)/CD/XLDLIBS/MAPDATA3.XLD", 
            (MapId)(Map)300,
            (MapId)Map.TorontoBegin,
            (x, s, c) => (MapData2D)Loaders.MapLoader.Serdes(x, s, c));
    }

    [Fact]
    public void Map3DTest()
    {
        RoundTripXld<MapData3D>(nameof(Map3DTest),
            "$(ALBION)/CD/XLDLIBS/MAPDATA1.XLD", 
            (MapId)(Map)100,
            (MapId)Map.OldFormerBuilding,
            (x, s, c) => (MapData3D)Loaders.MapLoader.Serdes(x, s, c));
    }

    [Fact]
    public void MapTextTest()
    {
        RoundTripXld<ListStringSet>(nameof(MapTextTest),
            "$(ALBION)/CD/XLDLIBS/ENGLISH/MAPTEXT3.XLD", 
            (StringSetId)(MapText)300,
            (StringSetId)MapText.TorontoBegin,
            (x, s, c) => Loaders.AlbionStringTableLoader.Serdes(x, s, c));
    }

    [Fact]
    public void MerchantTest()
    {
        RoundTripXld<Inventory>(nameof(MerchantTest),
            "$(ALBION)/CD/XLDLIBS/INITIAL/MERCHDT1.XLD", 
            (MerchantId)(Merchant)100,
            (MerchantId)Merchant.AltheaSpells,
            (x, s, c) => Loaders.MerchantLoader.Serdes(x, s, c));
    }

    [Fact]
    public void MonsterGroupTest()
    {
        RoundTripXld<Formats.Assets.MonsterGroup>(nameof(MonsterGroupTest),
            "$(ALBION)/CD/XLDLIBS/MONGRP0.XLD",
            (MonsterGroupId)(MonsterGroup)1,
            (MonsterGroupId)MonsterGroup.TwoSkrinn1OneKrondir1,
            (x, s, c) => Loaders.MonsterGroupLoader.Serdes(x, s, c));
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
        var loader = BuildCharacterLoader();
        RoundTripXld<CharacterSheet>(nameof(MonsterTest),
            "$(ALBION)/CD/XLDLIBS/MONCHAR0.XLD",
            (SheetId)(MonsterSheet)1,
            (SheetId)MonsterSheet.Krondir1,
            (x, s, c) => loader.Serdes(x, s, c));
    }

    [Fact]
    public void NpcTest()
    {
        var loader = BuildCharacterLoader();
        RoundTripXld<CharacterSheet>(nameof(NpcTest),
            "$(ALBION)/CD/XLDLIBS/INITIAL/NPCCHAR1.XLD", 
            (SheetId)(NpcSheet)100,
            (SheetId)NpcSheet.Christine,
            (x, s, c) => loader.Serdes(x, s, c));
    }

    [Fact]
    public void PaletteTest()
    {
        RoundTripXld<AlbionPalette>(nameof(PaletteTest),
            "$(ALBION)/CD/XLDLIBS/PALETTE0.XLD",
            (PaletteId)(Palette)1,
            (PaletteId)Palette.Toronto2D,
            (x, s, c) => Loaders.PaletteLoader.Serdes(x, s, c));
    }

    [Fact]
    public void PartyMemberTest()
    {
        var loader = BuildCharacterLoader();
        RoundTripXld<CharacterSheet>(nameof(PartyMemberTest),
            "$(ALBION)/CD/XLDLIBS/INITIAL/PRTCHAR0.XLD",
            (SheetId)(PartySheet)1,
            (SheetId)PartySheet.Tom,
            (x, s, c) => loader.Serdes(x, s, c));
    }

    [Fact]
    public void SampleTest()
    {
        RoundTripXld<AlbionSample>(nameof(SampleTest),
            "$(ALBION)/CD/XLDLIBS/SAMPLES0.XLD",
            (SampleId)(Sample)1,
            (SampleId)Sample.IllTemperedLlama,
            (x, s, c) => Loaders.SampleLoader.Serdes(x, s, c));
    }

    /* They're text anyway so not too bothered - at the moment they don't round trip due to using friendly asset id names
    // Would need to add a ToStringNumeric or something to the relevant events, starts getting ugly.
    [Fact]
    public void ScriptTest()
    {
        var info = new AssetInfo { AssetId = AssetId.From(Script.TomMeetsChristine) };
        RoundTripXld<IList<IEvent>>(nameof(ScriptTest),
            "$(ALBION)/CD/XLDLIBS/SCRIPT0.XLD", 1,
            (x, s, c) => Loaders.ScriptLoader.Serdes(x, s, c));
    } //*/

    [Fact]
    public void SongTest()
    {
        RoundTripXld<byte[]>(nameof(SongTest),
            "$(ALBION)/CD/XLDLIBS/SONGS0.XLD",
            (SongId)(Song)1,
            (SongId)Song.Toronto,
            (x, s, c) => Loaders.SongLoader.Serdes(x, s, c));
    }

    [Fact]
    public void SpellTest()
    {
        RoundTripSpell<SpellData>(nameof(SpellTest),
            "$(ALBION)/CD/XLDLIBS/SPELLDAT.DAT",
            Spell.FrostAvalanche,
            (x, s, c) => Loaders.SpellLoader.Serdes(x, s, c));
    }

    [Fact]
    public void TilesetTest()
    {
        RoundTripXld<TilesetData>(nameof(TilesetTest),
            "$(ALBION)/CD/XLDLIBS/ICONDAT0.XLD",
            (TilesetId)(Tileset)1,
            (TilesetId)Tileset.Toronto,
            (x, s, c) => Loaders.TilesetLoader.Serdes(x, s, c));
    }

    [Fact]
    public void TiledTilesetTest()
    {
        var tilesetId = (TilesetId)Tileset.Toronto;

        var gfxId = (SpriteId)TilesetGfx.Toronto;
        var gfxNode = new AssetNode((SpriteId)(TilesetGfx)1);
        gfxNode.SetProperty(AssetProps.Palette, (PaletteId)Palette.Toronto2D);

        var modApplier = new MockModApplier();
        modApplier.AddInfo(gfxId, gfxNode);

        var exchange = new EventExchange()
            .Attach(modApplier)
            .Attach(new AssetManager());

        var modContext = new ModContext("Test", JsonUtil, Disk, AssetMapping.Global);
        var context = new AssetLoadContext(tilesetId, new AssetNode((TilesetId)(Tileset)1), modContext);

        var bytes = Asset.BytesFromXld(PathResolver, "$(ALBION)/CD/XLDLIBS/ICONDAT0.XLD", context);

        TilesetData Serdes(TilesetData x, ISerializer s, AssetLoadContext c) => Loaders.TilesetLoader.Serdes(x, s, c);
        var (asset, preTxt) = Asset.Load<TilesetData>(bytes, Serdes, context);

        var loader = new TiledTilesetLoader();
        exchange.Attach(loader);
        var (tiledBytes, tiledTxt) = Asset.Save(asset, (x, s, c) => loader.Serdes(x, s, c), context);

        var (fromTiled, _) = Asset.Load<TilesetData>(tiledBytes,
            (x, s, c) => loader.Serdes(x, s, c), context);

        var (roundTripped, roundTripTxt) = Asset.Save(fromTiled, Serdes, context);
        Asset.Compare(ResultDir,
            nameof(TiledTilesetTest),
            bytes,
            roundTripped,
            new[] { (".pre.txt", preTxt), (".post.txt", tiledTxt), (".reload.txt", roundTripTxt)});
    }

    [Fact]
    public void TiledStampTest()
    {
        var id = (BlockListId)BlockList.Toronto;
        var modContext = new ModContext("Test", JsonUtil, Disk, AssetMapping.Global);
        var context = new AssetLoadContext(id, new AssetNode((BlockListId)(BlockList)1), modContext);
        var bytes = Asset.BytesFromXld(PathResolver, "$(ALBION)/CD/XLDLIBS/BLKLIST0.XLD", context);

        Formats.Assets.BlockList Serdes(Formats.Assets.BlockList x, ISerializer s, AssetLoadContext c2) => Loaders.BlockListLoader.Serdes(x, s, c2);
        var (asset, preTxt) = Asset.Load<Formats.Assets.BlockList>(bytes, Serdes, context);

        var loader = new StampLoader();
        var (tiledBytes, tiledTxt) = Asset.Save(asset, (x, s, c) => loader.Serdes(x, s, c), context);

        var (fromTiled, _) = Asset.Load<Formats.Assets.BlockList>(tiledBytes,
            (x, s, c) => loader.Serdes(x, s, c),
            context);

        var (roundTripped, roundTripTxt) = Asset.Save(fromTiled, Serdes, context);
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
        RoundTripXld<WaveLib>(nameof(WaveLibTest),
            "$(ALBION)/CD/XLDLIBS/WAVELIB0.XLD",
            (WaveLibraryId)(WaveLibrary)1,
            (WaveLibraryId)WaveLibrary.Unknown5,
            (x, s, c) => Loaders.WaveLibLoader.Serdes(x, s, c));
    }

    [Fact]
    public void WordTest()
    {
        RoundTripXld<ListStringSet>(nameof(WordTest),
            "$(ALBION)/CD/XLDLIBS/ENGLISH/WORDLIS0.XLD",
            (SpecialId)Special.Words1,
            (SpecialId)Special.Words1,
            (x, s, c) => Loaders.WordListLoader.Serdes(x, s, c));
    }
//*
    [Fact]
    public void AutomapGfxTest()
    {
        RoundTripXld<IReadOnlyTexture<byte>>(nameof(AutomapGfxTest),
            "$(ALBION)/CD/XLDLIBS/AUTOGFX0.XLD",
            (SpriteId)(AutomapTiles)1,
            (SpriteId)AutomapTiles.Set1,
            (x, s, c) => Loaders.AmorphousSpriteLoader.Serdes(x, s, c),
            node => node.SetProperty(AmorphousSpriteLoader.SubSpritesProperty, "(8,8,576) (16,16)"));
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

        var id = (SpriteId)AutomapTiles.Set1;
        var node = new AssetNode((SpriteId)(AutomapTiles)1);
        node.SetProperty(AmorphousSpriteLoader.SubSpritesProperty, "(1,1,3) (2,2)");

        var sprite = RoundTrip<IReadOnlyTexture<byte>>(
            nameof(AmorphousTest),
            bytes,
            (x, s, c) => Loaders.AmorphousSpriteLoader.Serdes(x, s, c),
            id, node);

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
        RoundTripXld<IReadOnlyTexture<byte>>(nameof(CombatBgTest),
            "$(ALBION)/CD/XLDLIBS/COMBACK0.XLD",
            (SpriteId)(CombatBackground)1,
            (SpriteId)CombatBackground.Toronto,
            (x, s, c) => Loaders.FixedSizeSpriteLoader.Serdes(x, s, c),
            node => node.SetProperty(AssetProps.Width, 360));
    }

    [Fact]
    public void DungeonObjectTest()
    {
        RoundTripXld<IReadOnlyTexture<byte>>(nameof(DungeonObjectTest),
            "$(ALBION)/CD/XLDLIBS/3DOBJEC2.XLD",
            (SpriteId)(DungeonObject)200,
            (SpriteId)DungeonObject.Krondir,
            (x, s, c) => Loaders.FixedSizeSpriteLoader.Serdes(x, s, c),
            node =>
            {
                node.SetProperty(AssetProps.Width, 145);
                node.SetProperty(AssetProps.Height, 165);
            });
    }

    [Fact]
    public void FontGfxTest()
    {
        RoundTripXld<IReadOnlyTexture<byte>>(nameof(FontGfxTest),
            "$(ALBION)/CD/XLDLIBS/FONTS0.XLD",
            (SpriteId)(FontGfx)1,
            (SpriteId)FontGfx.Regular,
            (x, s, c) => Loaders.FixedSizeSpriteLoader.Serdes(x, s, c),
            node =>
            {
                node.SetProperty(AssetProps.Width, 8);
                node.SetProperty(AssetProps.Height, 8);
            });
    }

    [Fact]
    public void ItemSpriteTest()
    {
        var id = (SpriteId)ItemGfx.ItemSprites;
        var node = new AssetNode((SpriteId)(ItemGfx)1);
        node.SetProperty(AssetProps.Width, 16);
        node.SetProperty(AssetProps.Height, 16);

        RoundTripRaw<IReadOnlyTexture<byte>>(nameof(ItemSpriteTest),
            "$(ALBION)/CD/XLDLIBS/ITEMGFX",
            (x, s, c) => Loaders.FixedSizeSpriteLoader.Serdes(x, s, c),
            id, node);
    }

    [Fact]
    public void SlabTest()
    {
        var id = (SpriteId)UiBackground.Slab;
        var node = new AssetNode((SpriteId)UiBackground.Slab);
        node.SetProperty(AssetProps.Width, 360);
        RoundTripRaw<IReadOnlyTexture<byte>>(nameof(SlabTest),
            "$(ALBION)/CD/XLDLIBS/SLAB",
            (x, s, c) => Loaders.SlabLoader.Serdes(x, s, c),
            id, node);
    }

    [Fact]
    public void TileGfxTest()
    {
        RoundTripXld<IReadOnlyTexture<byte>>(nameof(TileGfxTest),
            "$(ALBION)/CD/XLDLIBS/ICONGFX0.XLD",
            (TilesetGfxId)(TilesetGfx)1,
            (TilesetGfxId)TilesetGfx.Toronto,
            (x, s, c) => Loaders.FixedSizeSpriteLoader.Serdes(x, s, c),
            node =>
            {
                node.SetProperty(AssetProps.Width, 16);
                node.SetProperty(AssetProps.Height, 16);
            });
    }

    [Fact]
    public void CombatGfxTest()
    {
        RoundTripXld<IReadOnlyTexture<byte>>(nameof(CombatGfxTest),
            "$(ALBION)/CD/XLDLIBS/COMGFX0.XLD",
            (SpriteId)(CombatGfx)1,
            (SpriteId)CombatGfx.SplashYellow,
            (x, s, c) => Loaders.MultiHeaderSpriteLoader.Serdes(x, s, c));
    }

    [Fact]
    public void DungeonBgTest()
    {
        RoundTripXld<IReadOnlyTexture<byte>>(nameof(DungeonBgTest),
            "$(ALBION)/CD/XLDLIBS/3DBCKGR0.XLD",
            (SpriteId)(DungeonBackground)1,
            (SpriteId)DungeonBackground.EarlyGameL,
            (x, s, c) => Loaders.SingleHeaderSpriteLoader.Serdes(x, s, c));
    }

    [Fact]
    public void FloorTest()
    {
        RoundTripXld<IReadOnlyTexture<byte>>(nameof(FloorTest),
            "$(ALBION)/CD/XLDLIBS/3DFLOOR0.XLD",
            (SpriteId)(Floor)1,
            (SpriteId)Floor.Water,
            (x, s, c) => Loaders.FixedSizeSpriteLoader.Serdes(x, s, c),
            node =>
            {
                node.SetProperty(AssetProps.Width, 64);
                node.SetProperty(AssetProps.Height, 64);
            });
    }

    [Fact]
    public void FullBodyPictureTest()
    {
        RoundTripXld<IReadOnlyTexture<byte>>(nameof(FullBodyPictureTest),
            "$(ALBION)/CD/XLDLIBS/FBODPIX0.XLD",
            (SpriteId)(PartyInventoryGfx)1,
            (SpriteId)PartyInventoryGfx.Tom,
            (x, s, c) => Loaders.SingleHeaderSpriteLoader.Serdes(x, s, c));
    }

    [Fact]
    public void LargeNpcTest()
    {
        RoundTripXld<IReadOnlyTexture<byte>>(nameof(LargeNpcTest),
            "$(ALBION)/CD/XLDLIBS/NPCGR0.XLD",
            (SpriteId)(NpcLargeGfx)1,
            (SpriteId)NpcLargeGfx.Christine,
            (x, s, c) => Loaders.SingleHeaderSpriteLoader.Serdes(x, s, c));
    }

    [Fact]
    public void LargePartyMemberTest()
    {
        RoundTripXld<IReadOnlyTexture<byte>>(nameof(LargePartyMemberTest),
            "$(ALBION)/CD/XLDLIBS/PARTGR0.XLD",
            (SpriteId)(PartyLargeGfx)1,
            (SpriteId)PartyLargeGfx.Tom,
            (x, s, c) => Loaders.SingleHeaderSpriteLoader.Serdes(x, s, c));
    }

    [Fact]
    public void MonsterGfxTest()
    {
        RoundTripXld<IReadOnlyTexture<byte>>(nameof(MonsterGfxTest),
            "$(ALBION)/CD/XLDLIBS/MONGFX0.XLD",
            (SpriteId)(MonsterGfx)1,
            (SpriteId)MonsterGfx.Krondir,
            (x, s, c) => Loaders.MultiHeaderSpriteLoader.Serdes(x, s, c));
    }

    [Fact]
    public void OverlayTest()
    {
        RoundTripXld<IReadOnlyTexture<byte>>(nameof(OverlayTest),
            "$(ALBION)/CD/XLDLIBS/3DOVERL0.XLD",
            (SpriteId)(WallOverlay)1,
            (SpriteId)WallOverlay.JiriFrameL,
            (x, s, c) => Loaders.FixedSizeSpriteLoader.Serdes(x, s, c),
            node =>
            {
                node.SetProperty(AssetProps.Width, 112);
                node.SetProperty(FixedSizeSpriteLoader.TransposedProperty, true);
            });
    }

    [Fact]
    public void OverlayMultiFrameTest()
    {
        RoundTripXld<IReadOnlyTexture<byte>>(nameof(OverlayTest),
            "$(ALBION)/CD/XLDLIBS/3DOVERL2.XLD",
            (SpriteId)(WallOverlay)200,
            (SpriteId)WallOverlay.Unknown201,
            (x, s, c) => Loaders.FixedSizeSpriteLoader.Serdes(x, s, c),
            node =>
            {
                node.SetProperty(AssetProps.Width, 6);
                node.SetProperty(AssetProps.Height, 20);
                node.SetProperty(FixedSizeSpriteLoader.TransposedProperty, true);
            });
    }

    // 201

    /* No code to write these atm, if anyone wants to mod them or add new ones they can still use ImageMagick or something to convert to ILBM
    [Fact]
    public void PictureTest()
    {
        RoundTripXld<InterlacedBitmap>(nameof(PictureTest),
            "$(ALBION)/CD/XLDLIBS/PICTURE0.XLD",
            (PictureId)(Picture)1,
            (PictureId)Picture.OpenChestWithGold,
            (x, s, c) => Loaders.InterlacedBitmapLoader.Serdes(x, s, c));
    } //*/

    [Fact]
    public void PortraitTest()
    {
        RoundTripXld<IReadOnlyTexture<byte>>(nameof(PortraitTest),
            "$(ALBION)/CD/XLDLIBS/SMLPORT0.XLD",
            (PortraitId)(Portrait)1,
            (PortraitId)Portrait.Tom,
            (x, s, c) => Loaders.FixedSizeSpriteLoader.Serdes(x, s, c),
            node => node.SetProperty(AssetProps.Width, 34));
    }

    [Fact]
    public void SmallNpcTest()
    {
        RoundTripXld<IReadOnlyTexture<byte>>(nameof(SmallNpcTest),
            "$(ALBION)/CD/XLDLIBS/NPCKL0.XLD",
            (SpriteId)(NpcSmallGfx)1,
            (SpriteId)NpcSmallGfx.Krondir,
            (x, s, c) => Loaders.SingleHeaderSpriteLoader.Serdes(x, s, c));
    }

    [Fact]
    public void SmallPartyMemberTest()
    {
        RoundTripXld<IReadOnlyTexture<byte>>(nameof(SmallPartyMemberTest),
            "$(ALBION)/CD/XLDLIBS/PARTKL0.XLD",
            (SpriteId)(PartySmallGfx)1,
            (SpriteId)PartySmallGfx.Tom,
            (x, s, c) => Loaders.SingleHeaderSpriteLoader.Serdes(x, s, c));
    }

    [Fact]
    public void TacticalGfxTest()
    {
        RoundTripXld<IReadOnlyTexture<byte>>(nameof(TacticalGfxTest),
            "$(ALBION)/CD/XLDLIBS/TACTICO0.XLD",
            (SpriteId)(TacticalGfx)1,
            (SpriteId)TacticalGfx.Tom,
            (x, s, c) => Loaders.FixedSizeSpriteLoader.Serdes(x, s, c),
            node => node.SetProperty(AssetProps.Width, 32));
    }

    [Fact]
    public void WallTest()
    {
        RoundTripXld<IReadOnlyTexture<byte>>(nameof(WallTest),
            "$(ALBION)/CD/XLDLIBS/3DWALLS0.XLD",
            (SpriteId)(Wall)1,
            (SpriteId)Wall.TorontoPanelling,
            (x, s, c) => Loaders.FixedSizeSpriteLoader.Serdes(x, s, c),
            node => node.SetProperty(AssetProps.Width, 80));
    }
// */
}
