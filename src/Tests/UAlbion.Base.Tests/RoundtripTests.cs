using System;
using System.IO;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Containers;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Assets;
using UAlbion.Game.Tests;
using UAlbion.TestCommon;
using Xunit;

namespace UAlbion.Base.Tests
{
    public class RoundtripTests
    {
        readonly IFileSystem _disk;
        readonly string _baseDir;
        readonly string _resultDir;

        public RoundtripTests()
        {
            Event.AddEventsFromAssembly(typeof(ActionEvent).Assembly);
            AssetMapping.GlobalIsThreadLocal = true;
            _disk = new MockFileSystem(true);
            _baseDir = ConfigUtil.FindBasePath(_disk);
            _resultDir = Path.Combine(_baseDir, "re", "RoundTripTests");
            var mapping = AssetMapping.Global;
            var assetConfigPath = Path.Combine(_baseDir, "mods", "Base", "assets.json");
            var assetConfig = AssetConfig.Load(assetConfigPath, _disk);

            foreach (var assetType in assetConfig.IdTypes.Values)
            {
                var enumType = Type.GetType(assetType.EnumType);
                if (enumType == null)
                    throw new InvalidOperationException(
                            $"Could not load enum type \"{assetType.EnumType}\" defined in \"{assetConfigPath}\"");

                mapping.RegisterAssetType(assetType.EnumType, assetType.AssetType);
            }
        }

        static readonly XldContainer XldLoader = new();

        byte[] BytesFromXld(IGeneralConfig conf, string path, AssetInfo info)
        {
            using var s = XldLoader.Read(conf.ResolvePath(path), info, _disk);
            return s.Bytes(null, null, (int)s.BytesRemaining);
        }

        T RoundTrip<T>(string testName, byte[] bytes, Func<T, ISerializer, T> serdes) where T : class
        {
            var (asset, preTxt) = Asset.Load(bytes, serdes);
            var (postBytes, postTxt) = Asset.Save(asset, serdes);
            var (_, reloadTxt) = Asset.Load(postBytes, serdes);
            Asset.Compare(_resultDir,
                testName,
                bytes,
                postBytes,
                new[] { (".pre.txt", preTxt), (".post.txt", postTxt), (".reload.txt", reloadTxt)});

            if (asset is IReadOnlyTexture<byte>) // TODO: Png round-trip?
                return asset;

            var json = Asset.SaveJson(asset);
            var fromJson = Asset.LoadJson<T>(json);
            var (fromJsonBytes, fromJsonTxt) = Asset.Save(fromJson, serdes);
            Asset.Compare(_resultDir,
                testName + ".json",
                bytes,
                fromJsonBytes,
                new[] { (".pre.txt", preTxt), (".post.txt", json), (".reload.txt", fromJsonTxt)});

            return asset;
        }

        void RoundTripXld<T>(string testName, string file, int subId, Func<T, ISerializer, T> serdes) where T : class
        {
            var conf = AssetSystem.LoadGeneralConfig(_baseDir, _disk);
            var info = new AssetInfo { Index = subId };
            var bytes = BytesFromXld(conf, file, info);
            RoundTrip(testName, bytes, serdes);
        }

        void RoundTripRaw<T>(string testName, string file, Func<T, ISerializer, T> serdes) where T : class
        {
            var conf = AssetSystem.LoadGeneralConfig(_baseDir, _disk);
            var bytes = File.ReadAllBytes(conf.ResolvePath(file));
            RoundTrip(testName, bytes, serdes);
        }

        void RoundTripItem<T>(string testName, string file, int subId, Func<T, ISerializer, T> serdes) where T : class
        {
            var conf = AssetSystem.LoadGeneralConfig(_baseDir, _disk);
            var info = new AssetInfo { Index = subId };
            var loader = new ItemListContainer();
            using var s = loader.Read(conf.ResolvePath(file), info, _disk);
            var bytes = s.Bytes(null, null, (int)s.BytesRemaining);
            RoundTrip(testName, bytes, serdes);
        }

        void RoundTripSpell<T>(string testName, string file, int subId, Func<T, ISerializer, T> serdes) where T : class
        {
            var conf = AssetSystem.LoadGeneralConfig(_baseDir, _disk);
            var info = new AssetInfo { Index = subId };
            var loader = new SpellListContainer();
            using var s = loader.Read(conf.ResolvePath(file), info, _disk);
            var bytes = s.Bytes(null, null, (int)s.BytesRemaining);
            RoundTrip(testName, bytes, serdes);
        }

