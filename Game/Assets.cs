using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Textures;
using UAlbion.Formats;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;
using UAlbion.Game.Events;

namespace UAlbion.Game
{
    public class Assets : Component, IDisposable, IAssetManager
    {
        public Assets(AssetConfig assetConfig, CoreSpriteConfig coreSpriteConfig) : base(Handlers)
        {
            _assetConfig = assetConfig;
            _coreSpriteConfig = coreSpriteConfig;
            _assetCache = new AssetCache();
        }

        static readonly HandlerSet Handlers = new HandlerSet(
            H<Assets, SubscribedEvent>((x, e) => x._assetCache.Attach(x.Exchange))
        );

        readonly AssetConfig _assetConfig;
        readonly CoreSpriteConfig _coreSpriteConfig;
        readonly IDictionary<AssetType, XldFile[]> _xlds = new Dictionary<AssetType, XldFile[]>();

        // ReSharper disable StringLiteralTypo
        readonly IDictionary<AssetType, (AssetLocation, string)> _assetFiles = new Dictionary<AssetType, (AssetLocation, string)> {
            // General game assets
            { AssetType.Palette,            (AssetLocation.Base,      "PALETTE!.XLD") }, // Palettes (first 192 colours)
            { AssetType.PaletteNull,        (AssetLocation.BaseRaw,   "PALETTE.000" ) }, // The 64 colours above 192 that are shared by all palettes.
            { AssetType.SmallPortrait,      (AssetLocation.Base,      "SMLPORT!.XLD") }, // Small portraits of players and NPCs for conversations etc
            { AssetType.EventSet,           (AssetLocation.Base,      "EVNTSET!.XLD") }, // 
            { AssetType.Song,               (AssetLocation.Base,      "SONGS!.XLD"  ) }, // XMI audio, can be translated to MIDI
            { AssetType.Sample,             (AssetLocation.Base,      "SAMPLES!.XLD") }, // General sound effects
            { AssetType.WaveLibrary,        (AssetLocation.Base,      "WAVELIB!.XLD") }, // General sound effects
            { AssetType.BlockList,          (AssetLocation.Base,      "BLKLIST!.XLD") }, // 
            { AssetType.Flic,               (AssetLocation.Localised, "FLICS!.XLD"  ) }, // Videos
            { AssetType.Dictionary,         (AssetLocation.Localised, "WORDLIS!.XLD") }, // The words that can be used as conversation topics
            { AssetType.Script,             (AssetLocation.Base,      "SCRIPT!.XLD" ) }, // Scripted sequences of events for narrative sequences etc
            { AssetType.Picture,            (AssetLocation.Base,      "PICTURE!.XLD") }, // Full screen graphics for various special events, menu backgrounds etc (in the obscure ILBM / interlaced bitmap format from IBM)
            { AssetType.TransparencyTables, (AssetLocation.Base,      "TRANSTB!.XLD") }, // 
            { AssetType.CoreGraphics,       (AssetLocation.MainExe,   "MAIN.EXE") }, // Various UI graphics that get loaded directly from the original game executable
            { AssetType.CoreGraphicsMetadata, (AssetLocation.MainExe, "MAIN.EXE") },
            // { AssetType.Unnamed2,        (AssetLocation.Base,      ""            ) },

            // Text
            { AssetType.Font,       (AssetLocation.Base,              "FONTS!.XLD"  ) }, // Fonts (raw, 8 wide. 00 = normal, 01 = bold)
            { AssetType.MetaFont,   (AssetLocation.Meta,              null          ) }, // Generated from a Font, with colours changed appropriately
            { AssetType.SystemText, (AssetLocation.LocalisedRaw,      "SYSTEXTS"    ) }, // Core game strings for things like menus etc
            { AssetType.EventText,  (AssetLocation.Localised,         "EVNTTXT!.XLD") }, // Text to show for various gameplay events
            { AssetType.MapText,    (AssetLocation.Localised,         "MAPTEXT!.XLD") }, // Map-specific text

            // Inventory / merchant assets
            { AssetType.Slab,               (AssetLocation.BaseRaw,   "SLAB"        ) }, // The background texture for the inventory screen
            { AssetType.ItemNames,          (AssetLocation.BaseRaw,   "ITEMNAME.DAT") }, // Item names (array of fixed length strings)
            { AssetType.ItemList,           (AssetLocation.BaseRaw,   "ITEMLIST.DAT") }, // Item statistics
            { AssetType.ItemGraphics,       (AssetLocation.BaseRaw,   "ITEMGFX"     ) }, // Inventory item sprites
            { AssetType.FullBodyPicture,    (AssetLocation.Base,      "FBODPIX!.XLD") }, // Full body party member images for the inventory screen
            { AssetType.ChestData,          (AssetLocation.Initial,   "CHESTDT!.XLD") }, // Chest contents
            { AssetType.MerchantData,       (AssetLocation.Initial,   "MERCHDT!.XLD") }, // Merchant inventories

            // 2D map assets
            { AssetType.MapData,            (AssetLocation.Base,      "MAPDATA!.XLD") }, // 2D maps
            { AssetType.IconData,           (AssetLocation.Base,      "ICONDAT!.XLD") }, // Tileset info for the 2D maps, including animated tile ranges etc
            { AssetType.IconGraphics,       (AssetLocation.Base,      "ICONGFX!.XLD") }, // Tiles for the 2D maps
            { AssetType.BigPartyGraphics,   (AssetLocation.Base,      "PARTGR!.XLD" ) }, // Regular scale party-member sprites.
            { AssetType.BigNpcGraphics,     (AssetLocation.Base,      "NPCGR!.XLD"  ) }, // Regular scale NPC sprites
            { AssetType.SmallPartyGraphics, (AssetLocation.Base,      "PARTKL!.XLD" ) }, // Small scale party-member sprites (i.e. on outdoor maps)
            { AssetType.SmallNpcGraphics,   (AssetLocation.Base,      "NPCKL!.XLD"  ) }, // Small scale NPC sprites (i.e. on outdoor maps)

            // 3D map assets
            { AssetType.LabData,            (AssetLocation.Base,      "LABDATA!.XLD") }, // Labyrinth data, defines tileset etc for 3D maps.
            { AssetType.Wall3D,             (AssetLocation.Base,      "3DWALLS!.XLD") }, // Wall textures for 3D maps
            { AssetType.Object3D,           (AssetLocation.Base,      "3DOBJEC!.XLD") }, // Object sprites for 3D maps
            { AssetType.Overlay3D,          (AssetLocation.Base,      "3DOVERL!.XLD") }, // Wall decals for 3D maps
            { AssetType.Floor3D,            (AssetLocation.Base,      "3DFLOOR!.XLD") }, // Floor and ceiling textures for 3D maps
            { AssetType.BackgroundGraphics, (AssetLocation.Base,      "3DBCKGR!.XLD") }, // Skybox textures for 3D maps
            { AssetType.Automap,            (AssetLocation.Initial,   "AUTOMAP!.XLD") }, //
            { AssetType.AutomapGraphics,    (AssetLocation.Base,      "AUTOGFX!.XLD") }, // Tiles for the map screen on 3D maps

            // Combat assets
            { AssetType.PartyCharacterData, (AssetLocation.Initial,   "PRTCHAR!.XLD") }, // Character statistics for party members
            { AssetType.NpcCharacterData,   (AssetLocation.Initial,   "NPCCHAR!.XLD") }, // NPC character statistics
            { AssetType.MonsterCharacter,   (AssetLocation.Base,      "MONCHAR!.XLD") }, // Monster character statistics
            { AssetType.MonsterGroup,       (AssetLocation.Base,      "MONGRP!.XLD" ) }, // Pre-defined monster groupings for combat
            { AssetType.MonsterGraphics,    (AssetLocation.Base,      "MONGFX!.XLD" ) }, // Monster sprites on the 3D combat screen
            { AssetType.CombatGraphics,     (AssetLocation.Base,      "COMGFX!.XLD" ) }, // Various sprites and effects for 3D combat screen, spells etc
            { AssetType.TacticalIcon,       (AssetLocation.Base,      "TACTICO!.XLD") }, // Sprites for the combat chessboard-view
            { AssetType.CombatBackground,   (AssetLocation.Base,      "COMBACK!.XLD") }, // Background graphics for combat
            { AssetType.SpellData,          (AssetLocation.Base,      "SPELLDAT.DAT") }, // Spell definitions & statistics
        };
        // ReSharper restore StringLiteralTypo

