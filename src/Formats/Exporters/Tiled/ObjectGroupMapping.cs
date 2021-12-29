using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Config;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Scripting;

namespace UAlbion.Formats.Exporters.Tiled;

public static class ObjectGroupMapping
{
    public static class TypeName
    {
        public const string Marker = "Marker";
        public const string Npc = "NPC";
        public const string Trigger = "Trigger";
    }

    public static List<ObjectGroup> BuildObjectGroups(
        BaseMapData map,
        int tileWidth,
        int tileHeight,
        NpcMapping.GetTileFunc getTileFunc,
        Dictionary<ushort, string> functionsByEventId,
        ref int nextObjectGroupId,
        ref int nextObjectId)
    {
        var results = new[]
        {
            TriggerMapping.BuildTriggers(map, tileWidth, tileHeight, functionsByEventId, ref nextObjectGroupId, ref nextObjectId),
            NpcMapping.BuildNpcs(map, tileWidth, tileHeight, getTileFunc, functionsByEventId, ref nextObjectGroupId, ref nextObjectId),
        }.SelectMany(x => x);

        if (map is MapData3D map3d)
            results = results.Concat(AutomapMapping.BuildMarkers(map3d, tileWidth, tileHeight, ref nextObjectGroupId, ref nextObjectId));

        return results.ToList();
    }


    public static void LoadObjectGroups(AssetInfo info,
        Map map,
        int tileWidth,
        int tileHeight,
        EventLayout eventLayout,
        List<TriggerInfo> triggers,
        List<MapNpc> npcs,
        List<MapEventZone> zones,
        List<AutomapInfo> markers,
        List<byte> markerTiles)
    {
        ushort ResolveEntryPoint(string name)
        {
            var (isChain, id) = ScriptConstants.ParseEntryPoint(name);
            return isChain ? eventLayout.Chains[id] : id;
        }

        var mapObjects = map.ObjectGroups.SelectMany(x => x.Objects);
        var pathParser = NpcPathBuilder.BuildParser(mapObjects, tileWidth, tileHeight);

        foreach (var objectGroup in map.ObjectGroups)
        {
            foreach (var obj in objectGroup.Objects)
            {
                if (TypeName.Trigger.Equals(obj.Type, StringComparison.OrdinalIgnoreCase))
                    triggers.Add(TriggerMapping.ParseTrigger(obj, tileWidth, tileHeight, ResolveEntryPoint));

                if (TypeName.Npc.Equals(obj.Type, StringComparison.OrdinalIgnoreCase))
                    npcs.Add(NpcMapping.ParseNpc(obj, tileWidth, tileHeight, ResolveEntryPoint, pathParser));

                if (TypeName.Marker.Equals(obj.Type, StringComparison.OrdinalIgnoreCase))
                {
                    var (marker, tile) = AutomapMapping.ParseMarker(obj, tileWidth, tileHeight);
                    markers.Add(marker);
                    markerTiles.Add(tile);
                }
            }
        }

        TriggerMapping.LoadZones(zones, info.AssetId, triggers, map);
    }
}