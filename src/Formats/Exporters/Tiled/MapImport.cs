using System;
using System.Collections.Generic;
using UAlbion.Config;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Formats.Exporters.Tiled;

public static class MapImport
{
    public static BaseMapData ToAlbion(this Map map, AssetInfo info, string script)
    {
        if (map == null) throw new ArgumentNullException(nameof(map));
        if (info == null) throw new ArgumentNullException(nameof(info));

        // Check width/height <= 255
        if (map.Width > 255) throw new FormatException($"Map widths above 255 are not currently supported (was {map.Width})");
        if (map.Height > 255) throw new FormatException($"Map heights above 255 are not currently supported (was {map.Height})");

        bool is3d = map.Orientation == "isometric";
        var mapId = (MapId)info.AssetId;
        var eventLayout = AlbionCompiler.Compile(script, mapId.ToMapText());

        List<TriggerInfo> triggers = new();
        List<MapNpc> npcs = new();
        List<MapEventZone> zones = new();
        List<AutomapInfo> markers = new();
        List<byte> markerTiles = new();
        ObjectGroupMapping.LoadObjectGroups(
            info, map,
            is3d ? map.TileHeight : map.TileWidth,
            map.TileHeight,
            eventLayout, triggers, npcs, zones,
            markers, markerTiles);

        var paletteId = MapperUtil.PropId(map, MapMapping.Prop.Palette, true);

        BaseMapData albionMap;
        if (is3d)
        {
            var labId = MapperUtil.PropId(map, MapMapping.Prop.Labyrinth, true);
            var map3d = new MapData3D(info.AssetId, paletteId, labId, (byte)map.Width, (byte)map.Height, eventLayout.Events, eventLayout.Chains, npcs, zones);
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
            albionMap = new MapData2D(info.AssetId, paletteId, tilesetId, (byte)map.Width, (byte)map.Height, eventLayout.Events, eventLayout.Chains, npcs, zones)
            {
                RawLayout = LayerMapping2D.ReadLayout(map)
            };
        }

        MapMapping.ReadMapProperties(albionMap, map);
        return albionMap;
    }
}