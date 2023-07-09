using System;
using System.IO;
using SerdesNet;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Config.Properties;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Formats.Exporters.Tiled;

public class TiledMapLoader : Component, IAssetLoader<BaseMapData>
{
    // Isometric tileset/map properties
    public static readonly IntAssetProperty TilesPerRow             = new("TilesPerRow"); // int
    public static readonly PathPatternProperty WallPngPattern       = new("WallPngPattern"); // string
    public static readonly PathPatternProperty CeilingPngPattern    = new("CeilingPngPattern"); // string
    public static readonly PathPatternProperty ContentsPngPattern   = new("ContentsPngPattern"); // string
    public static readonly PathPatternProperty FloorPngPattern      = new("FloorPngPattern"); // string
    public static readonly IntAssetProperty TileHeight              = new("TileHeight"); // int
    public static readonly IntAssetProperty BaseHeight              = new("BaseHeight"); // int
    public static readonly IntAssetProperty TileWidth               = new("TileWidth"); // int
    public static readonly PathPatternProperty TiledCeilingPattern  = new("TiledCeilingPattern"); // string
    public static readonly PathPatternProperty TiledContentsPattern = new("TiledContentsPattern"); // string
    public static readonly PathPatternProperty TiledFloorPattern    = new("TiledFloorPattern"); // string
    public static readonly PathPatternProperty TiledWallPattern     = new("TiledWallPattern"); // string
    public static readonly StringAssetProperty LargeNpcs      = new("LargeNpcs"); // string
    public static readonly StringAssetProperty SmallNpcs      = new("SmallNpcs"); // string
    public static readonly PathPatternProperty TilesetPattern = new("TilesetPattern", "../Tilesets/{0}_{2}.tsx"); // string
    public static readonly PathPatternProperty ScriptPattern  = new("ScriptPattern"); // string

    public object Serdes(object existing, ISerializer s, AssetLoadContext context)
        => Serdes((BaseMapData) existing, s, context);

    public BaseMapData Serdes(BaseMapData existing, ISerializer s, AssetLoadContext context)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        if (context == null) throw new ArgumentNullException(nameof(context));

        if (!s.IsWriting())
            return Read(s, context);

        Write(existing, s, context);
        return existing;
    }

    static string GetScriptFilename(AssetLoadContext context)
    {
        var scriptPattern = context.GetProperty(ScriptPattern);
        return scriptPattern.Format(context.BuildAssetPath());
    }

    void Write(BaseMapData existing, ISerializer s, AssetLoadContext context)
    {
        (byte[] bytes, string script) = existing switch
        {
            MapData2D map2d => Write2D(map2d, context),
            MapData3D map3d => Write3D(map3d, context),
            _ => (null, null)
        };

        if (bytes != null)
            s.Bytes(null, bytes, bytes.Length);
        else
            Warn($"No bytes were generated when saving map {context.AssetId}");

        if (script == null)
        {
            Warn($"No script for map {context.AssetId}, aborting script output");
            return;
        }

        var scriptPath = GetScriptFilename(context);
        if (string.IsNullOrEmpty(scriptPath))
        {
            Warn($"No script path was set for map {context.AssetId}, aborting script output");
            return;
        }

        var assetDir = GetAssetDir(context);
        if (!context.Disk.DirectoryExists(assetDir))
            context.Disk.CreateDirectory(assetDir);

        context.Disk.WriteAllText(Path.Combine(assetDir, scriptPath), script);
    }

    BaseMapData Read(ISerializer s, AssetLoadContext context)
    {
        var assetDir = GetAssetDir(context);
        var scriptPath = Path.Combine(assetDir, GetScriptFilename(context));
        string script = null;
        if (!string.IsNullOrEmpty(scriptPath) && context.Disk.FileExists(scriptPath))
            script = context.Disk.ReadAllText(scriptPath);

        var bytes = s.Bytes(null, null, (int)s.BytesRemaining);
        using var ms = new MemoryStream(bytes);
        var map = Map.Parse(ms);
        return map.ToAlbion(context.AssetId, script);
    }

    (byte[], string) Write2D(MapData2D map, AssetLoadContext context)
    {
        var assets = Resolve<IAssetManager>();
        TilesetData tileset = assets.LoadTileData(map.TilesetId);
        if (tileset == null)
        {
            Error($"Tileset {map.TilesetId} not found when writing map {map.Id}, aborting");
            return (null, null);
        }

        var tilesetPattern = context.GetProperty(TilesetPattern);
        var tilesetPath = tilesetPattern.Format(new AssetPath(map.TilesetId, 0, null, ConfigUtil.AssetName(map.TilesetId)));

        var npcTilesetPath = map.MapType == MapType.TwoDOutdoors 
            ? context.GetProperty(SmallNpcs, "../Tilesets/SmallNPCs.tsx")
            : context.GetProperty(LargeNpcs, "../Tilesets/LargeNPCs.tsx");

        var assetDir = GetAssetDir(context);
        var npcTileset = Tileset.Load(Path.Combine(assetDir, npcTilesetPath), context.Disk);
        npcTileset.Filename = npcTilesetPath; // The path in the map file should be relative to the map path, not to the mod dir so replace it here.
        var properties = new Tilemap2DProperties { TileWidth = 16, TileHeight = 16 };
        var formatter = new EventFormatter(assets.LoadStringSafe, map.Id.ToMapText());
        var (tiledMap, script) = MapExport.FromAlbionMap2D(map, tileset, properties, tilesetPath, npcTileset, formatter);

        var mapBytes = FormatUtil.BytesFromTextWriter(tiledMap.Serialize);
        return (mapBytes, script);
    }

    (byte[], string) Write3D(MapData3D map, AssetLoadContext context)
    {
        var assets = Resolve<IAssetManager>();
        var destModApplier = Resolve<IModApplier>();

        var floorPattern    = context.GetProperty(TiledFloorPattern);
        var ceilingPattern  = context.GetProperty(TiledCeilingPattern);
        var wallPattern     = context.GetProperty(TiledWallPattern);
        var contentsPattern = context.GetProperty(TiledContentsPattern);

        if (floorPattern.IsEmpty || ceilingPattern.IsEmpty || wallPattern.IsEmpty || contentsPattern.IsEmpty)
            return (Array.Empty<byte>(), null);

        var labInfo = destModApplier.GetAssetInfo(map.LabDataId);
        if (labInfo == null)
        {
            Error($"Could not load asset info for lab {map.LabDataId} in map {map.Id}");
            return (Array.Empty<byte>(), null);
        }

        var assetPath = new AssetPath(map.LabDataId, 0, labInfo.PaletteId.Id);
        var properties = new Tilemap3DProperties
        {
            TileWidth    = context.GetProperty(TileWidth, 0),
            TileHeight   = context.GetProperty(BaseHeight),
            FloorPath    = floorPattern.Format(assetPath),
            CeilingPath  = ceilingPattern.Format(assetPath),
            WallPath     = wallPattern.Format(assetPath),
            ContentsPath = contentsPattern.Format(assetPath),
        };

        var formatter = new EventFormatter(assets.LoadStringSafe, map.Id.ToMapText());
        var (tiledMap, script) = MapExport.FromAlbionMap3D(map, properties, formatter);

        var mapBytes = FormatUtil.BytesFromTextWriter(tiledMap.Serialize);
        return (mapBytes, script);
    }

    string GetAssetDir(AssetLoadContext context)
    {
        var pathResolver = Resolve<IPathResolver>();
        var destPath = pathResolver.ResolvePath(context.Filename);
        return destPath;
    }
}
