using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UAlbion.Core;
using UAlbion.Core.Textures;
using UAlbion.Formats;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Config;
using UAlbion.Formats.Parsers;

namespace UAlbion.Game
{
    public class Assets : RegisteredComponent, IDisposable
    {
        public Assets(AssetConfig assetConfig, CoreSpriteConfig coreSpriteConfig) : base(Handlers)
        {
            _assetConfig = assetConfig;
            _coreSpriteConfig = coreSpriteConfig;
            _assetCache = new AssetCache();
        }

        static readonly IList<Handler> Handlers = new Handler[] { };

        readonly AssetConfig _assetConfig;
        readonly CoreSpriteConfig _coreSpriteConfig;
        readonly IDictionary<AssetType, XldFile[]> _xlds = new Dictionary<AssetType, XldFile[]>();

        // ReSharper disable StringLiteralTypo
        readonly IDictionary<AssetType, (AssetLocation, string)> _assetFiles = new Dictionary<AssetType, (AssetLocation, string)> {
            { AssetType.MapData,            (AssetLocation.Base,      "MAPDATA!.XLD") }, // Map2d
            { AssetType.IconData,           (AssetLocation.Base,      "ICONDAT!.XLD") }, // Texture
            { AssetType.IconGraphics,       (AssetLocation.Base,      "ICONGFX!.XLD") }, // Texture
            { AssetType.Palette,            (AssetLocation.Base,      "PALETTE!.XLD") }, // PaletteView
            { AssetType.PaletteNull,        (AssetLocation.BaseRaw,   "PALETTE.000" ) }, // PaletteView (supplementary)
            { AssetType.Slab,               (AssetLocation.BaseRaw,   "SLAB"        ) }, // Texture
            { AssetType.BigPartyGraphics,   (AssetLocation.Base,      "PARTGR!.XLD" ) }, // Texture
            { AssetType.SmallPartyGraphics, (AssetLocation.Base,      "PARTKL!.XLD" ) }, // Texture
            { AssetType.LabData,            (AssetLocation.Base,      "LABDATA!.XLD") }, //
            { AssetType.Wall3D,             (AssetLocation.Base,      "3DWALLS!.XLD") }, // Texture
            { AssetType.Object3D,           (AssetLocation.Base,      "3DOBJEC!.XLD") }, // Texture
            { AssetType.Overlay3D,          (AssetLocation.Base,      "3DOVERL!.XLD") }, // Texture
            { AssetType.Floor3D,            (AssetLocation.Base,      "3DFLOOR!.XLD") }, // Texture
            { AssetType.BigNpcGraphics,     (AssetLocation.Base,      "NPCGR!.XLD"  ) }, // Texture
            { AssetType.BackgroundGraphics, (AssetLocation.Base,      "3DBCKGR!.XLD") }, // Texture
            { AssetType.Font,               (AssetLocation.Base,      "FONTS!.XLD"  ) }, // Font (raw, 8 wide. 00 = normal, 01 = bold)
            { AssetType.BlockList,          (AssetLocation.Base,      "BLKLIST!.XLD") }, //
            { AssetType.PartyCharacterData, (AssetLocation.Initial,   "PRTCHAR!.XLD") }, //
            { AssetType.SmallPortrait,      (AssetLocation.Base,      "SMLPORT!.XLD") }, // Texture
            { AssetType.SystemTexts,        (AssetLocation.Localised, "SYSTEXTS"    ) }, // Strings
            { AssetType.EventSet,           (AssetLocation.Base,      "EVNTSET!.XLD") }, //
            { AssetType.EventTexts,         (AssetLocation.Localised, "EVNTTXT!.XLD") }, // Strings
            { AssetType.MapTexts,           (AssetLocation.Localised, "MAPTEXT!.XLD") }, // Strings
            { AssetType.ItemList,           (AssetLocation.Base,      "ITEMLIST.DAT") }, //
            { AssetType.ItemNames,          (AssetLocation.Base,      "ITEMNAME.DAT") }, // Strings
            { AssetType.ItemGraphics,       (AssetLocation.BaseRaw,   "ITEMGFX"     ) }, // Texture
            { AssetType.FullBodyPicture,    (AssetLocation.Base,      "FBODPIX!.XLD") }, // Texture
            { AssetType.Automap,            (AssetLocation.Initial,   "AUTOMAP!.XLD") }, //
            { AssetType.AutomapGraphics,    (AssetLocation.Base,      "AUTOGFX!.XLD") }, // Texture
            { AssetType.Song,               (AssetLocation.Base,      "SONGS!.XLD"  ) }, // Midi
            { AssetType.Sample,             (AssetLocation.Base,      "SAMPLES!.XLD") }, // Sample
            { AssetType.WaveLibrary,        (AssetLocation.Base,      "WAVELIB!.XLD") }, // Sample
            // { AssetType.Unnamed2,        (AssetLocation.Base,      ""            ) },
            { AssetType.ChestData,          (AssetLocation.Initial,   "CHESTDT!.XLD") }, //
            { AssetType.MerchantData,       (AssetLocation.Initial,   "MERCHDT!.XLD") }, //
            { AssetType.NpcCharacterData,   (AssetLocation.Initial,   "NPCCHAR!.XLD") }, //
            { AssetType.MonsterGroup,       (AssetLocation.Base,      "MONGRP!.XLD" ) }, //
            { AssetType.MonsterCharacter,   (AssetLocation.Base,      "MONCHAR!.XLD") }, // Texture
            { AssetType.MonsterGraphics,    (AssetLocation.Base,      "MONGFX!.XLD" ) }, // Texture
            { AssetType.CombatBackground,   (AssetLocation.Base,      "COMBACK!.XLD") }, // Texture
            { AssetType.CombatGraphics,     (AssetLocation.Base,      "COMGFX!.XLD" ) }, // Texture
            { AssetType.TacticalIcon,       (AssetLocation.Base,      "TACTICO!.XLD") }, // Texture
            { AssetType.SpellData,          (AssetLocation.Base,      "SPELLDAT.DAT") }, // Spell
            { AssetType.SmallNpcGraphics,   (AssetLocation.Base,      "NPCKL!.XLD"  ) }, // Texture
            { AssetType.Flic,               (AssetLocation.Localised, "FLICS!.XLD"  ) }, // Video
            { AssetType.Dictionary,         (AssetLocation.Localised, "WORDLIS!.XLD") }, // Dictionary
            { AssetType.Script,             (AssetLocation.Base,      "SCRIPT!.XLD" ) }, // Script
            { AssetType.Picture,            (AssetLocation.Base,      "PICTURE!.XLD") }, // Texture (ILBM)
            { AssetType.TransparencyTables, (AssetLocation.Base,      "TRANSTB!.XLD") },
            { AssetType.CoreGraphics,       (AssetLocation.MainExe,   "MAIN.EXE") },
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
                    result.OverridePath = Path.Combine(_assetConfig.BaseDataPath, lang, baseName, objectNumber.ToString());
                    result.XldNameInConfig = "$(LANG)\\" + baseName;
                    break;

                case AssetLocation.LocalisedRaw:
                    result.XldPath = Path.Combine(_assetConfig.BasePath, _assetConfig.XldPath, lang, baseName);
                    result.OverridePath = Try(Path.Combine(_assetConfig.BaseDataPath, lang, baseName));
                    result.XldNameInConfig = "$(LANG)\\" + baseName;
                    break;

                case AssetLocation.Initial:
                    result.XldPath = Path.Combine(_assetConfig.BasePath, _assetConfig.XldPath, "INITIAL", baseName);
                    result.OverridePath = Path.Combine(_assetConfig.BaseDataPath, "INITIAL", baseName, objectNumber.ToString());
                    result.XldNameInConfig = "INITIAL\\" + baseName;
                    break;

                case AssetLocation.Current:
                    result.XldPath = Path.Combine(_assetConfig.BasePath, _assetConfig.XldPath, "CURRENT", baseName);
                    result.OverridePath = Path.Combine(_assetConfig.BaseDataPath, "CURRENT", baseName, objectNumber.ToString());
                    result.XldNameInConfig = "INITIAL\\" + baseName; // Note: Use the same metadata for CURRENT & INITIAL
                    break;

                default: throw new ArgumentOutOfRangeException("Invalid asset location");
            }

