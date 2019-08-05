using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Textures;
using UAlbion.Formats;
using UAlbion.Game.AssetIds;

namespace UAlbion.Game
{
    public enum GameLanguage
    {
        German,
        English,
        French,
    }

    [Event("assets:reload", "Flush the asset cache, forcing all data to be reloaded from disk")]
    public class ReloadAssetsEvent : GameEvent { }

    [Event("assets:stats", "Print asset cache statistics.")]
    public class AssetStatsEvent : GameEvent { }

    public class Assets : RegisteredComponent
    {

        static readonly IList<Handler> Handlers = new Handler[]
        {
            new Handler<Assets, ReloadAssetsEvent>((x, e) => { lock (x._syncRoot) { x._assetCache.Clear(); } }),
            new Handler<Assets, AssetStatsEvent>((x, e) =>
            {
                Console.WriteLine("Asset Statistics:");
                lock (x._syncRoot)
                {
                    foreach (var key in x._assetCache.Keys.OrderBy(y => y.ToString()))
                    {
                        Console.WriteLine("    {0}: {1} items", key, x._assetCache[key].Values.Count);
                    }
                }
            }),
        };

        readonly object _syncRoot = new object();
        readonly Config _config;
        readonly IDictionary<AssetType, XldFile[]> _xlds = new Dictionary<AssetType, XldFile[]>();
        readonly IDictionary<AssetType, IDictionary<int, object>> _assetCache = new Dictionary<AssetType, IDictionary<int, object>>();

        // ReSharper disable StringLiteralTypo
        readonly IDictionary<AssetType, (AssetLocation, string)> _assetFiles = new Dictionary<AssetType, (AssetLocation, string)> {
            { AssetType.MapData,            (AssetLocation.Base,      "MAPDATA!.XLD") }, // Map2d
            { AssetType.IconData,           (AssetLocation.Base,      "ICONDAT!.XLD") }, // Texture
            { AssetType.IconGraphics,       (AssetLocation.Base,      "ICONGFX!.XLD") }, // Texture
            { AssetType.Palette,            (AssetLocation.Base,      "PALETTE!.XLD") }, // PaletteView
            { AssetType.PaletteNull,        (AssetLocation.BaseRaw,   "PALETTE.000" ) }, // PaletteView (supplementary)
            { AssetType.Slab,               (AssetLocation.Base,      "SLAB"        ) }, //
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
            { AssetType.TransparencyTables, (AssetLocation.Base,      "TRANSTB!.XLD") }
        };
        // ReSharper restore StringLiteralTypo

        string GetXldPath(AssetLocation location, GameLanguage language, string baseName, int number)
        {
            Debug.Assert(number >= 0);
            Debug.Assert(number <= 9);

            switch (location)
            {
                case AssetLocation.Base:
                    if(!baseName.Contains("!"))
                        return Path.Combine(_config.BasePath, _config.XldPath, baseName);
                    return Path.Combine(_config.BasePath, _config.XldPath, baseName.Replace("!", number.ToString()));

                case AssetLocation.Localised:
                    if (!baseName.Contains("!"))
                        return Path.Combine(_config.BasePath, _config.XldPath, language.ToString().ToUpper(), baseName);
                    return Path.Combine(_config.BasePath, _config.XldPath, language.ToString().ToUpper(), baseName.Replace("!", number.ToString()));

                case AssetLocation.Initial:
                    if (!baseName.Contains("!"))
                        return Path.Combine(_config.BasePath, _config.XldPath, "INITIAL", baseName);
                    return Path.Combine(_config.BasePath, _config.XldPath, "INITIAL", baseName.Replace("!", number.ToString()));

                case AssetLocation.Current:
                    if (!baseName.Contains("!"))
                        return Path.Combine(_config.BasePath, _config.XldPath, "CURRENT", baseName);
                    return Path.Combine(_config.BasePath, _config.XldPath, "CURRENT", baseName.Replace("!", number.ToString()));

                case AssetLocation.BaseRaw: return Path.Combine(_config.BasePath, _config.XldPath, baseName);
                case AssetLocation.LocalisedRaw: return Path.Combine(_config.BasePath, _config.XldPath, language.ToString().ToUpper(), baseName);
                default: throw new ArgumentOutOfRangeException("Invalid asset location");
            }
        }

        readonly string[] _overrideExtensions = { "bmp", "png", "wav", "json", "mp3" };

        public Assets(Config config) : base(Handlers)
        {
            _config = config;
        }

        string GetOverridePath(AssetLocation location, GameLanguage language, string baseName, int number, int objectNumber)
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

