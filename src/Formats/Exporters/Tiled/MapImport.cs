using System;
using System.Collections.Generic;
using UAlbion.Config;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Scripting;

namespace UAlbion.Formats.Exporters.Tiled;

public static class MapImport
{
    public static BaseMapData ToAlbion(this Map map, AssetInfo info, string script, TilesetData tileset)
    {
        if (map == null) throw new ArgumentNullException(nameof(map));
        if (info == null) throw new ArgumentNullException(nameof(info));

        // Check width/height <= 255
        if (map.Width > 255) throw new FormatException($"Map widths above 255 are not currently supported (was {map.Width})");
        if (map.Height > 255) throw new FormatException($"Map heights above 255 are not currently supported (was {map.Height})");

        bool is3d = map.Orientation == "isometric";
        var steps = new List<(string, IGraph)>();
        var eventLayout = ScriptCompiler.Compile(script, steps);

        List<TriggerInfo> triggers = new();
        List<MapNpc> npcs = new();
        List<MapEventZone> zones = new();
        ObjectGroupMapping.LoadObjectGroups(
            info,
            map,
            is3d ? map.TileHeight : map.TileWidth,
            map.TileHeight,
            eventLayout,
            triggers,
            npcs,
            zones);

        BaseMapData albionMap;
        if (is3d)
        {
            albionMap = new MapData3D(info.AssetId, (byte)map.Width, (byte)map.Height, eventLayout.Events, eventLayout.Chains, npcs, zones);
            LayerMapping3D.ReadLayers((MapData3D)albionMap, map);
        }
        else
        {
            albionMap = new MapData2D(info.AssetId, (byte)map.Width, (byte)map.Height, eventLayout.Events, eventLayout.Chains, npcs, zones)
            {
                RawLayout = LayerMapping2D.ReadLayout(map)
            };
        }

        MapMapping.ReadMapProperties(albionMap, map);
        return albionMap;
    }
}