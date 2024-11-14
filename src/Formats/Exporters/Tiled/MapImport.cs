using System;
using System.Collections.Generic;
using UAlbion.Config;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Formats.Exporters.Tiled;

public static class MapImport
{
    public static BaseMapData ToAlbion(this Map map, AssetId assetId, string script)
    {
        ArgumentNullException.ThrowIfNull(map);

        // Check width/height <= 255
        if (map.Width > 256) throw new FormatException($"Map widths above 256 are not currently supported (was {map.Width})");
        if (map.Height > 256) throw new FormatException($"Map heights above 256 are not currently supported (was {map.Height})");

        bool is3d = map.Orientation == "isometric";
        var eventLayout = AlbionCompiler.Compile(script);

        List<TriggerInfo> triggers = [];
        List<MapNpc> npcs = [];
        List<MapEventZone> zones = [];
        List<AutomapInfo> markers = [];
        List<byte> markerTiles = [];
        ObjectGroupMapping.LoadObjectGroups(
            assetId,
            map,
            is3d ? map.TileHeight : map.TileWidth,
            map.TileHeight,
            eventLayout, triggers, npcs, zones,
            markers, markerTiles);

        var paletteId = MapperUtil.PropId(map, MapMapping.Prop.Palette, true);

        BaseMapData albionMap;
        if (is3d)
        {
            var labId = MapperUtil.PropId(map, MapMapping.Prop.Labyrinth, true);
            var map3d = new MapData3D(assetId, paletteId, labId, (byte)map.Width, (byte)map.Height, eventLayout.Events, eventLayout.Chains, npcs, zones);
            LayerMapping3D.ReadLayers(map3d, map.Layers);

            for (int i = 0; i < markerTiles.Count; i++)
                map3d.AutomapGraphics[i] = markerTiles[i];

            map3d.Automap.Clear();
            map3d.Automap.AddRange(markers);

            albionMap = map3d;
        }
        else
        {
            var tilesetId = MapperUtil.PropId(map, MapMapping.Prop.Tileset, true);
            var tiles = LayerMapping2D.ReadMapLayers(map);
            var albionMap2d = new MapData2D(assetId, paletteId, tilesetId, map.Width, map.Height, eventLayout.Events, eventLayout.Chains, npcs, zones);
            Array.Copy(tiles, albionMap2d.Tiles, albionMap2d.Tiles.Length);
            albionMap = albionMap2d;
        }

        MapMapping.ReadMapProperties(albionMap, map);
        return albionMap;
    }
}