            switch (location)
            {
                case AssetLocation.Base:
                    if(!baseName.Contains("!"))
                        return Try(Path.Combine(_config.BaseDataPath, baseName, objectNumber.ToString()));
                    return Try(Path.Combine(_config.BaseDataPath, baseName.Replace("!", number.ToString()), objectNumber.ToString()));

                case AssetLocation.BaseRaw:
                    return Try(Path.Combine(_config.BaseDataPath, baseName));

                case AssetLocation.Localised:
                    if (!baseName.Contains("!"))
                        return Path.Combine(_config.BaseDataPath, language.ToString().ToUpper(), baseName, objectNumber.ToString());
                    return Path.Combine(_config.BaseDataPath, language.ToString().ToUpper(), baseName.Replace("!", number.ToString()), objectNumber.ToString());

                case AssetLocation.LocalisedRaw:
                    return Try(Path.Combine(_config.BaseDataPath, language.ToString().ToUpper(), baseName));

                case AssetLocation.Initial:
                    if (!baseName.Contains("!"))
                        return Path.Combine(_config.BaseDataPath, "INITIAL", baseName, objectNumber.ToString());
                    return Path.Combine(_config.BaseDataPath, "INITIAL", baseName.Replace("!", number.ToString()), objectNumber.ToString());

                case AssetLocation.Current:
                    if (!baseName.Contains("!"))
                        return Path.Combine(_config.BaseDataPath, "CURRENT", baseName, objectNumber.ToString());
                    return Path.Combine(_config.BaseDataPath, "CURRENT", baseName.Replace("!", number.ToString()), objectNumber.ToString());

                default: throw new ArgumentOutOfRangeException("Invalid asset location");
            }
        }

        object LoadAsset(AssetType type, int id, string name) => LoadAsset(type, id, name, GameLanguage.English, null);
        object LoadAsset(AssetType type, int id, string name, GameLanguage language, object context)
        {
            int xldIndex = id / 1000;
            Debug.Assert(xldIndex >= 0);
            Debug.Assert(xldIndex <= 9);
            int objectIndex = id % 1000;
            var (location, baseName) = _assetFiles[type];

            var overrideFilename = GetOverridePath(location, language, baseName, xldIndex, objectIndex);
            if (overrideFilename != null || IsLocationRaw(location))
            {
                var path = overrideFilename ?? GetXldPath(location, language, baseName, id);
                using(var stream = File.OpenRead(path))
                using (var br = new BinaryReader(stream))
                {
                    var asset = AssetLoader.Load(br, type, id, name, (int)stream.Length, context);
                    if(asset == null)
                        throw new InvalidOperationException($"Object {type}:{id} could not be loaded from file {path}");

                    return asset;
                }
            }

            if (!_xlds.ContainsKey(type))
            {
                _xlds[type] = new XldFile[10];
                for (int i = 0; i < (baseName.Contains("!") ? 10 : 1); i++)
                {
                    var filename = GetXldPath(location, language, baseName, i);
                    if(File.Exists(filename))
                        _xlds[type][i] = new XldFile(filename);
                }
            }

            var xldArray = _xlds[type];
            var xld = xldArray[xldIndex];
            if (xld == null)
                throw new InvalidOperationException($"XLD not found for object: {type}:{id} in {baseName} ({location})");

            using (var br = xld.GetReaderForObject(objectIndex, out var length))
            {
                if (length == 0)
                    return null;

                var asset = AssetLoader.Load(br, type, id, name, length, context);
                if (asset == null)
                    throw new InvalidOperationException($"Object {type}:{id} could not be loaded from XLD {xld.Filename}");

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

        object LoadAssetCached(AssetType type, int id, string name, GameLanguage language) { return LoadAssetCached(type, id, name, language, null); }
        object LoadAssetCached(AssetType type, int id, string name, object context = null) { return LoadAssetCached(type, id, name, GameLanguage.English, context); }
        object LoadAssetCached(AssetType type, int id, string name, GameLanguage language, object context)
        {
            lock(_syncRoot)
            {
                if (_assetCache.TryGetValue(type, out var typeCache))
                {
                    if (typeCache.TryGetValue(id, out var cachedAsset))
                        return cachedAsset;
                }
                else _assetCache[type] = new Dictionary<int, object>();
            }

            var newAsset = LoadAsset(type, id, name, language, context);

            lock (_syncRoot)
            {
                _assetCache[type][id] = newAsset;
                return newAsset;
            }
        }

        public Map2D LoadMap2D(MapDataId id) { return (Map2D)LoadAssetCached(AssetType.MapData, (int)id, $"Map2D:{id}"); }
        public Map3D LoadMap3D(MapDataId id) { return (Map3D)LoadAssetCached(AssetType.MapData, (int)id, $"Map3D:{id}"); }
        public AlbionPalette LoadPalette(PaletteId id)
        {
            var commonPalette = (byte[])LoadAssetCached(AssetType.PaletteNull, 0, $"Pal_Common");
            return (AlbionPalette)LoadAssetCached(AssetType.Palette, (int)id, $"Pal:{id}", new AlbionPalette.PaletteContext((int)id, commonPalette));
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
                default: return (ITexture)LoadAssetCached(type, id, $"{type}:{id}", GameLanguage.English, null);
            }
        }

