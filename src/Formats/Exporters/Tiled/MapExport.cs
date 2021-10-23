using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UAlbion.Api;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Labyrinth;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Formats.Exporters.Tiled
{
    public static class MapExport
    {
        const int TilesetSpacing = 1000;
        const int FloorGid = 0;
        const int WallGid = TilesetSpacing;
        const int ContentsGid = 2 * TilesetSpacing;
        const int CeilingGid = 3 * TilesetSpacing;

        public static (Map, string) FromAlbionMap2D(
            MapData2D map,
            TilesetData tileset,
            Tilemap2DProperties properties,
            string tilesetPath,
            Tileset npcTileset,
            EventFormatter eventFormatter)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));
            if (tileset == null) throw new ArgumentNullException(nameof(tileset));
            if (properties == null) throw new ArgumentNullException(nameof(properties));
            if (npcTileset == null) throw new ArgumentNullException(nameof(npcTileset));

            ushort blankTileIndex = 0;
            int nextObjectId = 1;
            int nextLayerId = 3; // 1 & 2 are always underlay & overlay.
            npcTileset.GidOffset = tileset.Tiles.Count;
            var (script, functionsByEventId) = BuildScript(map, eventFormatter);

            var result = new Map
            {
                TiledVersion = "1.4.2",
                Version = "1.4",
                Width = map.Width,
                Height = map.Height,
                TileWidth = properties.TileWidth,
                TileHeight = properties.TileHeight,
                Infinite = 0,
                NextLayerId = 5, // max(layer or objectgroup id) + 1
                Orientation = "orthogonal",
                RenderOrder = "right-down",
                BackgroundColor = "#000000",
                Properties = BuildMapProperties(map),
                Tilesets = new List<MapTileset>
                {
                    new() { FirstGid = 0, Source = tilesetPath, },
                    new() { FirstGid = npcTileset.GidOffset, Source = npcTileset.Filename }
                },
                Layers = new List<TiledMapLayer> {
                    new()
                    {
                        Id = 1,
                        Name = "Underlay",
                        Width = map.Width,
                        Height = map.Height,
                        Data = new LayerData { Encoding = "csv", Content = BuildCsvData(map, tileset, false, blankTileIndex) }
                    },
                    new()
                    {
                        Id = 2,
                        Name = "Overlay",
                        Width = map.Width,
                        Height = map.Height,
                        Data = new LayerData { Encoding = "csv", Content = BuildCsvData(map, tileset, true, blankTileIndex) }
                    }
                },
                ObjectGroups = new[] {
                    BuildTriggers(map, properties, false, functionsByEventId, ref nextLayerId, ref nextObjectId),
                    BuildNpcs(map, properties, npcTileset, functionsByEventId, ref nextLayerId, ref nextObjectId),
                }.SelectMany(x => x).ToList()
            };

            result.NextObjectId = nextObjectId;
            result.NextLayerId = nextLayerId;
            return (result, script);
        }

        static List<TiledProperty> BuildMapProperties(MapData2D map)
        {
            var props = new List<TiledProperty>
            {
                new("OriginalNpcCount", map.OriginalNpcCount),
                new("Tileset", map.TilesetId.ToString()),
                new("Palette", map.PaletteId.ToString())
            };

            if (map.FrameRate > 0) props.Add(new("FrameRate", map.FrameRate));
            if (map.Sound > 0) props.Add(new("Sound", map.Sound));
            if (map.Flags != 0) props.Add(new("Flags", map.Flags.ToString()));
            if (map.SongId != SongId.None) props.Add(new("Song", map.SongId.ToString()));
            if (map.CombatBackgroundId != SpriteId.None) props.Add(new("CombatBackground", map.CombatBackgroundId.ToString()));
            return props;
        }

        public static (Map, string) FromAlbionMap3D(MapData3D map, Tilemap3DProperties properties, EventFormatter eventFormatter)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));
            if (properties == null) throw new ArgumentNullException(nameof(properties));
            if (eventFormatter == null) throw new ArgumentNullException(nameof(eventFormatter));

            if (string.IsNullOrEmpty(properties.FloorPath)) throw new ArgumentException("No floor path given", nameof(properties));
            if (string.IsNullOrEmpty(properties.CeilingPath)) throw new ArgumentException("No ceiling path given", nameof(properties));
            if (string.IsNullOrEmpty(properties.WallPath)) throw new ArgumentException("No wall path given", nameof(properties));
            if (string.IsNullOrEmpty(properties.ContentsPath)) throw new ArgumentException("No contents path given", nameof(properties));
            if (properties.TileWidth <= 0 || properties.TileWidth > 255) throw new ArgumentException("Width must be in the range [1..255]", nameof(properties));
            if (properties.TileHeight <= 0 || properties.TileHeight > 255) throw new ArgumentException("Height must be in the range [1..255]", nameof(properties));

            /* Layers:
            1: Floors
            2: Walls
            3: Content
            4: Ceilings (opacity 50%)
            5+ objectgroups for triggers
            n+ objectgroups for NPCs
            */

            int nextObjectId = 1;
            int nextObjectGroupId = 3; // 1 & 2 are always underlay & overlay.
            var (script, functionsByEventId) = BuildScript(map, eventFormatter);

            return (new Map
            {
                TiledVersion = "1.4.2",
                Version = "1.4",
                Width = map.Width,
                Height = map.Height,
                TileWidth = properties.TileWidth,
                TileHeight = properties.TileHeight,
                Infinite = 0,
                NextLayerId = 5, // max(layer or objectgroup id) + 1
                NextObjectId = 1,
                Orientation = "isometric",
                RenderOrder = "right-down",
                BackgroundColor = "#000000",
                Tilesets = new List<MapTileset>
                {
                    new() { FirstGid = FloorGid, Source = properties.FloorPath, },
                    new() { FirstGid = WallGid, Source = properties.WallPath, },
                    new() { FirstGid = ContentsGid, Source = properties.ContentsPath },
                    new() { FirstGid = CeilingGid, Source = properties.CeilingPath, },
                },
                Layers = new List<TiledMapLayer> {
                    new()
                    {
                        Id = 1,
                        Name = "Floors",
                        Width = map.Width,
                        Height = map.Height,
                        Data = new LayerData { Encoding = "csv", Content = BuildCsvData(map, IsometricMode.Floors) }
                    },
                    new()
                    {
                        Id = 2,
                        Name = "Walls",
                        Width = map.Width,
                        Height = map.Height,
                        Data = new LayerData { Encoding = "csv", Content = BuildCsvData(map, IsometricMode.Walls) }
                    },
                    new()
                    {
                        Id = 3,
                        Name = "Contents",
                        Width = map.Width,
                        Height = map.Height,
                        Data = new LayerData { Encoding = "csv", Content = BuildCsvData(map, IsometricMode.Contents) }
                    },
                    new()
                    {
                        Id = 4,
                        Name = "Ceilings",
                        Width = map.Width,
                        Height = map.Height,
                        Opacity = 0.5,
                        Data = new LayerData { Encoding = "csv", Content = BuildCsvData(map, IsometricMode.Ceilings) }
                    }
                },
                ObjectGroups = new[] {
                    BuildTriggers(map, properties, true, functionsByEventId, ref nextObjectGroupId, ref nextObjectId),
                    // BuildNpcs(map, properties, npcTileset, functionsByEventId, ref nextObjectGroupId, ref nextObjectId)
                }.SelectMany(x => x).ToList()
            }, script);
        }

        static (string script, Dictionary<ushort, string> functionsByEventId) BuildScript(IMapData map, EventFormatter eventFormatter)
        {
            var sb = new StringBuilder();
            var mapping = new Dictionary<ushort, string>();

            for (int chainId = 0; chainId < map.Chains.Count; chainId++)
            {
                var i = map.Chains[chainId];
                if (i == 0xffff) continue;

                var name = $"C{chainId}";
                mapping[i] = name;

                var labels = new List<IEventNode>();

                if (chainId > 0)
                    sb.AppendLine();

                sb.AppendLine($"function {name} {{");
                eventFormatter.FormatChainDecompiled(sb, map.Events[i], labels, 1);
                sb.AppendLine("}");
            }

            foreach (var key in GetDummyChains(map))
            {
                sb.AppendLine();
                var name = $"C{key.Chain}_{key.DummyNumber}";
                mapping[key.Node.Id] = name;

                var labels = new List<IEventNode>();

                sb.AppendLine($"function {name} {{");
                eventFormatter.FormatChainDecompiled(sb, key.Node, labels, 1);
                sb.AppendLine("}");
            }

            return (sb.ToString(), mapping);
        }

        static ushort[] GetEventToChainMapping(IMapData map)
        {
            var eventToChainMapping = new ushort[map.Events.Count];
            Array.Fill<ushort>(eventToChainMapping, 0xffff);

            var queue = new Queue<EventNode>();
            for (ushort i = 0; i < map.Chains.Count; i++)
            {
                if (map.Chains[i] == 0xffff) continue;
                queue.Enqueue(map.Events[map.Chains[i]]);
                while (queue.TryDequeue(out var e))
                {
                    if (eventToChainMapping[e.Id] != 0xffff) continue; // Already visited?
                    eventToChainMapping[e.Id] = i;

                    if (e.Next != null)
                        queue.Enqueue((EventNode)e.Next);

                    if (e is BranchNode { NextIfFalse: { } } branch)
                        queue.Enqueue((EventNode)branch.NextIfFalse);
                }
            }

            return eventToChainMapping;
        }

        static List<ZoneKey> GetDummyChains(IMapData map)
        {
            ushort chainId = 0;
            ushort dummyId = 1;
            var visited = new HashSet<EventNode>();
            var eventToChainMapping = GetEventToChainMapping(map);
            var dummies = new List<ZoneKey>();
            for (ushort i = 0; i < eventToChainMapping.Length; i++)
            {
                if (eventToChainMapping[i] != 0xffff)
                {
                    chainId = eventToChainMapping[i];
                    dummyId = 1;
                    continue;
                }

                var e = map.Events[i];
                if (visited.Contains(e))
                    continue;

                dummies.Add(new ZoneKey(chainId, dummyId, map.Events[i]));
                dummyId++;

                var queue = new Queue<EventNode>();
                queue.Enqueue(e);
                while (queue.TryDequeue(out e))
                {
                    if (visited.Contains(e)) continue;
                    if (eventToChainMapping[e.Id] != 0xffff) continue; // Belongs to a real chain. TODO: Assert always false?
                    visited.Add(e);

                    if (e.Next != null)
                        queue.Enqueue((EventNode)e.Next);

                    if (e is BranchNode { NextIfFalse: { } } branch)
                        queue.Enqueue((EventNode)branch.NextIfFalse);
                }
            }

            return dummies;
        }

        static IEnumerable<ObjectGroup> BuildTriggers(
            BaseMapData map,
            TilemapProperties properties,
            bool isometric,
            Dictionary<ushort, string> functionsByEventId,
            ref int nextObjectGroupId,
            ref int nextObjectId)
        {
            var objectGroups = new List<ObjectGroup>();

            var regions = TriggerZoneBuilder.BuildZones(map);

            int globalIndex = 0;
            var globals = regions.Where(x => x.Item1.Chain != 0xffff && x.Item1.Global).ToList();
            if (globals.Any())
            {
                foreach (var global in globals)
                {
                    var (x, y) = DiagonalLayout.GetPositionForIndex(globalIndex++);
                    (global.Item2.OffsetX, global.Item2.OffsetY) = (-x - 1, -y - 1);
                }

                objectGroups.Add(BuildTriggerObjectGroup(
                    nextObjectGroupId++,
                    "T:Global",
                    globals,
                    properties,
                    functionsByEventId,
                    isometric,
                    ref nextObjectId));
            }

            var groupedByTriggerType = regions
                .Where(x => x.Item1.Chain != 0xffff && !x.Item1.Global)
                .GroupBy(x => x.Item1.Trigger)
                .OrderBy(x => x.Key);

            foreach (var polygonsForTriggerType in groupedByTriggerType)
            {
                objectGroups.Add(BuildTriggerObjectGroup(
                    nextObjectGroupId++,
                    $"T:{polygonsForTriggerType.Key}",
                    polygonsForTriggerType,
                    properties,
                    functionsByEventId,
                    isometric,
                    ref nextObjectId));

                if (polygonsForTriggerType.Key == TriggerTypes.Examine)
                    objectGroups[^1].Hidden = true;
            }

            return objectGroups;
        }

        static List<TiledProperty> BuildTriggerProperties(ZoneKey zone, Dictionary<ushort, string> functionsByEventId)
        {
            var properties = new List<TiledProperty> { new("Trigger", zone.Trigger.ToString()) };

            if (zone.Node != null)
                properties.Add(new TiledProperty("Script", functionsByEventId[zone.Node.Id]));

            if (zone.Unk1 != 0)
                properties.Add(new TiledProperty("Unk1", zone.Unk1.ToString(CultureInfo.InvariantCulture)));

            if (zone.Global)
                properties.Add(new TiledProperty("Global", "true"));

            return properties;
        }

        static ObjectGroup BuildTriggerObjectGroup(
            int objectGroupId,
            string name,
            IEnumerable<(ZoneKey, Geometry.Polygon)> polygons,
            TilemapProperties properties,
            Dictionary<ushort, string> functionsByEventId,
            bool isometric,
            ref int nextObjectId)
        {
            int nextId = nextObjectId;
            var width = isometric ? properties.TileHeight : properties.TileWidth;
            var zonePolygons =
                from r in polygons
                select new MapObject
                {
                    Id = nextId++,
                    Name = $"C{r.Item1.Chain}{(r.Item1.DummyNumber == 0 ? "" : $".{r.Item1.DummyNumber}")} {r.Item1.Trigger}",
                    Type = "Trigger",
                    X = r.Item2.OffsetX * width,
                    Y = r.Item2.OffsetY * properties.TileHeight,
                    Polygon = new Polygon(r.Item2.Points, width, properties.TileHeight),
                    Properties = BuildTriggerProperties(r.Item1, functionsByEventId)
                };

            var objectGroup = new ObjectGroup
            {
                Id = objectGroupId,
                Name = name,
                Color = "#" + (name.GetHashCode(StringComparison.InvariantCulture) & 0x00ffffff).ToString("x", CultureInfo.InvariantCulture),
                Opacity = 0.5f,
                Objects = zonePolygons.ToList(),
            };

            nextObjectId = nextId;
            return objectGroup;
        }

        static IEnumerable<ObjectGroup> BuildNpcs(
            BaseMapData map,
            TilemapProperties properties,
            Tileset npcTileset,
            Dictionary<ushort, string> functionsByEventId,
            ref int nextObjectGroupId,
            ref int nextObjectId)
        {
            int nextId = nextObjectId;
            int npcGroupId = nextObjectGroupId++;

            var waypointGroups = new List<ObjectGroup>();
            var npcPathIndices = new Dictionary<int, int>();
            foreach (var npc in map.Npcs)
            {
                if ((npc.Movement & NpcMovementTypes.RandomMask) != 0)
                    continue;

                int firstWaypointObjectId = nextId;
                npcPathIndices[npc.Index] = firstWaypointObjectId;
                waypointGroups.Add(new ObjectGroup
                {
                    Id = nextObjectGroupId++,
                    Name = $"NPC{npc.Index} Path",
                    Objects = NpcPathBuilder.Build(npc, properties, ref nextId),
                    Hidden = true,
                });
            }

            var group = new ObjectGroup
            {
                Id = npcGroupId,
                Name = "NPCs",
                Objects = map.Npcs.Select(x =>
                        BuildNpcObject(
                            properties,
                            functionsByEventId,
                            npcTileset,
                            npcPathIndices,
                            x,
                            ref nextId))
                    .ToList(),
            };

            nextObjectId = nextId;
            return new[] { group }.Concat(waypointGroups);
        }

        static MapObject BuildNpcObject(TilemapProperties properties,
            Dictionary<ushort, string> functionsByEventId,
            Tileset npcTileset,
            Dictionary<int, int> npcPathIndices,
            MapNpc npc,
            ref int nextId)
        {
            var objProps = new List<TiledProperty>
            {
                new("Visual", npc.SpriteOrGroup.ToString()),
                new("Flags", npc.Flags.ToString()),
                new("Movement", ((int) npc.Movement).ToString(CultureInfo.InvariantCulture)),
                new("Unk8", npc.Unk8.ToString(CultureInfo.InvariantCulture)),
                new("Unk9", npc.Unk9.ToString(CultureInfo.InvariantCulture))
            };

            if (!npc.Id.IsNone) objProps.Add(new TiledProperty("Id", npc.Id.ToString()));
            if (npc.Node != null) objProps.Add(new TiledProperty("Script", functionsByEventId[npc.Node.Id]));
            if (npc.Sound > 0) objProps.Add(new TiledProperty("Sound", npc.Sound.ToString(CultureInfo.InvariantCulture)));
            if (npcPathIndices.TryGetValue(npc.Index, out var pathObjectId)) objProps.Add(TiledProperty.Object("Path", pathObjectId));

            var assetName = npc.SpriteOrGroup.ToString();
            var tile = npcTileset.Tiles.FirstOrDefault(x => x.Properties.Any(p => p.Name == "Visual" && p.Value == assetName));

            return new MapObject
            {
                Id = nextId++,
                Gid = tile == null ? 0 : (tile.Id + npcTileset.GidOffset),
                Name = $"NPC{npc.Index} {npc.Id}",
                Type = "NPC",
                X = npc.Waypoints[0].X * properties.TileWidth,
                Y = npc.Waypoints[0].Y * properties.TileHeight,
                Width = tile?.Image.Width ?? properties.TileWidth,
                Height = tile?.Image.Height ?? properties.TileHeight,
                Properties = objProps
            };
        }

        static string BuildCsvData(MapData2D map, TilesetData tileset, bool useOverlay, ushort blankTileIndex)
        {
            var sb = new StringBuilder();
            for (int j = 0; j < map.Height; j++)
            {
                for (int i = 0; i < map.Width; i++)
                {
                    int index = j * map.Width + i;
                    var tileIndex = useOverlay ? map.Overlay[index] : map.Underlay[index];
                    var tile = tileset.Tiles[tileIndex];
                    sb.Append(tile.IsBlank ? blankTileIndex : tileIndex);
                    sb.Append(',');
                }

                sb.AppendLine();
            }
            return sb.ToString(0, sb.Length - (Environment.NewLine.Length + 1));
        }

        static string BuildCsvData(MapData3D map, IsometricMode mode)
        {
            var (gidOffset, tiles) = mode switch
            {
                IsometricMode.Floors => (FloorGid, map.Floors),
                IsometricMode.Walls => (WallGid, map.BuildWallArray()),
                IsometricMode.Contents => (ContentsGid, map.BuildObjectArray()),
                IsometricMode.Ceilings => (CeilingGid, map.Ceilings),
                _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
            };

            var sb = new StringBuilder();
            for (int j = 0; j < map.Height; j++)
            {
                for (int i = 0; i < map.Width; i++)
                {
                    int index = j * map.Width + i;
                    sb.Append(gidOffset + tiles[index]);
                    sb.Append(',');
                }

                sb.AppendLine();
            }
            return sb.ToString(0, sb.Length - (Environment.NewLine.Length + 1));
        }
    }
}