        class AssetPaths
        {
            public string OverridePath { get; set; }
            public string XldPath { get; set; }
            public string XldNameInConfig { get; set; }
        }

        readonly string[] _overrideExtensions = { "bmp", "png", "wav", "json", "mp3" };
        readonly AssetCache _assetCache;

        AssetPaths GetAssetPaths(AssetLocation location, GameLanguage language, string baseName, int number, int objectNumber)
        {
            string Try(string x)
            {
                foreach (var extension in _overrideExtensions)
                {
                    var path = $"{x}.{extension}";
                    if (File.Exists(path))
                        return path;
                }
                return null;
            }

            Debug.Assert(number >= 0);
            Debug.Assert(number <= 9);

            var result = new AssetPaths();
            baseName = baseName.Replace("!", number.ToString());
            var lang = language.ToString().ToUpper();
            switch (location)
            {
                case AssetLocation.Base:
                    result.XldPath = Path.Combine(_assetConfig.BasePath, _assetConfig.XldPath, baseName);
                    result.OverridePath = Try(Path.Combine(_assetConfig.BaseDataPath, baseName, objectNumber.ToString()));
                    result.XldNameInConfig = baseName;
                    break;

                case AssetLocation.BaseRaw:
                    result.XldPath = Path.Combine(_assetConfig.BasePath, _assetConfig.XldPath, baseName);
                    result.OverridePath = Try(Path.Combine(_assetConfig.BaseDataPath, baseName));
                    result.XldNameInConfig = baseName;
                    break;

                case AssetLocation.Localised:
                    result.XldPath = Path.Combine(_assetConfig.BasePath, _assetConfig.XldPath, lang, baseName);
                    result.OverridePath = Try(Path.Combine(_assetConfig.BaseDataPath, lang, baseName, objectNumber.ToString()));
                    result.XldNameInConfig = "$(LANG)/" + baseName;
                    break;

                case AssetLocation.LocalisedRaw:
                    result.XldPath = Path.Combine(_assetConfig.BasePath, _assetConfig.XldPath, lang, baseName);
                    result.OverridePath = Try(Path.Combine(_assetConfig.BaseDataPath, lang, baseName));
                    result.XldNameInConfig = "$(LANG)/" + baseName;
                    break;

                case AssetLocation.Initial:
                    result.XldPath = Path.Combine(_assetConfig.BasePath, _assetConfig.XldPath, "INITIAL", baseName);
                    result.OverridePath = Try(Path.Combine(_assetConfig.BaseDataPath, "INITIAL", baseName, objectNumber.ToString()));
                    result.XldNameInConfig = "INITIAL/" + baseName;
                    break;

                case AssetLocation.Current:
                    result.XldPath = Path.Combine(_assetConfig.BasePath, _assetConfig.XldPath, "CURRENT", baseName);
                    result.OverridePath = Try(Path.Combine(_assetConfig.BaseDataPath, "CURRENT", baseName, objectNumber.ToString()));
                    result.XldNameInConfig = "INITIAL/" + baseName; // Note: Use the same metadata for CURRENT & INITIAL
                    break;

                default: throw new ArgumentOutOfRangeException("Invalid asset location");
            }

            return result;
        }