            return result;
        }

        object LoadAsset(AssetType type, int id, string name, GameLanguage language)
        {
            var (location, baseName) = _assetFiles[type];
            if (location == AssetLocation.MainExe || type == AssetType.CoreGraphics)
                return AssetLoader.LoadCoreSprite((CoreSpriteId)id, _assetConfig.BasePath, _coreSpriteConfig);

            int xldIndex = id / 100;
            Debug.Assert(xldIndex >= 0);
            Debug.Assert(xldIndex <= 9);
            int objectIndex = id % 100;

            var paths = GetAssetPaths(location, language, baseName, xldIndex, objectIndex);
            var xldConfig = _assetConfig.Xlds[paths.XldNameInConfig];
            xldConfig.Assets.TryGetValue(objectIndex, out var assetConfig);

            if (paths.OverridePath != null || IsLocationRaw(location))
            {
                var path = paths.OverridePath ?? paths.XldPath;
                using(var stream = File.OpenRead(path))
                using (var br = new BinaryReader(stream))
                {
                    var asset = AssetLoader.Load(br, name, (int)stream.Length, assetConfig);
                    if(asset == null)
                        throw new AssetNotFoundException($"Object {type}:{id} could not be loaded from file {path}", type, id);
                    GameTrace.Log.AssetLoaded(type, id, name, language, path);

                    return asset;
                }
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
            int id = (int)(object)enumId;
            object asset = _assetCache.Get(type, id);
            if (asset is Exception _) // If it failed to load once then stop trying (at least until an asset:reload / cycle)
                return null;

            if (asset != null)
                return asset;

            var name = $"{type}.{enumId}";
            try
            {
                asset = LoadAsset(type, id, name, language);
            }
            catch(Exception e)
            {
                Raise(new LogEvent((int)LogEvent.Level.Error, $"Could not load asset {name}: {e}"));
                asset = e;
            }

            _assetCache.Add(asset, type, id);
            return asset is Exception ? null : asset;
        }

