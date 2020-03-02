using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UAlbion.Api;
using UAlbion.Formats;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Config;

namespace UAlbion.Game.Assets
{
    public class StandardAssetLocator : IAssetLocator, IDisposable
    {
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
            { AssetType.Tileset,           (AssetLocation.Base,      "ICONDAT!.XLD") }, // Tileset info for the 2D maps, including animated tile ranges etc
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
            { AssetType.PartyMember,        (AssetLocation.Initial,   "PRTCHAR!.XLD") }, // Character statistics for party members
            { AssetType.Npc,                (AssetLocation.Initial,   "NPCCHAR!.XLD") }, // NPC character statistics
            { AssetType.Monster,            (AssetLocation.Base,      "MONCHAR!.XLD") }, // Monster character statistics
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
        readonly IDictionary<AssetType, XldFile[]> _xlds = new Dictionary<AssetType, XldFile[]>();
        readonly object _syncRoot = new object();

        AssetPaths GetAssetPaths(IGeneralConfig config, AssetLocation location, GameLanguage language, string baseName, int number, int objectNumber)
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

            ApiUtil.Assert(number >= 0);
            ApiUtil.Assert(number <= 9);

            var result = new AssetPaths();
            baseName = baseName.Replace("!", number.ToString());
            var lang = language.ToString().ToUpper();
            switch (location)
            {
                case AssetLocation.Base:
                    result.XldPath = Path.Combine(config.BasePath, config.XldPath, baseName);
                    result.OverridePath = Try(Path.Combine(config.BaseDataPath, baseName, objectNumber.ToString()));
                    result.XldNameInConfig = baseName;
                    break;

                case AssetLocation.BaseRaw:
                    result.XldPath = Path.Combine(config.BasePath, config.XldPath, baseName);
                    result.OverridePath = Try(Path.Combine(config.BaseDataPath, baseName));
                    result.XldNameInConfig = baseName;
                    break;

                case AssetLocation.Localised:
                    result.XldPath = Path.Combine(config.BasePath, config.XldPath, lang, baseName);
                    result.OverridePath = Try(Path.Combine(config.BaseDataPath, lang, baseName, objectNumber.ToString()));
                    result.XldNameInConfig = "$(LANG)/" + baseName;
                    break;

                case AssetLocation.LocalisedRaw:
                    result.XldPath = Path.Combine(config.BasePath, config.XldPath, lang, baseName);
                    result.OverridePath = Try(Path.Combine(config.BaseDataPath, lang, baseName));
                    result.XldNameInConfig = "$(LANG)/" + baseName;
                    break;

                case AssetLocation.Initial:
                    result.XldPath = Path.Combine(config.BasePath, config.XldPath, "INITIAL", baseName);
                    result.OverridePath = Try(Path.Combine(config.BaseDataPath, "INITIAL", baseName, objectNumber.ToString()));
                    result.XldNameInConfig = "INITIAL/" + baseName;
                    break;

                case AssetLocation.Current:
                    result.XldPath = Path.Combine(config.BasePath, config.XldPath, "CURRENT", baseName);
                    result.OverridePath = Try(Path.Combine(config.BaseDataPath, "CURRENT", baseName, objectNumber.ToString()));
                    result.XldNameInConfig = "INITIAL/" + baseName; // Note: Use the same metadata for CURRENT & INITIAL
                    break;

                default: throw new ArgumentOutOfRangeException("Invalid asset location");
            }

            return result;
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

        object ReadFromFile(string path, Func<string, BinaryReader, long, object> readFunc)
        {
            using var stream = File.OpenRead(path);
            using var br = new BinaryReader(stream);
            return readFunc(path, br, stream.Length);
        }

        object ReadFromXld(AssetPaths paths, AssetKey key, Func<string, BinaryReader, long, object> readFunc)
        {
            lock (_syncRoot)
            {
                int xldIndex = key.Id / 100;
                int objectIndex = key.Id % 100;
                if (!_xlds.ContainsKey(key.Type))
                    _xlds[key.Type] = new XldFile[10];

                if (File.Exists(paths.XldPath) && _xlds[key.Type][xldIndex] == null)
                    _xlds[key.Type][xldIndex] = new XldFile(paths.XldPath);

                var xldArray = _xlds[key.Type];
                var xld = xldArray[xldIndex];
                if (xld == null)
                    throw new AssetNotFoundException(
                        $"XLD not found for object: {key.Type}:{key.Id} in {paths.XldPath}", key.Type, key.Id);

                using var br = xld.GetReaderForObject(objectIndex, out var length);
                if (length == 0)
                    return null;
                return readFunc(paths.XldPath, br, length);
            }
        }

        public object LoadAsset(AssetKey key, string name, Func<AssetKey, string, object> loaderFunc)
        {
            var basicAssetConfig = (IAssetConfig)loaderFunc(new AssetKey(AssetType.AssetConfig), "AssetConfig");
            var generalConfig = (IGeneralConfig)loaderFunc(new AssetKey(AssetType.GeneralConfig), "GeneralConfig");

            int xldIndex = key.Id / 100;
            ApiUtil.Assert(xldIndex >= 0);
            ApiUtil.Assert(xldIndex <= 9);
            int objectIndex = key.Id % 100;

            var (location, baseName) = _assetFiles[key.Type];
            var paths = GetAssetPaths(generalConfig, location, key.Language, baseName, xldIndex, objectIndex);
            var assetConfig = basicAssetConfig.GetAsset(paths.XldNameInConfig, objectIndex);

            object Reader(string path, BinaryReader br, long length)
            {
                var loader = AssetLoaderRegistry.GetLoader(assetConfig.Format);
                var asset = loader.Load(br, (int)length, name, assetConfig);
                if (asset == null) throw new AssetNotFoundException($"Object {key.Type}:{key.Id} could not be loaded from file {path}", key.Type, key.Id);
                GameTrace.Log.AssetLoaded(key, name, path);
                return asset;
            }

            return paths.OverridePath != null || IsLocationRaw(location)
                ? ReadFromFile(paths.OverridePath ?? paths.XldPath, Reader)
                : ReadFromXld(paths, key, Reader);
        }

        public void Dispose()
        {
            foreach (var xld in _xlds.SelectMany(x => x.Value))
                xld?.Dispose();
        }
    }
}
