using System;
using System.IO;
using System.Linq;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Game.Assets;
using UAlbion.Game.Settings;
using UAlbion.TestCommon;
using Xunit;

namespace UAlbion.Base.Tests
{
    public class AssetConversionTests
    {
        const string BaseAssetMod = "Base";
        const string UnpackedAssetMod = "Unpacked";
        const string RepackedAssetMod = "Repacked";

        readonly string _baseDir;
        readonly IFileSystem _disk;
        readonly IModApplier _baseApplier;

        public AssetConversionTests()
        {
            AssetMapping.GlobalIsThreadLocal = true;
            // Hide any assets that have already been unpacked on the actual disk to prevent them interfering in the tests
            _disk = new MockFileSystem(x =>
                !x.Contains(Path.Combine("mods", UnpackedAssetMod, "Assets")) &&
                !x.Contains(Path.Combine("mods", RepackedAssetMod, "Albion")));

            _baseDir = ConfigUtil.FindBasePath(_disk);
            _baseApplier = BuildApplier(BaseAssetMod);
        }

        IModApplier BuildApplier(string mod)
        {
            var generalConfig = AssetSystem.LoadGeneralConfig(_baseDir, _disk);
            var coreConfig = new CoreConfig();
            var gameConfig = AssetSystem.LoadGameConfig(_baseDir, _disk);
            var settings = new GeneralSettings
            {
                ActiveMods = { mod },
                Language = Language.English
            };
            var factory = new MockFactory();
            var exchange = AssetSystem.Setup(_disk, factory, generalConfig, settings, coreConfig, gameConfig);
            return exchange.Resolve<IModApplier>();
        }

        void Test<T>(AssetId id,
            AssetId[] prerequisites,
            Func<T, ISerializer, T> serdes) where T : class
        {
            prerequisites ??= Array.Empty<AssetId>();
            var allIds = prerequisites.Append(id);

            var resultsDir = Path.Combine(_baseDir, "re", "ConversionTests");

            var baseAsset = (T)_baseApplier.LoadAsset(id);
            var (baseBytes, baseNotes) = Asset.Save(baseAsset, serdes);

            var idStrings = allIds.Select(x => $"{x.Type}.{x.Id}").ToArray();
            var assetTypes = allIds.Select(x => x.Type).Distinct().ToHashSet();

            ConvertAssets.Convert(
                _disk,
                new MockFactory(),
                BaseAssetMod,
                UnpackedAssetMod,
                idStrings,
                assetTypes);

            var unpackedAsset = (T)BuildApplier(UnpackedAssetMod).LoadAsset(id);
            var (unpackedBytes, unpackedNotes) = Asset.Save(unpackedAsset, serdes);
            Asset.Compare(resultsDir, id.Type.ToString(), baseBytes, unpackedBytes, baseNotes, unpackedNotes, null);

            ConvertAssets.Convert(
                _disk,
                new MockFactory(),
                UnpackedAssetMod,
                RepackedAssetMod,
                idStrings,
                assetTypes);

            var repackedAsset = (T)BuildApplier(RepackedAssetMod).LoadAsset(id);
            var (repackedBytes, repackedNotes) = Asset.Save(repackedAsset, serdes);
            Asset.Compare(resultsDir, id.Type.ToString(), baseBytes, repackedBytes, baseNotes, repackedNotes, null);
        }