        public MapData2D LoadMap2D(MapDataId id) { return LoadAssetCached(AssetType.MapData, id) as MapData2D; }
        public MapData3D LoadMap3D(MapDataId id) { return LoadAssetCached(AssetType.MapData, id) as MapData3D; }
        public AlbionPalette LoadPalette(PaletteId id)
        {
            var palette = (AlbionPalette)LoadAssetCached(AssetType.Palette, id);
            if (palette != null)
            {
                var commonPalette = (byte[]) LoadAssetCached(AssetType.PaletteNull, 0);
                palette.SetCommonPalette(commonPalette);
            }

            return palette;
        }

        public ITexture LoadTexture(AssetType type, int id)
        {
            switch (type)
            {
                case AssetType.AutomapGraphics:    return LoadTexture((AutoMapId)id);
                case AssetType.BackgroundGraphics: return LoadTexture((DungeonBackgroundId)id);
                case AssetType.BigNpcGraphics:     return LoadTexture((LargeNpcId)id);
                case AssetType.BigPartyGraphics:   return LoadTexture((LargePartyGraphicsId)id);
                case AssetType.CombatBackground:   return LoadTexture((CombatBackgroundId)id);
                case AssetType.CombatGraphics:     return LoadTexture((CombatGraphicsId)id);
                case AssetType.Floor3D:            return LoadTexture((DungeonFloorId)id);
                case AssetType.Font:               return LoadTexture((FontId)id);
                case AssetType.FullBodyPicture:    return LoadTexture((FullBodyPictureId)id);
                case AssetType.IconData:           return LoadTexture((IconDataId)id);
                case AssetType.IconGraphics:       return LoadTexture((IconGraphicsId)id);
                case AssetType.ItemGraphics:       return LoadTexture((ItemId)id);
                case AssetType.MonsterGraphics:    return LoadTexture((MonsterGraphicsId)id);
                case AssetType.Object3D:           return LoadTexture((DungeonObjectId)id);
                case AssetType.Overlay3D:          return LoadTexture((DungeonOverlayId)id);
                case AssetType.Picture:            return LoadTexture((PictureId) id);
                case AssetType.SmallNpcGraphics:   return LoadTexture((SmallNpcId)id);
                case AssetType.SmallPartyGraphics: return LoadTexture((SmallPartyGraphicsId)id);
                case AssetType.SmallPortrait:      return LoadTexture((SmallPortraitId)id);
                case AssetType.TacticalIcon:       return LoadTexture((TacticId)id);
                case AssetType.Wall3D:             return LoadTexture((DungeonWallId)id);
                case AssetType.CoreGraphics:       return LoadTexture((CoreSpriteId)id);
                case AssetType.Slab:               return (ITexture)LoadAssetCached(AssetType.Slab, 0);
                default: return (ITexture)LoadAssetCached(type, id);
            }
        }

