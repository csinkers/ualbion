using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.Exporters.Tiled;

public static class TriggerMapping
{
    static class Prop
    {
        public const string Trigger = "Trigger";
        public const string Script = "Script";
        public const string Unk1 = "Unk1";
        public const string Global = "Global";
    }

    public static IEnumerable<ObjectGroup> BuildTriggers(
        BaseMapData map,
        int tileWidth,
        int tileHeight,
        Dictionary<ushort, string> functionsByEventId,
        ref int nextObjectGroupId,
        ref int nextObjectId)
    {
        var objectGroups = new List<ObjectGroup>();
        var regions = TriggerZoneBuilder.BuildZones(map);

        if (map.GlobalZones.Count > 0)
        {
            var globalRegions = new List<(ZoneKey, Geometry.Polygon)>();
            for (var index = 0; index < map.GlobalZones.Count; index++)
            {
                var global = map.GlobalZones[index];
                var (x, y) = DiagonalLayout.GetPositionForIndex(index);
                var polygon = new Geometry.Polygon
                {
                    OffsetX = -x - 1,
                    OffsetY = -y - 1,
                    Points = new List<(int, int)> { (0, 0), (1, 0), (1, 1), (0, 1) }
                };
                globalRegions.Add((new ZoneKey(global), polygon));
            }

            objectGroups.Add(BuildTriggerObjectGroup(
                nextObjectGroupId++,
                "T:Global",
                globalRegions,
                tileWidth,
                tileHeight,
                functionsByEventId,
                ref nextObjectId));
        }

        var groupedByTriggerType = regions
            .Where(x => !x.Item1.Global)
            .GroupBy(x => x.Item1.Trigger)
            .OrderBy(x => x.Key);

        foreach (var polygonsForTriggerType in groupedByTriggerType)
        {
            objectGroups.Add(BuildTriggerObjectGroup(
                nextObjectGroupId++,
                $"T:{polygonsForTriggerType.Key}",
                polygonsForTriggerType,
                tileWidth,
                tileHeight,
                functionsByEventId,
                ref nextObjectId));

            if (polygonsForTriggerType.Key == TriggerTypes.Examine)
                objectGroups[^1].Hidden = true;
        }

        return objectGroups;
    }

    static List<TiledProperty> BuildTriggerProperties(ZoneKey zone, Dictionary<ushort, string> functionsByEventId)
    {
        var properties = new List<TiledProperty> { new(Prop.Trigger, zone.Trigger.ToString()) };

        if (zone.Node != null)
            properties.Add(new TiledProperty(Prop.Script, functionsByEventId[zone.Node.Id]));

        if (zone.Unk1 != 0)
            properties.Add(new TiledProperty(Prop.Unk1, zone.Unk1.ToString()));

        if (zone.Global)
            properties.Add(new TiledProperty(Prop.Global, "true"));

        return properties;
    }

    static ObjectGroup BuildTriggerObjectGroup(
        int objectGroupId,
        string name,
        IEnumerable<(ZoneKey, Geometry.Polygon)> polygons,
        int tileWidth,
        int tileHeight,
        Dictionary<ushort, string> functionsByEventId,
        ref int nextObjectId)
    {
        int nextId = nextObjectId;
        var zonePolygons =
            from r in polygons
            select new MapObject
            {
                Id = nextId++,
                Name = $"C{r.Item1.Chain}{(r.Item1.DummyNumber == 0 ? "" : $".{r.Item1.DummyNumber}")} {r.Item1.Trigger}",
                Type = ObjectGroupMapping.TypeName.Trigger,
                X = r.Item2.OffsetX * tileWidth,
                Y = r.Item2.OffsetY * tileHeight,
                Polygon = new Polygon(r.Item2.Points, tileWidth, tileHeight),
                Properties = BuildTriggerProperties(r.Item1, functionsByEventId)
            };

        var objectGroup = new ObjectGroup
        {
            Id = objectGroupId,
            Name = name,
            Color = "#" + (name.GetHashCode(StringComparison.InvariantCulture) & 0x00ffffff).ToString("x"),
            Opacity = 0.5f,
            Objects = zonePolygons.ToList(),
        };

        nextObjectId = nextId;
        return objectGroup;
    }

