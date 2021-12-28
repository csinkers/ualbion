using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Config;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Scripting;

namespace UAlbion.Formats.Exporters.Tiled;

public static class ObjectGroupMapping
{
    public static class ObjectTypeName
    {
        public const string Trigger = "Trigger";
        public const string Npc = "NPC";
    }

    public static List<ObjectGroup> BuildObjectGroups(
        BaseMapData map,
        int tileWidth,
        int tileHeight,
        NpcMapping.GetTileFunc getTileFunc,
        Dictionary<ushort, string> functionsByEventId,
        ref int nextObjectGroupId,
        ref int nextObjectId)
        =>
            new[]
            {
                TriggerMapping.BuildTriggers(map, tileWidth, tileHeight, functionsByEventId, ref nextObjectGroupId, ref nextObjectId),
                NpcMapping.BuildNpcs(map, tileWidth, tileHeight, getTileFunc, functionsByEventId, ref nextObjectGroupId, ref nextObjectId),
            }.SelectMany(x => x).ToList();


    public static void LoadObjectGroups(AssetInfo info,
        Map map,
        int tileWidth,
        int tileHeight,
        EventLayout eventLayout,
        List<TriggerInfo> triggers,
        List<MapNpc> npcs,
        List<MapEventZone> zones)
    {
        ushort ResolveEntryPoint(string name)
        {
            var (isChain, id) = ScriptConstants.ParseEntryPoint(name);
            return isChain ? eventLayout.Chains[id] : id;
        }

        var getWaypoints = NpcPathBuilder.BuildWaypointLookup(map);
        foreach (var objectGroup in map.ObjectGroups)
        {
            foreach (var obj in objectGroup.Objects)
            {
                if (ObjectTypeName.Trigger.Equals(obj.Type, StringComparison.OrdinalIgnoreCase))
                    triggers.Add(TriggerMapping.ParseTrigger(obj, tileWidth, tileHeight, ResolveEntryPoint));

                if (ObjectTypeName.Npc.Equals(obj.Type, StringComparison.OrdinalIgnoreCase))
                    npcs.Add(NpcMapping.ParseNpc(obj, tileWidth, tileHeight, ResolveEntryPoint, getWaypoints));
            }
        }

        TriggerMapping.LoadZones(zones, info.AssetId, triggers, map);
    }
}