        public ITexture LoadTexture(AutoMapId id)            => (ITexture)LoadAssetCached(AssetType.AutomapGraphics,    (int)id, $"AutomapGraphics:{id}");
        public ITexture LoadTexture(CombatBackgroundId id)   => (ITexture)LoadAssetCached(AssetType.CombatBackground,   (int)id, $"CombatBackground:{id}");
        public ITexture LoadTexture(CombatGraphicsId id)     => (ITexture)LoadAssetCached(AssetType.CombatGraphics,     (int)id, $"CombatGraphics:{id}");
        public ITexture LoadTexture(DungeonBackgroundId id)  => (ITexture)LoadAssetCached(AssetType.BackgroundGraphics, (int)id, $"BackgroundGraphics:{id}");
        public ITexture LoadTexture(DungeonFloorId id)       => (ITexture)LoadAssetCached(AssetType.Floor3D,            (int)id, $"Floor3D:{id}");
        public ITexture LoadTexture(DungeonObjectId id)      => (ITexture)LoadAssetCached(AssetType.Object3D,           (int)id, $"Object3D:{id}");
        public ITexture LoadTexture(DungeonOverlayId id)     => (ITexture)LoadAssetCached(AssetType.Overlay3D,          (int)id, $"Overlay3D:{id}");
        public ITexture LoadTexture(DungeonWallId id)        => (ITexture)LoadAssetCached(AssetType.Wall3D,             (int)id, $"Wall3D:{id}");
        public ITexture LoadTexture(FullBodyPictureId id)    => (ITexture)LoadAssetCached(AssetType.FullBodyPicture,    (int)id, $"FullBodyPicture:{id}");
        public ITexture LoadTexture(IconDataId id)           => (ITexture)LoadAssetCached(AssetType.IconData,           (int)id, $"IconData:{id}");
        public ITexture LoadTexture(IconGraphicsId id)       => (ITexture)LoadAssetCached(AssetType.IconGraphics,       (int)id, $"IconGraphics:{id}");
        public ITexture LoadTexture(ItemId id)               => (ITexture)LoadAssetCached(AssetType.ItemGraphics,       (int)id, $"ItemGraphics:{id}"); // TODO: Enum
        public ITexture LoadTexture(LargeNpcId id)           => (ITexture)LoadAssetCached(AssetType.BigNpcGraphics,     (int)id, $"BigNpcGraphics:{id}");
        public ITexture LoadTexture(LargePartyGraphicsId id) => (ITexture)LoadAssetCached(AssetType.BigPartyGraphics,   (int)id, $"BigPartyGraphics:{id}");
        public ITexture LoadTexture(MonsterGraphicsId id)    => (ITexture)LoadAssetCached(AssetType.MonsterGraphics,    (int)id, $"MonsterGraphics:{id}");
        public ITexture LoadTexture(PictureId id)            => (ITexture)LoadAssetCached(AssetType.Picture,            (int)id, $"Picture:{id}");
        public ITexture LoadTexture(SmallNpcId id)           => (ITexture)LoadAssetCached(AssetType.SmallNpcGraphics,   (int)id, $"SmallNpcGraphics:{id}");
        public ITexture LoadTexture(SmallPartyGraphicsId id) => (ITexture)LoadAssetCached(AssetType.SmallPartyGraphics, (int)id, $"SmallPartyGraphics:{id}");
        public ITexture LoadTexture(SmallPortraitId id)      => (ITexture)LoadAssetCached(AssetType.SmallPortrait,      (int)id, $"SmallPortrait:{id}");
        public ITexture LoadTexture(TacticId id)             => (ITexture)LoadAssetCached(AssetType.TacticalIcon,       (int)id, $"TacticalIcon:{id}");

        public string LoadString(AssetType type, int id, GameLanguage language) { return (string)LoadAssetCached(AssetType.MapData, id, $"String:{id}", language); }
        public AlbionSample LoadSample(AssetType type, int id) { return (AlbionSample)LoadAssetCached(type, id, $"Sample:{id}"); }
        public AlbionFont LoadFont(FontId id) { return (AlbionFont)LoadAssetCached(AssetType.Font, (int)id, $"Font:{id}"); }
        public AlbionVideo LoadVideo(VideoId id, GameLanguage language) => (AlbionVideo) LoadAsset(AssetType.Flic, (int)id, $"Video:{id}", language, null); // Don't cache videos.
    }
}
