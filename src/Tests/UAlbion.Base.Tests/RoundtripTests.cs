using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Containers;
using UAlbion.Formats.Parsers;
using Xunit;

namespace UAlbion.Base.Tests
{
    public class RoundtripTests
    {
        public RoundtripTests()
        {
            AssetMapping.GlobalIsThreadLocal = true;
            var mapping = AssetMapping.Global;
            var assetConfigPath = Path.Combine(BaseDir, "mods", "Base", "assets.json");
            var assetConfig = AssetConfig.Load(assetConfigPath);

            foreach (var assetType in assetConfig.IdTypes.Values)
            {
                var enumType = Type.GetType(assetType.EnumType);
                if (enumType == null)
                    throw new InvalidOperationException(
                            $"Could not load enum type \"{assetType.EnumType}\" defined in \"{assetConfigPath}\"");

                mapping.RegisterAssetType(assetType.EnumType, assetType.AssetType);
            }
        }

        static readonly XldContainerLoader XldLoader = new XldContainerLoader();
        static readonly string BaseDir = ConfigUtil.FindBasePath();
        static readonly JsonSerializer JsonSerializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
            ContractResolver = new PrivatePropertyJsonContractResolver()
        });

        static string ReadToEnd(Stream stream)
        {
            stream.Position = 0;
            using var reader = new StreamReader(stream, null, true, -1, true);
            return reader.ReadToEnd();
        }

        static (T, string) Load<T>(byte[] bytes, Func<T, ISerializer, T> serdes) where T : class
        {
            using var stream = new MemoryStream(bytes);
            using var br = new BinaryReader(stream);
            using var annotationStream = new MemoryStream();
            using var annotationWriter = new StreamWriter(annotationStream);
            using var ar = new AnnotationFacadeSerializer(new AlbionReader(br, stream.Length), annotationWriter, FormatUtil.BytesFrom850String);
            var result = serdes(null, ar);
            annotationWriter.Flush();
            var annotation = ReadToEnd(annotationStream);

            if (ar.BytesRemaining > 0)
                throw new InvalidOperationException($"{ar.BytesRemaining} bytes left over after reading");

            return (result, annotation);
        }

        static (byte[], string) Save<T>(T asset, Func<T, ISerializer, T> serdes) where T : class
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);
            using var annotationStream = new MemoryStream();
            using var annotationWriter = new StreamWriter(annotationStream);
            using var aw = new AnnotationFacadeSerializer(new AlbionWriter(bw), annotationWriter, FormatUtil.BytesFrom850String);
            serdes(asset, aw);
            ms.Position = 0;
            var bytes = ms.ToArray();
            annotationWriter.Flush();
            var annotation = ReadToEnd(annotationStream);
            return (bytes, annotation);
        }

        static string SaveJson(object asset)
        {
            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream);
            JsonSerializer.Serialize(writer, asset);
            writer.Flush();
            return ReadToEnd(stream);
        }

        static T LoadJson<T>(string json)
        {
            using var jsonReader = new JsonTextReader(new StringReader(json));
            return (T)JsonSerializer.Deserialize<T>(jsonReader);
        }

        static byte[] BytesFromXld(IGeneralConfig conf, string path, AssetInfo info)
        {
            using var s = XldLoader.Open(conf.ResolvePath(path), info);
            return s.ByteArray(null, null, (int)s.BytesRemaining);
        }

        static void Compare(string testName, byte[] originalBytes, byte[] roundTripBytes, string preText, string postText, string reloadText)
        {
            ApiUtil.Assert(originalBytes.Length == roundTripBytes.Length, $"Asset size changed after round trip (delta {roundTripBytes.Length - originalBytes.Length})");
            ApiUtil.Assert(originalBytes.SequenceEqual(roundTripBytes));

            var diffs = XDelta.Compare(originalBytes, roundTripBytes).ToArray();

            if (originalBytes.Length != roundTripBytes.Length || diffs.Length > 1)
            {
                var resultDir = Path.Combine(BaseDir, "re", "RoundTripTests");
                if (!Directory.Exists(resultDir))
                    Directory.CreateDirectory(resultDir);

                var path = Path.Combine(resultDir, testName);
                File.WriteAllText(path + ".pre.txt", preText);
                File.WriteAllText(path + ".post.txt", postText);
                File.WriteAllText(path + ".reload.txt", reloadText);
            }

            Assert.Collection(diffs,
                    d =>
                    {
                        Assert.True(d.IsCopy);
                        Assert.Equal(0, d.Offset);
                        Assert.Equal(originalBytes.Length, d.Length);
                    });
        }

        static void RoundTrip<T>(string testName, byte[] bytes, Func<T, ISerializer, T> serdes) where T : class
        {
            var (asset, preTxt) = Load(bytes, serdes);
            var (postBytes, postTxt) = Save(asset, serdes);
            var (_, reloadTxt) = Load(postBytes, serdes);
            Compare(testName, bytes, postBytes, preTxt, postTxt, reloadTxt);

            var json = SaveJson(asset);
            var fromJson = LoadJson<T>(json);
            var (fromJsonBytes, fromJsonTxt) = Save(fromJson, serdes);
            Compare(testName + ".json", bytes, fromJsonBytes, preTxt, json, fromJsonTxt);
        }

        static void RoundTripXld<T>(string testName, string file, int subId, Func<T, ISerializer, T> serdes) where T : class
        {
            var conf = AssetSystem.LoadGeneralConfig(BaseDir);
            var info = new AssetInfo { SubAssetId = subId };
            var bytes = BytesFromXld(conf, file, info);
            RoundTrip(testName, bytes, serdes);
        }

        static void RoundTripRaw<T>(string testName, string file, Func<T, ISerializer, T> serdes) where T : class
        {
            var conf = AssetSystem.LoadGeneralConfig(BaseDir);
            var bytes = File.ReadAllBytes(conf.ResolvePath(file));
            RoundTrip(testName, bytes, serdes);
        }

        static void RoundTripItem<T>(string testName, string file, int subId, Func<T, ISerializer, T> serdes) where T : class
        {
            var conf = AssetSystem.LoadGeneralConfig(BaseDir);
            var info = new AssetInfo { SubAssetId = subId };
            var loader = new ItemListContainerLoader();
            using var s = loader.Open(conf.ResolvePath(file), info);
            var bytes = s.ByteArray(null, null, (int)s.BytesRemaining);
            RoundTrip(testName, bytes, serdes);
        }

        static void RoundTripSpell<T>(string testName, string file, int subId, Func<T, ISerializer, T> serdes) where T : class
        {
            var conf = AssetSystem.LoadGeneralConfig(BaseDir);
            var info = new AssetInfo { SubAssetId = subId };
            var loader = new SpellListContainerLoader();
            using var s = loader.Open(conf.ResolvePath(file), info);
            var bytes = s.ByteArray(null, null, (int)s.BytesRemaining);
            RoundTrip(testName, bytes, serdes);
        }

        [Fact]
        public void ItemTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Item.Knife) };
            RoundTripItem<ItemData>(nameof(ItemTest), "$(XLD)/ITEMLIST.DAT", 10,
                (x, s) => ItemDataLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void ItemNameTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Special.ItemNames) };
            RoundTripRaw<IDictionary<GameLanguage, StringCollection>>(nameof(ItemNameTest), "$(XLD)/ITEMNAME.DAT",
                (x, s) => ItemNameLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void AutomapTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Automap.Jirinaar) };
            RoundTripXld<Formats.Assets.Automap>(nameof(AutomapTest), "$(XLD)/INITIAL/AUTOMAP1.XLD", 10,
                (x, s) => AutomapLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void BlockListTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(BlockList.Toronto) };
            RoundTripXld<Formats.Assets.BlockList>(nameof(BlockListTest), "$(XLD)/BLKLIST0.XLD", 7,
                (x, s) => BlockListLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void ChestTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Chest.Unknown121) };
            RoundTripXld<Inventory>(nameof(ChestTest), "$(XLD)/INITIAL/CHESTDT1.XLD", 21,
                (x, s) => ChestLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void CommonPaletteTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Palette.CommonPalette) };
            info.Set("IsCommon", true);
            RoundTripRaw<AlbionPalette>(nameof(CommonPaletteTest), "$(XLD)/PALETTE.000",
                (x, s) => PaletteLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void EventSetTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(EventSet.Frill) };
            RoundTripXld<Formats.Assets.EventSet>(nameof(EventSetTest), "$(XLD)/EVNTSET1.XLD", 11,
                (x, s) => EventSetLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void EventTextTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(EventText.Frill) };
            RoundTripXld<StringCollection>(nameof(EventTextTest), "$(XLD)/ENGLISH/EVNTTXT1.XLD", 11,
                (x, s) => AlbionStringTableLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void LabyrinthTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(LabyrinthData.Jirinaar) };
            RoundTripXld<Formats.Assets.Labyrinth.LabyrinthData>(nameof(LabyrinthTest), "$(XLD)/LABDATA1.XLD", 9,
                (x, s) => LabyrinthDataLoader.Serdes(x, info, AssetMapping.Global, s));
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
            RoundTripXld<StringCollection>(nameof(MapTextTest), "$(XLD)/ENGLISH/MAPTEXT3.XLD", 0,
                (x, s) => AlbionStringTableLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void MerchantTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Merchant.Unknown109) };
            RoundTripXld<Inventory>(nameof(MerchantTest), "$(XLD)/INITIAL/MERCHDT1.XLD", 9,
                (x, s) => MerchantLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void MonsterGroupTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(MonsterGroup.TwoSkrinn1OneKrondir1) };
            RoundTripXld<Formats.Assets.MonsterGroup>(nameof(MonsterGroupTest), "$(XLD)/MONGRP0.XLD", 9,
                (x, s) => MonsterGroupLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void MonsterTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Monster.Krondir1) };
            RoundTripXld<CharacterSheet>(nameof(MonsterTest), "$(XLD)/MONCHAR0.XLD", 3,
                (x, s) => CharacterSheetLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void NpcTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Npc.Christine) };
            RoundTripXld<CharacterSheet>(nameof(NpcTest), "$(XLD)/INITIAL/NPCCHAR1.XLD", 83,
                (x, s) => CharacterSheetLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void PaletteTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Palette.Toronto2D) };
            RoundTripXld<AlbionPalette>(nameof(PaletteTest), "$(XLD)/PALETTE0.XLD", 25,
                (x, s) => PaletteLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void PartyMemberTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(PartyMember.Tom) };
            RoundTripXld<CharacterSheet>(nameof(PartyMemberTest), "$(XLD)/INITIAL/PRTCHAR0.XLD", 0,
                (x, s) => CharacterSheetLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void SampleTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Sample.IllTemperedLlama) };
            RoundTripXld<AlbionSample>(nameof(SampleTest), "$(XLD)/SAMPLES0.XLD", 47,
                (x, s) => SampleLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        /* They're text anyway so not too bothered - at the moment they don't round trip due to using friendly asset id names
        // Would need to add a ToStringNumeric or something to the relevant events, starts getting ugly.
        [Fact]
        public void ScriptTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Script.TomMeetsChristine) };
            RoundTripXld<IList<IEvent>>(nameof(ScriptTest), "$(XLD)/SCRIPT0.XLD", 1,
                (x, s) => ScriptLoader.Serdes(x, info, AssetMapping.Global, s));
        } //*/

        [Fact]
        public void SongTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Song.Toronto) };
            RoundTripXld<byte[]>(nameof(SongTest), "$(XLD)/SONGS0.XLD", 3,
                (x, s) => SongLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void SpellTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Spell.FrostAvalanche) };
            RoundTripSpell<SpellData>(nameof(SpellTest), "$(XLD)/SPELLDAT.DAT", 7,
                (x, s) => SpellLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void TilesetTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Tileset.Toronto) };
            RoundTripXld<TilesetData>(nameof(TilesetTest), "$(XLD)/ICONDAT0.XLD", 7,
                (x, s) => TilesetLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void WaveLibTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(WaveLibrary.TorontoAmbient) };
            RoundTripXld<WaveLib>(nameof(WaveLibTest), "$(XLD)/WAVELIB0.XLD", 4,
                (x, s) => WaveLibLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void WordTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Special.Words1) };
            RoundTripXld<StringCollection>(nameof(WordTest), "$(XLD)/ENGLISH/WORDLIS0.XLD", 0,
                (x, s) => WordListLoader.Serdes(x, info, AssetMapping.Global, s));
        }