    public static void LoadZones(List<MapEventZone> zones, AssetId assetId, List<TriggerInfo> triggers, Map map)
    {
        zones.AddRange(BuildGlobalZones(assetId, triggers));
        zones.AddRange(BuildZones(assetId, map, triggers));
    }

    static List<MapEventZone> BuildGlobalZones(MapId mapId, List<TriggerInfo> triggers)
    {
        var results = new List<MapEventZone>();
        var globals = triggers
            .Where(x => x.Global)
            .OrderBy(x => DiagonalLayout.GetIndexForPosition(-x.Points[0].x, -x.Points[0].y));

        foreach (var global in globals)
        {
            if (global.TriggerType == 0) continue; // Ignore dummy zones
            results.Add(new MapEventZone
            {
                Global = true,
                X = 0,
                Y = 0,
                ChainSource = mapId,
                Node = global.EventIndex == EventNode.UnusedEventId ? null : new DummyEventNode(global.EventIndex),
                Trigger = global.TriggerType,
                Unk1 = global.Unk1
            });
        }

        return results;
    }

    static IEnumerable<MapEventZone> BuildZones(MapId mapId, Map map, List<TriggerInfo> triggers)
    {
        var zones = new MapEventZone[map.Width * map.Height];

        // Ensure that smaller regions on top of a bigger one replace them by processing the larger ones first
        foreach (var trigger in triggers.Where(x => !x.Global).OrderByDescending(x => x.Points.Count))
        {
            foreach (var (x, y) in trigger.Points)
            {
                int index = y * map.Width + x;
                zones[index] = new MapEventZone
                {
                    X = (byte)x,
                    Y = (byte)y,
                    ChainSource = mapId,
                    Node = trigger.EventIndex == EventNode.UnusedEventId ? null : new DummyEventNode(trigger.EventIndex),
                    Chain = trigger.EventIndex == EventNode.UnusedEventId ? EventNode.UnusedEventId : (ushort)0,
                    Trigger = trigger.TriggerType,
                    Unk1 = trigger.Unk1,
                    Global = trigger.Global
                };
            }
        }

        return zones.Where(x => x != null);
    }

    public static TriggerInfo ParseTrigger(MapObject obj, int tileWidth, int tileHeight, Func<string, ushort> resolveEntryPoint)
    {
        var polygon = obj.Polygon.Points.Select(p => (((int)obj.X + p.x) / tileWidth, ((int)obj.Y + p.y) / tileHeight));
        var entryPointName = obj.PropString(Prop.Script);
        var entryPoint = resolveEntryPoint(entryPointName);
        var trigger = obj.PropString(Prop.Trigger) ?? throw new FormatException($"Required property \"{Prop.Trigger}\" was not present on trigger \"{obj.Name}\" (id {obj.Id}) @ ({obj.X}, {obj.Y})");
        var unk1 = obj.PropString(Prop.Unk1);
        var global = obj.PropString(Prop.Global) is { } s && "true".Equals(s, StringComparison.OrdinalIgnoreCase);
        var points = TriggerZoneBuilder.GetPointsInsidePolygon(polygon);

        if (points.Count == 0)
            throw new FormatException($"Trigger {trigger} at ({obj.X}, {obj.Y}) resolved to an empty point set!");

        return new TriggerInfo
        {
            Global = global,
            ObjectId = obj.Id,
            TriggerType = (TriggerTypes)Enum.Parse(typeof(TriggerTypes), trigger),
            Unk1 = string.IsNullOrEmpty(unk1) ? (byte)0 : byte.Parse(unk1),
            EventIndex = entryPoint,
            Points = points
        };
    }
}