        public class ReaderScope : IDisposable
        {
            readonly BinaryReader _br;
            readonly Stream _stream;

            public ReaderScope(BinaryReader br, Stream stream)
            {
                _br = br;
                _stream = stream;
            }

            public void Dispose()
            {
                _br?.Dispose();
                _stream?.Dispose();
            }
        }

        object LoadAsset(AssetType type, int id, string name, GameLanguage language)
        {
            if (type == AssetType.CoreGraphics)
                return AssetLoader.LoadCoreSprite((CoreSpriteId)id, _assetConfig.BasePath, _coreSpriteConfig);

            if (type == AssetType.CoreGraphicsMetadata)
                return AssetLoader.LoadCoreSpriteMetadata((CoreSpriteId)id, _assetConfig.BasePath, _coreSpriteConfig);

            if (type == AssetType.MetaFont)
                return FontLoader.Load((MetaFontId)id, LoadTexture(FontId.RegularFont), LoadTexture(FontId.BoldFont));

            int xldIndex = id / 100;
            Debug.Assert(xldIndex >= 0);
            Debug.Assert(xldIndex <= 9);
            int objectIndex = id % 100;

            var (location, baseName) = _assetFiles[type];
            var paths = GetAssetPaths(location, language, baseName, xldIndex, objectIndex);
            var xldConfig = _assetConfig.Xlds[paths.XldNameInConfig];
            xldConfig.Assets.TryGetValue(objectIndex, out var assetConfig);