//*
        [Fact]
        public void AutomapGfxTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(AutomapTiles.Set1) };
            info.Set("SubSprites", "(8,8,576) (16,16)");
            RoundTripXld<AlbionSprite>(nameof(AutomapGfxTest), "$(XLD)/AUTOGFX0.XLD", 0,
                (x, s) => AmorphousSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void CombatBgTest()
        {
            var info = new AssetInfo
            {
                AssetId = AssetId.From(CombatBackground.Toronto),
                Width = 360
            };
            RoundTripXld<AlbionSprite>(nameof(CombatBgTest), "$(XLD)/COMBACK0.XLD", 0,
                (x, s) => FixedSizeSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
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
            RoundTripXld<AlbionSprite>(nameof(DungeonObjectTest), "$(XLD)/3DOBJEC2.XLD", 81,
                (x, s) => FixedSizeSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void FontTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Font.RegularFont), Width = 8, Height = 8 };
            RoundTripXld<AlbionSprite>(nameof(FontTest), "$(XLD)/FONTS0.XLD", 0,
                (x, s) => FontSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
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
            RoundTripRaw<AlbionSprite>(nameof(ItemSpriteTest), "$(XLD)/ITEMGFX",
                (x, s) => FixedSizeSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void SlabTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(UiBackground.Slab), Width = 360 };
            RoundTripRaw<AlbionSprite>(nameof(SlabTest), "$(XLD)/SLAB",
                (x, s) => SlabLoader.Serdes(x, info, AssetMapping.Global, s));
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
            RoundTripXld<AlbionSprite>(nameof(TileGfxTest), "$(XLD)/ICONGFX0.XLD", 7,
                (x, s) => FixedSizeSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void CombatGfxTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(CombatGraphics.Unknown27) };
            RoundTripXld<AlbionSprite>(nameof(CombatGfxTest), "$(XLD)/COMGFX0.XLD", 26,
                (x, s) => MultiHeaderSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void DungeonBgTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(DungeonBackground.EarlyGameL) };
            RoundTripXld<AlbionSprite>(nameof(DungeonBgTest), "$(XLD)/3DBCKGR0.XLD", 0,
                (x, s) => HeaderBasedSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
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
            RoundTripXld<AlbionSprite>(nameof(FloorTest), "$(XLD)/3DFLOOR0.XLD", 2,
                (x, s) => FixedSizeSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void FullBodyPictureTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(FullBodyPicture.Tom) };
            RoundTripXld<AlbionSprite>(nameof(FullBodyPictureTest), "$(XLD)/FBODPIX0.XLD", 0,
                (x, s) => HeaderBasedSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void LargeNpcTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(LargeNpc.Christine) };
            RoundTripXld<AlbionSprite>(nameof(LargeNpcTest), "$(XLD)/NPCGR0.XLD", 20,
                (x, s) => HeaderBasedSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void LargePartyMemberTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(LargePartyMember.Tom) };
            RoundTripXld<AlbionSprite>(nameof(LargePartyMemberTest), "$(XLD)/PARTGR0.XLD", 0,
                (x, s) => HeaderBasedSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void MonsterGfxTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(MonsterGraphics.Krondir) };
            RoundTripXld<AlbionSprite>(nameof(MonsterGfxTest), "$(XLD)/MONGFX0.XLD", 9,
                (x, s) => MultiHeaderSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void OverlayTest()
        {
            var info = new AssetInfo
            {
                AssetId = AssetId.From(WallOverlay.JiriWindow),
                Width = 44,
                File = new AssetFileInfo { Transposed = true }
            };
            RoundTripXld<AlbionSprite>(nameof(OverlayTest), "$(XLD)/3DOVERL0.XLD", 1,
                (x, s) => FixedSizeSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        /* No code to write these atm, if anyone wants to mod them or add new ones they can still use ImageMagick or something to convert to ILBM
        [Fact]
        public void PictureTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Picture.OpenChestWithGold) };
            RoundTripXld<InterlacedBitmap>(nameof(PictureTest), "$(XLD)/PICTURE0.XLD", 11,
                (x, s) => InterlacedBitmapLoader.Serdes(x, info, AssetMapping.Global, s));
        } //*/

        [Fact]
        public void PortraitTest()
        {
            var info = new AssetInfo
            {
                AssetId = AssetId.From(Portrait.Tom),
                Width = 34
            };
            RoundTripXld<AlbionSprite>(nameof(PortraitTest), "$(XLD)/SMLPORT0.XLD", 0,
                (x, s) => FixedSizeSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void SmallNpcTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(SmallNpc.Krondir) };
            RoundTripXld<AlbionSprite>(nameof(SmallNpcTest), "$(XLD)/NPCKL0.XLD", 22,
                (x, s) => HeaderBasedSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void SmallPartyMemberTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(SmallPartyMember.Tom) };
            RoundTripXld<AlbionSprite>(nameof(SmallPartyMemberTest), "$(XLD)/PARTKL0.XLD", 0,
                (x, s) => HeaderBasedSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void TacticalGfxTest()
        {
            var info = new AssetInfo
            {
                AssetId = AssetId.From(TacticalGraphics.Unknown1),
                Width = 32
            };
            RoundTripXld<AlbionSprite>(nameof(TacticalGfxTest), "$(XLD)/TACTICO0.XLD", 0,
                (x, s) => FixedSizeSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void WallTest()
        {
            var info = new AssetInfo
            {
                AssetId = AssetId.From(Wall.TorontoPanelling),
                Width = 80
            };
            RoundTripXld<AlbionSprite>(nameof(WallTest), "$(XLD)/3DWALLS0.XLD", 11,
                (x, s) => FixedSizeSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
        }
// */
        static readonly AlbionStringTableLoader AlbionStringTableLoader = new AlbionStringTableLoader();
        static readonly AmorphousSpriteLoader AmorphousSpriteLoader = new AmorphousSpriteLoader();
        static readonly AutomapLoader AutomapLoader = new AutomapLoader();
        static readonly BlockListLoader BlockListLoader = new BlockListLoader();
        static readonly CharacterSheetLoader CharacterSheetLoader = new CharacterSheetLoader();
        static readonly ChestLoader ChestLoader = new ChestLoader();
        static readonly EventSetLoader EventSetLoader = new EventSetLoader();
        static readonly FixedSizeSpriteLoader FixedSizeSpriteLoader = new FixedSizeSpriteLoader();
        static readonly FontSpriteLoader FontSpriteLoader = new FontSpriteLoader();
        static readonly HeaderBasedSpriteLoader HeaderBasedSpriteLoader = new HeaderBasedSpriteLoader();
        static readonly MultiHeaderSpriteLoader MultiHeaderSpriteLoader = new MultiHeaderSpriteLoader();
        // static readonly InterlacedBitmapLoader InterlacedBitmapLoader = new InterlacedBitmapLoader();
        static readonly ItemDataLoader ItemDataLoader = new ItemDataLoader();
        static readonly ItemNameLoader ItemNameLoader = new ItemNameLoader();
        static readonly LabyrinthDataLoader LabyrinthDataLoader = new LabyrinthDataLoader();
        static readonly MerchantLoader MerchantLoader = new MerchantLoader();
        static readonly MonsterGroupLoader MonsterGroupLoader = new MonsterGroupLoader();
        static readonly PaletteLoader PaletteLoader = new PaletteLoader();
        static readonly SampleLoader SampleLoader = new SampleLoader();
        // static readonly ScriptLoader ScriptLoader = new ScriptLoader();
        static readonly SlabLoader SlabLoader = new SlabLoader();
        static readonly SongLoader SongLoader = new SongLoader();
        static readonly SpellLoader SpellLoader = new SpellLoader();
        static readonly TilesetLoader TilesetLoader = new TilesetLoader();
        static readonly WaveLibLoader WaveLibLoader = new WaveLibLoader();
        static readonly WordListLoader WordListLoader = new WordListLoader();
    }
}