        [Fact]
        public void ItemTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Item.Knife) };
            RoundTripItem<ItemData>(nameof(ItemTest), "$(XLD)/ITEMLIST.DAT", 10,
                (x, s) => Loaders.ItemDataLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void ItemNameTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Special.ItemNames) };
            RoundTripRaw<MultiLanguageStringDictionary>(nameof(ItemNameTest), "$(XLD)/ITEMNAME.DAT",
                (x, s) => Loaders.ItemNameLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void AutomapTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Automap.Jirinaar) };
            RoundTripXld<Formats.Assets.Automap>(nameof(AutomapTest), "$(XLD)/INITIAL/AUTOMAP1.XLD", 10,
                (x, s) => Loaders.AutomapLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void BlockListTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(BlockList.Toronto) };
            RoundTripXld<Formats.Assets.BlockList>(nameof(BlockListTest), "$(XLD)/BLKLIST0.XLD", 7,
                (x, s) => Loaders.BlockListLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void ChestTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Chest.Unknown121) };
            RoundTripXld<Inventory>(nameof(ChestTest), "$(XLD)/INITIAL/CHESTDT1.XLD", 21,
                (x, s) => Loaders.ChestLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void CommonPaletteTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Palette.Common) };
            info.Set(AssetProperty.IsCommon, true);
            RoundTripRaw<AlbionPalette>(nameof(CommonPaletteTest), "$(XLD)/PALETTE.000",
                (x, s) => Loaders.PaletteLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void EventSetTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(EventSet.Frill) };
            RoundTripXld<Formats.Assets.EventSet>(nameof(EventSetTest), "$(XLD)/EVNTSET1.XLD", 11,
                (x, s) => Loaders.EventSetLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void EventTextTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(EventText.Frill) };
            RoundTripXld<ListStringCollection>(nameof(EventTextTest), "$(XLD)/ENGLISH/EVNTTXT1.XLD", 11,
                (x, s) => Loaders.AlbionStringTableLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void LabyrinthTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Labyrinth.Jirinaar) };
            RoundTripXld<Formats.Assets.Labyrinth.LabyrinthData>(nameof(LabyrinthTest), "$(XLD)/LABDATA1.XLD", 9,
                (x, s) => Loaders.LabyrinthDataLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void Map2DTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Map.TorontoBegin) };
            RoundTripXld<MapData2D>(nameof(Map2DTest), "$(XLD)/MAPDATA3.XLD", 0,
                (x, s) => MapData2D.Serdes(info, x, AssetMapping.Global, s));
        }

        [Fact]
        public void Map3DTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Map.OldFormerBuilding) };
            RoundTripXld<MapData3D>(nameof(Map3DTest), "$(XLD)/MAPDATA1.XLD", 22,
                (x, s) => MapData3D.Serdes(info, x, AssetMapping.Global, s));
        }

        [Fact]
        public void MapTextTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(MapText.TorontoBegin) };
            RoundTripXld<ListStringCollection>(nameof(MapTextTest), "$(XLD)/ENGLISH/MAPTEXT3.XLD", 0,
                (x, s) => Loaders.AlbionStringTableLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void MerchantTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Merchant.Unknown109) };
            RoundTripXld<Inventory>(nameof(MerchantTest), "$(XLD)/INITIAL/MERCHDT1.XLD", 9,
                (x, s) => Loaders.MerchantLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void MonsterGroupTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(MonsterGroup.TwoSkrinn1OneKrondir1) };
            RoundTripXld<Formats.Assets.MonsterGroup>(nameof(MonsterGroupTest), "$(XLD)/MONGRP0.XLD", 9,
                (x, s) => Loaders.MonsterGroupLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void MonsterTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Monster.Krondir1) };
            RoundTripXld<CharacterSheet>(nameof(MonsterTest), "$(XLD)/MONCHAR0.XLD", 9,
                (x, s) => Loaders.CharacterSheetLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void NpcTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Npc.Christine) };
            RoundTripXld<CharacterSheet>(nameof(NpcTest), "$(XLD)/INITIAL/NPCCHAR1.XLD", 83,
                (x, s) => Loaders.CharacterSheetLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void PaletteTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Palette.Toronto2D) };
            RoundTripXld<AlbionPalette>(nameof(PaletteTest), "$(XLD)/PALETTE0.XLD", 25,
                (x, s) => Loaders.PaletteLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void PartyMemberTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(PartyMember.Tom) };
            RoundTripXld<CharacterSheet>(nameof(PartyMemberTest), "$(XLD)/INITIAL/PRTCHAR0.XLD", 0,
                (x, s) => Loaders.CharacterSheetLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void SampleTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Sample.IllTemperedLlama) };
            RoundTripXld<AlbionSample>(nameof(SampleTest), "$(XLD)/SAMPLES0.XLD", 47,
                (x, s) => Loaders.SampleLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        /* They're text anyway so not too bothered - at the moment they don't round trip due to using friendly asset id names
        // Would need to add a ToStringNumeric or something to the relevant events, starts getting ugly.
        [Fact]
        public void ScriptTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Script.TomMeetsChristine) };
            RoundTripXld<IList<IEvent>>(nameof(ScriptTest), "$(XLD)/SCRIPT0.XLD", 1,
                (x, s) => Loaders.ScriptLoader.Serdes(x, info, AssetMapping.Global, s));
        } //*/

        [Fact]
        public void SongTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Song.Toronto) };
            RoundTripXld<byte[]>(nameof(SongTest), "$(XLD)/SONGS0.XLD", 3,
                (x, s) => Loaders.SongLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void SpellTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Spell.FrostAvalanche) };
            RoundTripSpell<SpellData>(nameof(SpellTest), "$(XLD)/SPELLDAT.DAT", 7,
                (x, s) => Loaders.SpellLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void TilesetTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Tileset.Toronto) };
            RoundTripXld<TilesetData>(nameof(TilesetTest), "$(XLD)/ICONDAT0.XLD", 7,
                (x, s) => Loaders.TilesetLoader.Serdes(x, info, AssetMapping.Global, s));
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

            var conf = AssetSystem.LoadGeneralConfig(_baseDir, _disk);
            var bytes = BytesFromXld(conf, "$(XLD)/ICONDAT0.XLD", info);

            TilesetData Serdes(TilesetData x, ISerializer s) => Loaders.TilesetLoader.Serdes(x, info, AssetMapping.Global, s);
            var (asset, preTxt) = Asset.Load<TilesetData>(bytes, Serdes);

            var loader = new TiledTilesetLoader();
            exchange.Attach(loader);
            var (tiledBytes, tiledTxt) = Asset.Save(asset, (x, s) => loader.Serdes(x, info, AssetMapping.Global, s));

            var (fromTiled, _) = Asset.Load<TilesetData>(tiledBytes,
                (x, s) => loader.Serdes(x, info, AssetMapping.Global, s));

            var (roundTripped, roundTripTxt) = Asset.Save(fromTiled, Serdes);
            Asset.Compare(_resultDir,
                nameof(TiledTilesetTest),
                bytes,
                roundTripped,
                new[] { (".pre.txt", preTxt), (".post.txt", tiledTxt), (".reload.txt", roundTripTxt)});
        }

        [Fact]
        public void TiledStampTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(BlockList.Toronto), Index = 7 };
            var conf = AssetSystem.LoadGeneralConfig(_baseDir, _disk);
            var bytes = BytesFromXld(conf, "$(XLD)/BLKLIST0.XLD", info);

            Formats.Assets.BlockList Serdes(Formats.Assets.BlockList x, ISerializer s) => Loaders.BlockListLoader.Serdes(x, info, AssetMapping.Global, s);
            var (asset, preTxt) = Asset.Load<Formats.Assets.BlockList>(bytes, Serdes);

            var loader = new Formats.Exporters.Tiled.StampLoader();
            var (tiledBytes, tiledTxt) = Asset.Save(asset, (x, s) => loader.Serdes(x, info, AssetMapping.Global, s));

            var (fromTiled, _) = Asset.Load<Formats.Assets.BlockList>(tiledBytes,
                (x, s) => loader.Serdes(x, info, AssetMapping.Global, s));

            var (roundTripped, roundTripTxt) = Asset.Save(fromTiled, Serdes);
            Asset.Compare(_resultDir,
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
                (x, s) => Loaders.WaveLibLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void WordTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Special.Words1) };
            RoundTripXld<ListStringCollection>(nameof(WordTest), "$(XLD)/ENGLISH/WORDLIS0.XLD", 0,
                (x, s) => Loaders.WordListLoader.Serdes(x, info, AssetMapping.Global, s));
        }
