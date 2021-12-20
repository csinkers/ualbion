using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Scripting;

namespace UAlbion.Formats.Exporters.Tiled
{
    public static class MapImport
    {
        static string PropString(Map map, string key)
        {
            if (map.Properties == null || map.Properties.Count == 0)
                return null;

            var prop = map.Properties.FirstOrDefault(x => key.Equals(x.Name, StringComparison.OrdinalIgnoreCase));
            return prop?.Value;
        }

        static int? PropInt(Map map, string key) => int.TryParse(PropString(map, key), out var i) ? i : null;
        static AssetId PropId(Map map, string key) => AssetId.Parse(PropString(map, key));

        public static BaseMapData ToAlbion(this Map map, AssetInfo info, AssetMapping mapping, string script)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));
            if (info == null) throw new ArgumentNullException(nameof(info));

            // Check width/height <= 255
            if (map.Width > 255) throw new FormatException($"Map widths above 255 are not currently supported (was {map.Width})");
            if (map.Height > 255) throw new FormatException($"Map heights above 255 are not currently supported (was {map.Height})");

            // Check layer existence
            var underlayLayer = map.Layers.FirstOrDefault(x => x.Name == "Underlay");
            var overlayLayer = map.Layers.FirstOrDefault(x => x.Name == "Overlay");
            if (underlayLayer == null) throw new FormatException("Expected a tile layer called \"Underlay\" in map");
            if (overlayLayer == null) throw new FormatException("Expected a tile layer called \"Overlay\" in map");

            var rawUnderlay = underlayLayer.Data.Content;
            var rawOverlay = overlayLayer.Data.Content;

            var underlay = ParseCsv(rawUnderlay).ToArray();
            var overlay = ParseCsv(rawOverlay).ToArray();

            if (underlay.Length != map.Width * map.Height) throw new FormatException($"Map underlay had {underlay.Length}, but {map.Width * map.Height} were expected ({map.Width} x {map.Height})");
            if (overlay.Length != map.Width * map.Height) throw new FormatException($"Map overlay had {overlay.Length}, but {map.Width * map.Height} were expected ({map.Width} x {map.Height})");

            var steps = new List<(string, IGraph)>();
            var eventLayout = ScriptCompiler.Compile(script, steps);

            ushort ResolveEntryPoint(string name)
            {
                var (isChain, id) = ScriptConstants.ParseEntryPoint(name);
                return isChain ? eventLayout.Chains[id] : id;
            }

            var triggers = new List<TriggerInfo>();
            var npcs = new List<MapNpc>();
            var getWaypoints = NpcPathBuilder.BuildWaypointLookup(map);

            foreach (var objectGroup in map.ObjectGroups)
            {
                foreach (var obj in objectGroup.Objects)
                {
                    if ("Trigger".Equals(obj.Type, StringComparison.OrdinalIgnoreCase))
                        triggers.Add(ParseTrigger(obj, map, ResolveEntryPoint));

                    if ("NPC".Equals(obj.Type, StringComparison.OrdinalIgnoreCase))
                        npcs.Add(ParseNpc(obj, map, ResolveEntryPoint, getWaypoints));
                }
            }

            // TODO: Dummy/blank NPCs?

            /* ObjectGroups = new[] {
                    BuildTriggers(map, properties, eventFormatter, ref nextObjectGroupId, ref nextObjectId),
                    BuildNpcs(map, properties, eventFormatter, npcTileset, ref nextObjectGroupId, ref nextObjectId)
                }.SelectMany(x => x).ToList() */

            var zones = BuildGlobalZones(info.AssetId, triggers);
            zones.AddRange(BuildZones(info.AssetId, map, triggers));

            return new MapData2D(info.AssetId, (byte)map.Width, (byte)map.Height, eventLayout.Events, eventLayout.Chains, npcs, zones)
            {
                RawLayout = FormatUtil.ToPacked(underlay, overlay, 1),
                Flags = Enum.Parse<FlatMapFlags>(PropString(map, "Flags")),
                OriginalNpcCount = (byte)(PropInt(map, "OriginalNpcCount") ?? 96),
                FrameRate = (byte)(PropInt(map, "FrameRate") ?? 0),
                Sound = (byte)(PropInt(map, "Sound") ?? 0),
                CombatBackgroundId = PropId(map, "CombatBackground"),
                PaletteId = PropId(map, "Palette"),
                SongId = PropId(map, "Song"),
                TilesetId = PropId(map, "Tileset"),
            };
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
                    X = 255,
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
                        Trigger = trigger.TriggerType,
                        Unk1 = trigger.Unk1,
                        Global = trigger.Global
                    };
                }
            }

            return zones.Where(x => x != null);
        }

        static MapNpc ParseNpc(MapObject obj, Map map, Func<string, ushort> resolveEntryPoint, Func<int, NpcWaypoint[]> getWaypoints)
        {
            var position = ((int)obj.X / map.TileWidth, (int)obj.Y / map.TileHeight);
            NpcWaypoint[] waypoints = { new((byte)position.Item1, (byte)position.Item2) };

            // string RequiredProp(string name) => Prop(name) ?? throw new FormatException($"Required property \"{name}\" was not present on NPC \"{obj.Name}\" (id {obj.Id})");

            var id = obj.PropString("Id");
            var visual = obj.PropString("Visual");
            var group = obj.PropString("Group");
            if (string.IsNullOrEmpty(visual) && string.IsNullOrEmpty(group)) // TODO: Differentiate between 2D/3D maps
                throw new FormatException($"NPC \"{obj.Name}\" (id {obj.Id}) requires either a Visual or Group property to determine its appearance");

            var entryPointName = obj.PropString("Script");
            var entryPoint = resolveEntryPoint(entryPointName);

            var pathStart = obj.PropInt("Path");
            if (pathStart.HasValue)
                waypoints = getWaypoints(pathStart.Value);

            return new MapNpc
            {
                Id = string.IsNullOrEmpty(id) ? AssetId.None : AssetId.Parse(id),
                Node = entryPoint == EventNode.UnusedEventId ? null : new DummyEventNode(entryPoint),
                Waypoints = waypoints,
                Flags = (NpcFlags)Enum.Parse(typeof(NpcFlags), obj.PropString("Flags")),
                Movement = (NpcMovementTypes)Enum.Parse(typeof(NpcMovementTypes), obj.PropString("Movement")),
                Unk8 = (byte)(obj.PropInt("Unk8") ?? 0),
                Unk9 = (byte)(obj.PropInt("Unk9") ?? 0),
                SpriteOrGroup = AssetId.Parse(visual) // TODO: Handle groups for 3D maps
            };
        }

        static TriggerInfo ParseTrigger(MapObject obj, Map map, Func<string, ushort> resolveEntryPoint)
        {
            string Prop(string name)
            {
                var prop = obj.Properties.FirstOrDefault(x => name.Equals(x.Name, StringComparison.OrdinalIgnoreCase));
                return prop?.Value ?? prop?.MultiLine;
            }

            string RequiredProp(string name) => Prop(name) ?? throw new FormatException($"Required property \"{name}\" was not present on NPC \"{obj.Name}\" (id {obj.Id})");

            var polygon = obj.Polygon.Points.Select(p => (((int)obj.X + p.x) / map.TileWidth, ((int)obj.Y + p.y) / map.TileHeight));
            var shape = PolygonToShape(polygon);
            var entryPointName = Prop("Script");
            var entryPoint = resolveEntryPoint(entryPointName);
            var trigger = RequiredProp("Trigger");
            var unk1 = Prop("Unk1");
            var global = Prop("Global") is { } s && "true".Equals(s, StringComparison.OrdinalIgnoreCase);

            return new TriggerInfo
            {
                Global = global,
                ObjectId = obj.Id,
                TriggerType = (TriggerTypes)Enum.Parse(typeof(TriggerTypes), trigger),
                Unk1 = string.IsNullOrEmpty(unk1) ? (byte)0 : byte.Parse(unk1, CultureInfo.InvariantCulture),
                EventIndex = entryPoint,
                Points = TriggerZoneBuilder.GetPointsInsideShape(shape)
            };
        }

        static IEnumerable<((int x, int y) from, (int x, int y) to)> PolygonToShape(IEnumerable<(int x, int y)> polygon)
        {
            bool first = true;
            (int x, int y) firstPoint = (0, 0);
            (int x, int y) lastPoint = (0, 0);

            foreach (var point in polygon)
            {
                if (first)
                    firstPoint = point;
                else
                    yield return (lastPoint, point);

                lastPoint = point;
                first = false;
            }

            yield return (lastPoint, firstPoint);
        }

        static IEnumerable<int> ParseCsv(string csv)
        {
            if (string.IsNullOrEmpty(csv))
                yield break;

            int n = 0;
            foreach (var c in csv)
            {
                switch (c)
                {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        n *= 10;
                        n += c - '0';
                        break;
                    case ',':
                        yield return n;
                        n = 0;
                        break;
                }
            }
            yield return n;
        }
    }
}