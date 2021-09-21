using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UAlbion.Config;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.MapEvents;

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

        public static BaseMapData ToAlbion(this Map map, AssetInfo info, AssetMapping mapping)
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

            var triggers = new List<TriggerInfo>();
            var npcs = new List<NpcInfo>();
            foreach (var objectGroup in map.ObjectGroups)
            {
                foreach (var obj in objectGroup.Objects)
                {
                    if ("Trigger".Equals(obj.Type, StringComparison.OrdinalIgnoreCase))
                        triggers.Add(ParseTrigger(obj, map));

                    if ("NPC".Equals(obj.Type, StringComparison.OrdinalIgnoreCase))
                        npcs.Add(ParseNpc(obj, map));
                }
            }

            // TODO: Dummy/blank NPCs?

            /* ObjectGroups = new[] {
                    BuildTriggers(map, properties, eventFormatter, ref nextObjectGroupId, ref nextObjectId),
                    BuildNpcs(map, properties, eventFormatter, npcTileset, ref nextObjectGroupId, ref nextObjectId)
                }.SelectMany(x => x).ToList() */

            var (events, chains) = BuildChains(info.AssetId, triggers, npcs, mapping);
            var zones = BuildGlobalZones(info.AssetId, triggers);
            zones.AddRange(BuildZones(info.AssetId, map, triggers));

            return new MapData2D(info.AssetId, (byte)map.Width, (byte)map.Height,
                events,
                chains,
                npcs.Select(x => BuildNpc(x)),
                zones)
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

        static void BuildEventHash(MapId mapId, IScriptable scriptable, AssetMapping mapping)
        {
            if (scriptable.Events == null)
                return;

            scriptable.EventBytes = FormatUtil.SerializeToBytes(s =>
            {
                for (ushort i = 0; i < scriptable.Events.Count; i++)
                    scriptable.Events[i] = EventNode.Serdes(i, scriptable.Events[i], s, mapId, mapId.ToMapText(), mapping);
            });
        }

        static (IList<EventNode> events, IList<ushort> chains) BuildChains(MapId mapId, List<TriggerInfo> triggers, List<NpcInfo> npcs, AssetMapping mapping)
        {
            var scriptables = triggers.Cast<IScriptable>().Concat(npcs);
            foreach (var scriptable in scriptables)
                BuildEventHash(mapId, scriptable, mapping);

            var scripts = scriptables.GroupBy(x => x.Key, ScriptableKeyComparer.Instance);
            var scriptsByHint = new Dictionary<ChainHint, List<IGrouping<ScriptableKey, IScriptable>>>();
            foreach (var g in scripts)
            {
                foreach (var scriptable in g)
                {
                    scriptable.EventBytes = g.Key.EventBytes;
                    var first = g.First();
                    if (scriptable != first && first.Events != null)
                        scriptable.Events = first.Events.ToList();
                }

                if (!scriptsByHint.TryGetValue(g.Key.ChainHint, out var group))
                {
                    group = new List<IGrouping<ScriptableKey, IScriptable>>();
                    scriptsByHint[g.Key.ChainHint] = group;
                }

                group.Add(g);
            }

            var events = new List<EventNode>();
            var chains = new List<ushort>();

            ushort eventId = 0;
            foreach (var kvp in scriptsByHint.OrderBy(x => x.Key))
            {
                var hint = kvp.Key;
                var groupsWithHint = kvp.Value;
                if (hint.IsNone) 
                    continue;

                if (hint.IsChain)
                {
                    while (chains.Count <= hint.ChainId)
                        chains.Add(ushort.MaxValue);
                    chains[hint.ChainId] = eventId;

                    if (groupsWithHint.Count > 1)
                    {
                        // Map Map.TorontoStart contains multiple scriptable entities with differing event chains but the same chain hint (12):
                        // Version 1: 1, 4, 5
                        // Version 2: 3
                        var sb = new StringBuilder();
                        sb.AppendLine($"Map {mapId} contains multiple scriptable entities with differing event chains but the same chain hint ({hint}):");
                        int version = 1;
                        foreach (var grouping in groupsWithHint)
                        {
                            sb.Append("Version "); sb.Append(version++); sb.Append(": ");
                            foreach (var scriptable in grouping)
                            {
                                sb.Append(scriptable.ObjectId);
                                sb.Append(' ');
                            }
                            sb.AppendLine();
                        }

                        throw new FormatException(sb.ToString());
                    }
                }

                var groupEvents = groupsWithHint[0].First().Events;
                foreach (var e in groupEvents)
                {
                    events.Add(e);
                    e.Id = eventId;
                    eventId++;
                }
            }

            if (scriptsByHint.TryGetValue(ChainHint.None, out var groupsWithoutHint))
            {
                var group = groupsWithoutHint[^1];
                groupsWithoutHint.RemoveAt(groupsWithoutHint.Count - 1);
                if (groupsWithoutHint.Count == 0)
                    scriptsByHint.Remove(ChainHint.None);

                var groupEvents = group.First().Events;
                if (groupEvents != null)
                {
                    foreach (var e in groupEvents)
                    {
                        events.Add(e);
                        e.Id = eventId;
                        eventId++;
                    }
                }
            }

            return (events, chains);
        }

        static MapNpc BuildNpc(NpcInfo npc)
        {
            // npc.Npc.Chain = 0; // TODO
            return npc.Npc;
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
                    Chain = global.ChainHint.ChainId,
                    ChainSource = mapId,
                    Node = global.Events[0],
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
                        Chain = trigger.ChainHint.ChainId,
                        ChainSource = mapId,
                        Node = trigger.Events[0],
                        Trigger = trigger.TriggerType,
                        Unk1 = trigger.Unk1,
                        Global = trigger.Global
                    };
                }
            }

            return zones.Where(x => x != null);
        }

        static List<EventNode> ParseScript(string script)
        {
            if (string.IsNullOrWhiteSpace(script))
                return null;
            var lines = FormatUtil.SplitLines(script);
            return lines.Select(EventNode.Parse).ToList();
        }

        static NpcInfo ParseNpc(MapObject obj, Map map)
        {
            var position = ((int)obj.X / map.TileWidth, (int)obj.Y / map.TileHeight);
            NpcWaypoint[] waypoints = { new((byte)position.Item1, (byte)position.Item2) };

            string Prop(string name) => obj.Properties.FirstOrDefault(x => name.Equals(x.Name, StringComparison.OrdinalIgnoreCase))?.Value;
            // string RequiredProp(string name) => Prop(name) ?? throw new FormatException($"Required property \"{name}\" was not present on NPC \"{obj.Name}\" (id {obj.Id})");

            var id = Prop("Id");
            var visual = Prop("Visual");
            var group = Prop("Group");
            if (string.IsNullOrEmpty(visual) && string.IsNullOrEmpty(group)) // TODO: Differentiate between 2D/3D maps
                throw new FormatException($"NPC \"{obj.Name}\" (id {obj.Id}) requires either a Visual or Group property to determine its appearance");

            var events = ParseScript(Prop("Script"));
            UnswizzleEvents(events);

            return new NpcInfo
            {
                ObjectId = obj.Id,
                ChainHint = ChainHint.None,
                Events = events,
                Npc = new MapNpc
                {
                    Id = string.IsNullOrEmpty(id) ? AssetId.None : AssetId.Parse(id),
                    Waypoints = waypoints,
                    Flags = (NpcFlags)Enum.Parse(typeof(NpcFlags), Prop("Flags")),
                    Movement = (NpcMovementTypes)Enum.Parse(typeof(NpcMovementTypes), Prop("Movement")),
                    Unk8 = byte.Parse(Prop("Unk8") ?? "0", CultureInfo.InvariantCulture),
                    Unk9 = byte.Parse(Prop("Unk9") ?? "0", CultureInfo.InvariantCulture),
                    SpriteOrGroup = AssetId.Parse(visual) // TODO: Handle groups for 3D maps
                }
            };
        }

        static ChainHint ParseChainHint(string name)
        {
            if (string.IsNullOrEmpty(name)) return ChainHint.None;
            if (name[0] != 'C') return ChainHint.None;
            int index = name.IndexOf(' ', StringComparison.InvariantCulture);
            var segment = index == -1 ? name.Substring(1) : name.Substring(1, index - 1);

            index = segment.IndexOf('.', StringComparison.Ordinal);
            if (!ushort.TryParse(index == -1 ? segment : segment[..index], out var chainId))
                return ChainHint.None;

            ushort dummy = 0;
            if (index != -1 && !ushort.TryParse(segment[(index + 1)..], out dummy)) 
                return ChainHint.None;

            return new ChainHint(chainId, dummy);
        }

        static TriggerInfo ParseTrigger(MapObject obj, Map map)
        {
            string Prop(string name)
            {
                var prop = obj.Properties.FirstOrDefault(x => name.Equals(x.Name, StringComparison.OrdinalIgnoreCase));
                return prop?.Value ?? prop?.MultiLine;
            }

            string RequiredProp(string name) => Prop(name) ?? throw new FormatException($"Required property \"{name}\" was not present on NPC \"{obj.Name}\" (id {obj.Id})");

            var polygon = obj.Polygon.Points.Select(p => (((int)obj.X + p.x) / map.TileWidth, ((int)obj.Y + p.y) / map.TileHeight));
            var shape = PolygonToShape(polygon);
            var events = ParseScript(Prop("Script"));
            UnswizzleEvents(events);
            var trigger = RequiredProp("Trigger");
            var unk1 = Prop("Unk1");
            var global = Prop("Global") is { } s && "true".Equals(s, StringComparison.OrdinalIgnoreCase);

            return new TriggerInfo
            {
                Global = global,
                ObjectId = obj.Id,
                ChainHint = ParseChainHint(obj.Name),
                TriggerType = (TriggerTypes)Enum.Parse(typeof(TriggerTypes), trigger),
                Unk1 = string.IsNullOrEmpty(unk1) ? (byte)0 : byte.Parse(unk1, CultureInfo.InvariantCulture),
                Events = events,
                Points = TriggerZoneBuilder.GetPointsInsideShape(shape)
            };
        }

        static void UnswizzleEvents(List<EventNode> events)
        {
            if (events == null)
                return;

            foreach (var e in events)
            {
                if (e.Next is DummyEventNode dummy)
                    e.Next = events[dummy.Id];

                if (e is BranchNode { NextIfFalse: DummyEventNode dummy2 } branch)
                    branch.NextIfFalse = events[dummy2.Id];
            }
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