        [Fact]
        public void ItemTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Item.Knife) };
            Test<ItemData>(info.AssetId, null, (x, s) => Loaders.ItemDataLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void ItemNameTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Special.ItemNames) };
            Test<MultiLanguageStringDictionary>(info.AssetId, null, (x, s) => Loaders.ItemNameLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void AutomapTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Automap.Jirinaar) };
            Test<Formats.Assets.Automap>(info.AssetId, null, (x, s) => Loaders.AutomapLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void BlockListTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(BlockList.Toronto) };
            Test<Formats.Assets.BlockList>(info.AssetId, null, (x, s) => Loaders.BlockListLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void ChestTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Chest.Unknown121) };
            Test<Inventory>(info.AssetId, null, (x, s) => Loaders.ChestLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void CommonPaletteTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Palette.Common) };
            info.Set(AssetProperty.IsCommon, true);
            Test<AlbionPalette>(info.AssetId, null, (x, s) => Loaders.PaletteLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void EventSetTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(EventSet.Frill) };
            Test<Formats.Assets.EventSet>(info.AssetId, null, (x, s) => Loaders.EventSetLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void EventTextTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(EventText.Frill) };
            Test<ListStringCollection>(info.AssetId, null, (x, s) => Loaders.AlbionStringTableLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void LabyrinthTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(LabyrinthData.Jirinaar) };
            Test<Formats.Assets.Labyrinth.LabyrinthData>(info.AssetId, null, (x, s) => Loaders.LabyrinthDataLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void Map2DTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Map.TorontoBegin) };
            Test<MapData2D>(info.AssetId, null, (x, s) => MapData2D.Serdes(info, x, AssetMapping.Global, s));
        }


        [Fact]
        public void Map3DTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Map.OldFormerBuilding) };
            Test<MapData3D>(info.AssetId, null, (x, s) => MapData3D.Serdes(info, x, AssetMapping.Global, s));
        }

        [Fact]
        public void MapTextTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(MapText.TorontoBegin) };
            Test<ListStringCollection>(info.AssetId, null, (x, s) => Loaders.AlbionStringTableLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void MerchantTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Merchant.Unknown109) };
            Test<Inventory>(info.AssetId, null, (x, s) => Loaders.MerchantLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void MonsterGroupTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(MonsterGroup.TwoSkrinn1OneKrondir1) };
            Test<Formats.Assets.MonsterGroup>(info.AssetId, null, (x, s) => Loaders.MonsterGroupLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void MonsterTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Monster.Krondir1) };
            Test<CharacterSheet>(info.AssetId, null, (x, s) => Loaders.CharacterSheetLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void NpcTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Npc.Christine) };
            Test<CharacterSheet>(info.AssetId, null, (x, s) => Loaders.CharacterSheetLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void PaletteTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Palette.Toronto2D) };
            Test<AlbionPalette>(info.AssetId, new[] { AssetId.From(Palette.Common) }, (x, s) => Loaders.PaletteLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void PartyMemberTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(PartyMember.Tom) };
            Test<CharacterSheet>(info.AssetId, null, (x, s) => Loaders.CharacterSheetLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void SampleTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Sample.IllTemperedLlama) };
            Test<AlbionSample>(info.AssetId, null, (x, s) => Loaders.SampleLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        /* They're text anyway so not too bothered - at the moment they don't round trip due to using friendly asset id names
        // Would need to add a ToStringNumeric or something to the relevant events, starts getting ugly.
        [Fact]
        public void ScriptTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Script.TomMeetsChristine) };
            Test<IList<IEvent>>(info.AssetId, null, (x, s) => Loaders.ScriptLoader.Serdes(x, info, AssetMapping.Global, s));
        } //*/

        [Fact]
        public void SongTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Song.Toronto) };
            Test<byte[]>(info.AssetId, null, (x, s) => Loaders.SongLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void SpellTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Spell.FrostAvalanche) };
            Test<SpellData>(info.AssetId, null, (x, s) => Loaders.SpellLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void TilesetTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Tileset.Toronto) };
            Test<TilesetData>(info.AssetId, null, (x, s) => Loaders.TilesetLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void WaveLibTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(WaveLibrary.TorontoAmbient) };
            Test<WaveLib>(info.AssetId, null, (x, s) => Loaders.WaveLibLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void WordTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Special.Words1) };
            Test<ListStringCollection>(info.AssetId, null, (x, s) => Loaders.WordListLoader.Serdes(x, info, AssetMapping.Global, s));
        }
        //*
        [Fact]
        public void AutomapGfxTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(AutomapTiles.Set1) };
            info.Set(AssetProperty.SubSprites, "(8,8,576) (16,16)");
            Test<IEightBitImage>(info.AssetId,
                new[] { AssetId.From(Palette.Common) },
                (x, s) => Loaders.AmorphousSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void CombatBgTest()
        {
            var info = new AssetInfo
            {
                AssetId = AssetId.From(CombatBackground.Toronto),
                Width = 360
            };
            Test<IEightBitImage>(info.AssetId,
                new[] { AssetId.From(Palette.TorontoCombat), AssetId.From(Palette.Common) },
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
            Test<IEightBitImage>(info.AssetId,
                new[] { AssetId.From(Palette.GlowyPlantDungeon), AssetId.From(Palette.Common) },
                (x, s) => Loaders.FixedSizeSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void FontTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Font.RegularFont), Width = 8, Height = 8 };
            Test<IEightBitImage>(info.AssetId,
                new[] { AssetId.From(Palette.Common) },
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
            Test<IEightBitImage>(info.AssetId,
                new[] { AssetId.From(Palette.Common) },
                (x, s) => Loaders.FixedSizeSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void SlabTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(UiBackground.Slab), Width = 360 };
            Test<IEightBitImage>(info.AssetId,
                new[] { AssetId.From(Palette.Common) },
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
            Test<IEightBitImage>(info.AssetId,
                new[] { AssetId.From(Palette.Toronto2D), AssetId.From(Palette.Common) },
                (x, s) => Loaders.FixedSizeSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void CombatGfxTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(CombatGraphics.Unknown27) };
            Test<IEightBitImage>(info.AssetId,
                new[] { AssetId.From(Palette.PlainsCombat), AssetId.From(Palette.Common) },
                (x, s) => Loaders.MultiHeaderSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void DungeonBgTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(DungeonBackground.EarlyGameL) };
            Test<IEightBitImage>(info.AssetId,
                new[] { AssetId.From(Palette.JirinaarDay), AssetId.From(Palette.Common) },
                (x, s) => Loaders.HeaderBasedSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
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
            Test<IEightBitImage>(info.AssetId,
                new[] { AssetId.From(Palette.JirinaarDay), AssetId.From(Palette.Common) },
                (x, s) => Loaders.FixedSizeSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void FullBodyPictureTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(FullBodyPicture.Tom) };
            Test<IEightBitImage>(info.AssetId,
                new[] { AssetId.From(Palette.Inventory), AssetId.From(Palette.Common) },
                (x, s) => Loaders.HeaderBasedSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void LargeNpcTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(LargeNpc.Christine) };
            Test<IEightBitImage>(info.AssetId,
                new[] { AssetId.From(Palette.Toronto2D), AssetId.From(Palette.Common) },
                (x, s) => Loaders.HeaderBasedSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void LargePartyMemberTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(LargePartyMember.Tom) };
            Test<IEightBitImage>(info.AssetId,
                new[] { AssetId.From(Palette.IskaiIndoorDark), AssetId.From(Palette.Common) },
                (x, s) => Loaders.HeaderBasedSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void MonsterGfxTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(MonsterGraphics.Krondir) };
            Test<IEightBitImage>(info.AssetId,
                new[] { AssetId.From(Palette.DungeonCombat), AssetId.From(Palette.Common) },
                (x, s) => Loaders.MultiHeaderSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
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
            Test<IEightBitImage>(info.AssetId,
                new[] { AssetId.From(Palette.JirinaarDay), AssetId.From(Palette.Common) },
                (x, s) => Loaders.FixedSizeSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        /* No code to write these atm, if anyone wants to mod them or add new ones they can still use ImageMagick or something to convert to ILBM
        [Fact]
        public void PictureTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(Picture.OpenChestWithGold) };
            Test<InterlacedBitmap>(info.AssetId, null, (x, s) => Loaders.InterlacedBitmapLoader.Serdes(x, info, AssetMapping.Global, s));
        } //*/

        [Fact]
        public void PortraitTest()
        {
            var info = new AssetInfo
            {
                AssetId = AssetId.From(Portrait.Tom),
                Width = 34
            };
            Test<IEightBitImage>(info.AssetId,
                new[] { AssetId.From(Palette.Common) },
                (x, s) => Loaders.FixedSizeSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void SmallNpcTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(SmallNpc.Krondir) };
            Test<IEightBitImage>(info.AssetId,
                new[] { AssetId.From(Palette.FirstIslandDay), AssetId.From(Palette.Common) },
                (x, s) => Loaders.HeaderBasedSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void SmallPartyMemberTest()
        {
            var info = new AssetInfo { AssetId = AssetId.From(SmallPartyMember.Tom) };
            Test<IEightBitImage>(info.AssetId,
                new[] { AssetId.From(Palette.FirstIslandDay), AssetId.From(Palette.Common) },
                (x, s) => Loaders.HeaderBasedSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
        }

        [Fact]
        public void TacticalGfxTest()
        {
            var info = new AssetInfo
            {
                AssetId = AssetId.From(TacticalGraphics.Unknown1),
                Width = 32
            };
            Test<IEightBitImage>(info.AssetId,
                new[] { AssetId.From(Palette.Common) },
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
            Test<IEightBitImage>(info.AssetId,
                new[] { AssetId.From(Palette.Toronto3D), AssetId.From(Palette.Common) },
                (x, s) => Loaders.FixedSizeSpriteLoader.Serdes(x, info, AssetMapping.Global, s));
        }
        // */
    }
}