            if (paths.OverridePath != null || IsLocationRaw(location))
            {
                var path = paths.OverridePath ?? paths.XldPath;
                using var stream = File.OpenRead(path);
                using var br = new BinaryReader(stream);
                var asset = AssetLoader.Load(br, name, (int)stream.Length, assetConfig);
                if (asset == null)
                    throw new AssetNotFoundException($"Object {type}:{id} could not be loaded from file {path}", type, id);
                GameTrace.Log.AssetLoaded(type, id, name, language, path);

                return asset;
            }

            if (!_xlds.ContainsKey(type))
                _xlds[type] = new XldFile[10];

            if (File.Exists(paths.XldPath) && _xlds[type][xldIndex] == null)
                _xlds[type][xldIndex] = new XldFile(paths.XldPath);

            var xldArray = _xlds[type];
            var xld = xldArray[xldIndex];
            if (xld == null)
                throw new AssetNotFoundException($"XLD not found for object: {type}:{id} in {baseName} ({location})", type, id);

            using (var br = xld.GetReaderForObject(objectIndex, out var length))
            {
                if (length == 0)
                    return null;

                var asset = AssetLoader.Load(br, name, length, assetConfig);
                if (asset == null)
                    throw new AssetNotFoundException($"Object {type}:{id} could not be loaded from XLD {xld.Filename}", type, id);
                GameTrace.Log.AssetLoaded(type, id, name, language, paths.XldPath);

                return asset;
            }
        }

        bool IsLocationRaw(AssetLocation location)
        {
            switch (location)
            {
                case AssetLocation.BaseRaw:
                case AssetLocation.LocalisedRaw:
                    return true;
                default: return false;
            }
        }

        object LoadAssetCached<T>(AssetType type, T enumId, GameLanguage language = GameLanguage.English)
        {
            int id = Convert.ToInt32(enumId);
            object asset = _assetCache.Get(type, id, language);
            if (asset is Exception) // If it failed to load once then stop trying (at least until an asset:reload / cycle)
                return null;

            if (asset != null)
                return asset;

            var name = $"{type}.{enumId}";
            try
            {
                asset = LoadAsset(type, id, name, language);
            }
            catch (Exception e)
            {
                Raise(new LogEvent(LogEvent.Level.Error, $"Could not load asset {name}: {e}"));
                asset = e;
            }

            _assetCache.Add(asset, type, id, language);
            return asset is Exception ? null : asset;
        }