//*
        [Fact]
        public void AutomapGfxTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(AutomapTiles.Set1) };
            info.Set(AssetProperty.SubSprites, "(8,8,576) (16,16)");
            RoundTripXld<IReadOnlyTexture<byte>>(nameof(AutomapGfxTest), "$(XLD)/AUTOGFX0.XLD", 0,
                (x, s) => Loaders.AmorphousSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
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
                (x, s) =>
                {
                    var albionSprite = Loaders.AmorphousSpriteLoader.Serdes(x, info, AssetMapping.Global, s);
                    var pp = new AlbionSpritePostProcessor();
                    return (IReadOnlyTexture<byte>)pp.Process(albionSprite, info);
                });

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
                (x, s) => Loaders.FixedSizeSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
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
                (x, s) => Loaders.FixedSizeSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void FontTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Font.RegularFont), Width = 8, Height = 8 };
            RoundTripXld<IReadOnlyTexture<byte>>(nameof(FontTest), "$(XLD)/FONTS0.XLD", 0,
                (x, s) => Loaders.FontSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
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
                (x, s) => Loaders.FixedSizeSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void SlabTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(UiBackground.Slab), Width = 360 };
            RoundTripRaw<IReadOnlyTexture<byte>>(nameof(SlabTest), "$(XLD)/SLAB",
                (x, s) => Loaders.SlabLoader.Serdes(x, info, AssetMapping.Global, s));
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
                (x, s) => Loaders.FixedSizeSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void CombatGfxTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(CombatGraphics.Unknown27) };
            RoundTripXld<IReadOnlyTexture<byte>>(nameof(CombatGfxTest), "$(XLD)/COMGFX0.XLD", 26,
                (x, s) => Loaders.MultiHeaderSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void DungeonBgTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(DungeonBackground.EarlyGameL) };
            RoundTripXld<IReadOnlyTexture<byte>>(nameof(DungeonBgTest), "$(XLD)/3DBCKGR0.XLD", 0,
                (x, s) => Loaders.SingleHeaderSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
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
                (x, s) => Loaders.FixedSizeSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void FullBodyPictureTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(FullBodyPicture.Tom) };
            RoundTripXld<IReadOnlyTexture<byte>>(nameof(FullBodyPictureTest), "$(XLD)/FBODPIX0.XLD", 0,
                (x, s) => Loaders.SingleHeaderSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void LargeNpcTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(LargeNpc.Christine) };
            RoundTripXld<IReadOnlyTexture<byte>>(nameof(LargeNpcTest), "$(XLD)/NPCGR0.XLD", 20,
                (x, s) => Loaders.SingleHeaderSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void LargePartyMemberTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(LargePartyMember.Tom) };
            RoundTripXld<IReadOnlyTexture<byte>>(nameof(LargePartyMemberTest), "$(XLD)/PARTGR0.XLD", 0,
                (x, s) => Loaders.SingleHeaderSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void MonsterGfxTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(MonsterGraphics.Krondir) };
            RoundTripXld<IReadOnlyTexture<byte>>(nameof(MonsterGfxTest), "$(XLD)/MONGFX0.XLD", 9,
                (x, s) => Loaders.MultiHeaderSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
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
                (x, s) => Loaders.FixedSizeSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
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
                (x, s) => Loaders.FixedSizeSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        // 201

        /* No code to write these atm, if anyone wants to mod them or add new ones they can still use ImageMagick or something to convert to ILBM
        [Fact]
        public void PictureTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Picture.OpenChestWithGold) };
            RoundTripXld<InterlacedBitmap>(nameof(PictureTest), "$(XLD)/PICTURE0.XLD", 11,
                (x, s) => Loaders.InterlacedBitmapLoader.Serdes(x, info, AssetMapping.Global, s));
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
                (x, s) => Loaders.FixedSizeSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void SmallNpcTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(SmallNpc.Krondir) };
            RoundTripXld<IReadOnlyTexture<byte>>(nameof(SmallNpcTest), "$(XLD)/NPCKL0.XLD", 22,
                (x, s) => Loaders.SingleHeaderSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void SmallPartyMemberTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(SmallPartyMember.Tom) };
            RoundTripXld<IReadOnlyTexture<byte>>(nameof(SmallPartyMemberTest), "$(XLD)/PARTKL0.XLD", 0,
                (x, s) => Loaders.SingleHeaderSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void TacticalGfxTest()
        {
            var info = new AssetInfo
            {
                AssetId = AssetId.From(TacticalGraphics.Unknown1),
                Width = 32
            };
            RoundTripXld<IReadOnlyTexture<byte>>(nameof(TacticalGfxTest), "$(XLD)/TACTICO0.XLD", 0,
                (x, s) => Loaders.FixedSizeSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
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
                (x, s) => Loaders.FixedSizeSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
        }
// */
    }
}
