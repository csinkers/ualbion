using System;
using System.IO;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Exporters.Tiled;

namespace UAlbion.Game.Assets;

public class TiledMapLoader : Component, IAssetLoader<BaseMapData>
{
    public object Serdes(object existing, AssetInfo info, ISerializer s, LoaderContext context)
        => Serdes((BaseMapData) existing, info, s, context);

    public BaseMapData Serdes(BaseMapData existing, AssetInfo info, ISerializer s, LoaderContext context)
    {
        if (info == null) throw new ArgumentNullException(nameof(info));
        if (s == null) throw new ArgumentNullException(nameof(s));

        if (!s.IsWriting())
            return Read(info, s);

        Write(existing, info, s, context);
        return existing;
    }

    static string GetScriptFilename(AssetInfo info)
    {
        var scriptPattern = info.GetPattern(AssetProperty.ScriptPattern, "");
        return scriptPattern.Format(info);
    }

    void Write(BaseMapData existing, AssetInfo info, ISerializer s, LoaderContext loaderContext)
    {
        (byte[] bytes, string script) = existing switch
        {
            MapData2D map2d => Write2D(map2d, info, loaderContext),
            MapData3D map3d => Write3D(map3d, info, loaderContext),
            _ => (null, null)
        };

        if (bytes != null)
            s.Bytes(null, bytes, bytes.Length);
        else
            Warn($"No bytes were generated when saving map {info.Id}");

        if (script == null)
        {
            Warn($"No script for map {info.Id}, aborting script output");
            return;
        }

        var scriptPath = GetScriptFilename(info);
        if (string.IsNullOrEmpty(scriptPath))
        {
            Warn($"No script path was set for map {info.Id}, aborting script output");
            return;
        }

        var disk = Resolve<IFileSystem>();
        var assetDir = GetAssetDir(info);
        if (!disk.DirectoryExists(assetDir))
            disk.CreateDirectory(assetDir);

        disk.WriteAllText(Path.Combine(assetDir, scriptPath), script);
    }

    BaseMapData Read(AssetInfo info, ISerializer s)
    {
        var disk = Resolve<IFileSystem>();
        var assetDir = GetAssetDir(info);
        var scriptPath = Path.Combine(assetDir, GetScriptFilename(info));
        string script = null;
        if (!string.IsNullOrEmpty(scriptPath) && disk.FileExists(scriptPath))
            script = disk.ReadAllText(scriptPath);

        var bytes = s.Bytes(null, null, (int)s.BytesRemaining);
        using var ms = new MemoryStream(bytes);
        var map = Map.Parse(ms);
        return map.ToAlbion(info, script);
    }

    (byte[], string) Write2D(MapData2D map, AssetInfo info, LoaderContext loaderContext)
    {
        var disk = Resolve<IFileSystem>();

        TilesetData tileset = loaderContext.Assets.LoadTileData(map.TilesetId);
        if (tileset == null)
        {
            Error($"Tileset {map.TilesetId} not found when writing map {map.Id}, aborting");
            return (null, null);
        }

        var tilesetPattern = info.GetPattern(AssetProperty.TilesetPattern, "../Tilesets/{0}_{2}.tsx");
        var tilesetPath = tilesetPattern.Format(new AssetPath(map.TilesetId.Id, 0, null, ConfigUtil.AssetName(map.TilesetId)));

        var npcTilesetPath = map.MapType == MapType.TwoDOutdoors 
            ? info.Get(AssetProperty.SmallNpcs, "../Tilesets/SmallNPCs.tsx")
            : info.Get(AssetProperty.LargeNpcs, "../Tilesets/LargeNPCs.tsx");

        var assetDir = GetAssetDir(info);
        var npcTileset = Tileset.Load(Path.Combine(assetDir, npcTilesetPath), disk);
        var properties = new Tilemap2DProperties { TileWidth = 16, TileHeight = 16 };
        var formatter = new EventFormatter(loaderContext.Assets.LoadString, map.Id.ToMapText());
        var (tiledMap, script) = MapExport.FromAlbionMap2D(map, tileset, properties, tilesetPath, npcTileset, formatter);

        var mapBytes = FormatUtil.BytesFromTextWriter(tiledMap.Serialize);
        return (mapBytes, script);
    }

    (byte[], string) Write3D(MapData3D map, AssetInfo info, LoaderContext loaderContext)
    {
        var destModApplier = Resolve<IModApplier>();

        var floorPattern    = info.GetPattern(AssetProperty.TiledFloorPattern,    "");
        var ceilingPattern  = info.GetPattern(AssetProperty.TiledCeilingPattern,  "");
        var wallPattern     = info.GetPattern(AssetProperty.TiledWallPattern,     "");
        var contentsPattern = info.GetPattern(AssetProperty.TiledContentsPattern, "");

        if (floorPattern.IsEmpty || ceilingPattern.IsEmpty || wallPattern.IsEmpty || contentsPattern.IsEmpty)
            return (Array.Empty<byte>(), null);

        var labInfo = destModApplier.GetAssetInfo(map.LabDataId, null);
        if (labInfo == null)
        {
            Error($"Could not load asset info for lab {map.LabDataId} in map {map.Id}");
            return (Array.Empty<byte>(), null);
        }

        var assetPath = new AssetPath(labInfo);
        var properties = new Tilemap3DProperties
        {
            TileWidth = info.Get(AssetProperty.TileWidth, 0),
            TileHeight = info.Get(AssetProperty.BaseHeight, 0),
            FloorPath    = floorPattern.Format(assetPath),
            CeilingPath  = ceilingPattern.Format(assetPath),
            WallPath     = wallPattern.Format(assetPath),
            ContentsPath = contentsPattern.Format(assetPath),
        };

        var formatter = new EventFormatter(loaderContext.Assets.LoadString, map.Id.ToMapText());
        var (tiledMap, script) = MapExport.FromAlbionMap3D(map, properties, formatter);

        var mapBytes = FormatUtil.BytesFromTextWriter(tiledMap.Serialize);
        return (mapBytes, script);
    }

    string GetAssetDir(AssetInfo info)
    {
        var config = Resolve<IGeneralConfig>();
        var destPath = config.ResolvePath(info.File.Filename);
        return destPath;
    }
}