        public MapData2D LoadMap2D(MapDataId id) => LoadAssetCached(AssetType.MapData, id) as MapData2D;
        public MapData3D LoadMap3D(MapDataId id) => LoadAssetCached(AssetType.MapData, id) as MapData3D;
        public ItemData LoadItem(ItemId id)
        {
            var data = (IList<ItemData>)LoadAssetCached(AssetType.ItemList, 0);
            if (data[0].Names == null)
            {
                var names = (IList<string>) LoadAssetCached(AssetType.ItemNames, 0);
                for (int i = 0; i < data.Count; i++)
                    data[i].Names = names.Skip(i * 3).Take(3).ToArray();
            }

            return data[(int)id];
        }


        public AlbionPalette LoadPalette(PaletteId id)
        {
            var palette = (AlbionPalette)LoadAssetCached(AssetType.Palette, id);
            if (palette == null)
                return null;

            var commonPalette = (byte[])LoadAssetCached(AssetType.PaletteNull, 0);
            palette.SetCommonPalette(commonPalette);

            return palette;
        }

        public ITexture LoadTexture(AssetType type, int id)
        {
            switch (type)
            {
                case AssetType.AutomapGraphics: return LoadTexture((AutoMapId)id);
                case AssetType.BackgroundGraphics: return LoadTexture((DungeonBackgroundId)id);
                case AssetType.BigNpcGraphics: return LoadTexture((LargeNpcId)id);
                case AssetType.BigPartyGraphics: return LoadTexture((LargePartyGraphicsId)id);
                case AssetType.CombatBackground: return LoadTexture((CombatBackgroundId)id);
                case AssetType.CombatGraphics: return LoadTexture((CombatGraphicsId)id);
                case AssetType.Floor3D: return LoadTexture((DungeonFloorId)id);
                case AssetType.Font: return LoadTexture((FontId)id);
                case AssetType.FullBodyPicture: return LoadTexture((FullBodyPictureId)id);
                case AssetType.IconGraphics: return LoadTexture((IconGraphicsId)id);
                case AssetType.ItemGraphics: return LoadTexture((ItemId)id);
                case AssetType.MonsterGraphics: return LoadTexture((MonsterGraphicsId)id);
                case AssetType.Object3D: return LoadTexture((DungeonObjectId)id);
                case AssetType.Overlay3D: return LoadTexture((DungeonOverlayId)id);
                case AssetType.Picture: return LoadTexture((PictureId)id);
                case AssetType.SmallNpcGraphics: return LoadTexture((SmallNpcId)id);
                case AssetType.SmallPartyGraphics: return LoadTexture((SmallPartyGraphicsId)id);
                case AssetType.SmallPortrait: return LoadTexture((SmallPortraitId)id);
                case AssetType.TacticalIcon: return LoadTexture((TacticId)id);
                case AssetType.Wall3D: return LoadTexture((DungeonWallId)id);
                case AssetType.CoreGraphics: return LoadTexture((CoreSpriteId)id);
                case AssetType.Slab: return (ITexture)LoadAssetCached(AssetType.Slab, 0);
                default: return (ITexture)LoadAssetCached(type, id);
            }
        }