        public ITexture LoadTexture(AutoMapId id)            => (ITexture)LoadAssetCached(AssetType.AutomapGraphics,    id);
        public ITexture LoadTexture(CombatBackgroundId id)   => (ITexture)LoadAssetCached(AssetType.CombatBackground,   id);
        public ITexture LoadTexture(CombatGraphicsId id)     => (ITexture)LoadAssetCached(AssetType.CombatGraphics,     id);
        public ITexture LoadTexture(DungeonBackgroundId id)  => (ITexture)LoadAssetCached(AssetType.BackgroundGraphics, id);
        public ITexture LoadTexture(DungeonFloorId id)       => (ITexture)LoadAssetCached(AssetType.Floor3D,            id);
        public ITexture LoadTexture(DungeonObjectId id)      => (ITexture)LoadAssetCached(AssetType.Object3D,           id);
        public ITexture LoadTexture(DungeonOverlayId id)     => (ITexture)LoadAssetCached(AssetType.Overlay3D,          id);
        public ITexture LoadTexture(DungeonWallId id)        => (ITexture)LoadAssetCached(AssetType.Wall3D,             id);
        public ITexture LoadTexture(FullBodyPictureId id)    => (ITexture)LoadAssetCached(AssetType.FullBodyPicture,    id);
        public ITexture LoadTexture(IconDataId id)           => (ITexture)LoadAssetCached(AssetType.IconData,           id);
        public ITexture LoadTexture(IconGraphicsId id)       => (ITexture)LoadAssetCached(AssetType.IconGraphics,       id);
        public ITexture LoadTexture(ItemId id)               => (ITexture)LoadAssetCached(AssetType.ItemGraphics,       id); // TODO: Enum
        public ITexture LoadTexture(LargeNpcId id)           => (ITexture)LoadAssetCached(AssetType.BigNpcGraphics,     id);
        public ITexture LoadTexture(LargePartyGraphicsId id) => (ITexture)LoadAssetCached(AssetType.BigPartyGraphics,   id);
        public ITexture LoadTexture(MonsterGraphicsId id)    => (ITexture)LoadAssetCached(AssetType.MonsterGraphics,    id);
        public ITexture LoadTexture(PictureId id)            => (ITexture)LoadAssetCached(AssetType.Picture,            id);
        public ITexture LoadTexture(SmallNpcId id)           => (ITexture)LoadAssetCached(AssetType.SmallNpcGraphics,   id);
        public ITexture LoadTexture(SmallPartyGraphicsId id) => (ITexture)LoadAssetCached(AssetType.SmallPartyGraphics, id);
        public ITexture LoadTexture(SmallPortraitId id)      => (ITexture)LoadAssetCached(AssetType.SmallPortrait,      id);
        public ITexture LoadTexture(TacticId id)             => (ITexture)LoadAssetCached(AssetType.TacticalIcon,       id);
        public ITexture LoadTexture(FontId id)               => (ITexture)LoadAssetCached(AssetType.Font,               id);
        public ITexture LoadTexture(CoreSpriteId id)         => (ITexture)LoadAssetCached(AssetType.CoreGraphics,       id);
        public TilesetData LoadTileData(IconDataId id)       => (TilesetData)LoadAssetCached(AssetType.IconData,        id);
        public LabyrinthData LoadLabyrinthData(LabyrinthDataId id) => (LabyrinthData) LoadAssetCached(AssetType.LabData, id);

        public string LoadString(AssetType type, int id, GameLanguage language, int subItem)
        {
            var stringTable = (IDictionary<int, string>)LoadAssetCached(AssetType.MapData, id, language);
            return stringTable[subItem];
        }
        public AlbionSample LoadSample(AssetType type, int id) { return (AlbionSample)LoadAssetCached(type, id); }
        public AlbionVideo LoadVideo(VideoId id, GameLanguage language) => (AlbionVideo) LoadAsset(AssetType.Flic, (int)id, $"Video:{id}", language); // Don't cache videos.

        public void Dispose()
        {
            foreach(var xld in _xlds.SelectMany(x => x.Value))
                xld?.Dispose();
        }
    }
}