        public ITexture LoadTexture(AutoMapId id) => (ITexture)LoadAssetCached(AssetType.AutomapGraphics, id);
        public ITexture LoadTexture(CombatBackgroundId id) => (ITexture)LoadAssetCached(AssetType.CombatBackground, id);
        public ITexture LoadTexture(CombatGraphicsId id) => (ITexture)LoadAssetCached(AssetType.CombatGraphics, id);
        public ITexture LoadTexture(DungeonBackgroundId id) => (ITexture)LoadAssetCached(AssetType.BackgroundGraphics, id);
        public ITexture LoadTexture(DungeonFloorId id) => (ITexture)LoadAssetCached(AssetType.Floor3D, id);
        public ITexture LoadTexture(DungeonObjectId id) => (ITexture)LoadAssetCached(AssetType.Object3D, id);
        public ITexture LoadTexture(DungeonOverlayId id) => (ITexture)LoadAssetCached(AssetType.Overlay3D, id);
        public ITexture LoadTexture(DungeonWallId id) => (ITexture)LoadAssetCached(AssetType.Wall3D, id);
        public ITexture LoadTexture(FullBodyPictureId id) => (ITexture)LoadAssetCached(AssetType.FullBodyPicture, id);
        public ITexture LoadTexture(IconGraphicsId id) => (ITexture)LoadAssetCached(AssetType.IconGraphics, id);
        public ITexture LoadTexture(ItemId id) => (ITexture)LoadAssetCached(AssetType.ItemGraphics, id); // TODO: Enum
        public ITexture LoadTexture(LargeNpcId id) => (ITexture)LoadAssetCached(AssetType.BigNpcGraphics, id);
        public ITexture LoadTexture(LargePartyGraphicsId id) => (ITexture)LoadAssetCached(AssetType.BigPartyGraphics, id);
        public ITexture LoadTexture(MonsterGraphicsId id) => (ITexture)LoadAssetCached(AssetType.MonsterGraphics, id);
        public ITexture LoadTexture(PictureId id) => (ITexture)LoadAssetCached(AssetType.Picture, id);
        public ITexture LoadTexture(SmallNpcId id) => (ITexture)LoadAssetCached(AssetType.SmallNpcGraphics, id);
        public ITexture LoadTexture(SmallPartyGraphicsId id) => (ITexture)LoadAssetCached(AssetType.SmallPartyGraphics, id);
        public ITexture LoadTexture(SmallPortraitId id) => (ITexture)LoadAssetCached(AssetType.SmallPortrait, id);
        public ITexture LoadTexture(TacticId id) => (ITexture)LoadAssetCached(AssetType.TacticalIcon, id);
        public ITexture LoadTexture(FontId id) => (ITexture)LoadAssetCached(AssetType.Font, id);
        public ITexture LoadTexture(MetaFontId id) => (ITexture)LoadAssetCached(AssetType.MetaFont, id);
        public ITexture LoadTexture(CoreSpriteId id) => (ITexture)LoadAssetCached(AssetType.CoreGraphics, id);
        public TilesetData LoadTileData(IconDataId id) => (TilesetData)LoadAssetCached(AssetType.IconData, id);
        public LabyrinthData LoadLabyrinthData(LabyrinthDataId id) => (LabyrinthData)LoadAssetCached(AssetType.LabData, id);
        public ITexture LoadFont(FontColor color, bool isBold) => LoadTexture(new MetaFontId(isBold, color));
        public CoreSpriteConfig.BinaryResource LoadCoreSpriteInfo(CoreSpriteId id) =>
            (CoreSpriteConfig.BinaryResource)LoadAssetCached(AssetType.CoreGraphicsMetadata, id);

        public string LoadString(StringId id, GameLanguage language)
        {
            var stringTable = (IDictionary<int, string>)LoadAssetCached(id.Type, id.Id, language);
            return stringTable[id.SubId];
        }

        public string LoadString(SystemTextId id, GameLanguage language) => LoadString(new StringId(AssetType.SystemText, 0, (int)id), language);
        public string LoadString(WordId id, GameLanguage language) => LoadString(new StringId(AssetType.Dictionary, (int)id / 500, (int)id), language);

        public AlbionSample LoadSample(AssetType type, int id) => (AlbionSample)LoadAssetCached(type, id);
        public AlbionVideo LoadVideo(VideoId id, GameLanguage language) => (AlbionVideo)LoadAsset(AssetType.Flic, (int)id, $"Video:{id}", language); // Don't cache videos.
        public CharacterSheet LoadCharacter(AssetType type, int id) => (CharacterSheet)LoadAssetCached(type, id);

        public void Dispose()
        {
            foreach (var xld in _xlds.SelectMany(x => x.Value))
                xld?.Dispose();
        }
    }
}
