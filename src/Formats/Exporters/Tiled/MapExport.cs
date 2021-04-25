using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UAlbion.Formats.Assets.Labyrinth;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Formats.Exporters.Tiled
{
    public static class MapExport
    {
        const int TilesetSpacing = 1000;
        const int FloorGid = 0;
        const int WallGid = TilesetSpacing;
        const int ContentsGid = 2 * TilesetSpacing;
        const int CeilingGid = 3 * TilesetSpacing;

        public static Map FromAlbionMap2D(
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
            int nextObjectGroupId = 3; // 1 & 2 are always underlay & overlay.
            npcTileset.GidOffset = tileset.Tiles.Count;

            return new Map
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
                Orientation = "orthogonal",
                RenderOrder = "right-down",
                BackgroundColor = "#000000",
                Tilesets = new List<MapTileset>
                {
                    new MapTileset { FirstGid = 0, Source = tilesetPath, },
                    new MapTileset { FirstGid = npcTileset.GidOffset, Source = npcTileset.Filename }
                },
                Layers = new List<MapLayer> {
                    new MapLayer
                    {
                        Id = 1,
                        Name = "Underlay",
                        Width = map.Width,
                        Height = map.Height,
                        Data = new LayerData { Encoding = "csv", Content = BuildCsvData(map, tileset, false, blankTileIndex) }
                    },
                    new MapLayer
                    {
                        Id = 2,
                        Name = "Overlay",
                        Width = map.Width,
                        Height = map.Height,
                        Data = new LayerData { Encoding = "csv", Content = BuildCsvData(map, tileset, true, blankTileIndex) }
                    }
                },
                ObjectGroups = new[] {
                    BuildTriggers(map, properties, eventFormatter, false, ref nextObjectGroupId, ref nextObjectId),
                    BuildNpcs(map, properties, eventFormatter, npcTileset, ref nextObjectGroupId, ref nextObjectId)
                }.SelectMany(x => x).ToList()
            };
        }

        public static Map FromAlbionMap3D(MapData3D map, Tilemap3DProperties properties, EventFormatter eventFormatter)
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

            return new Map
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
                    new MapTileset { FirstGid = FloorGid, Source = properties.FloorPath, },
                    new MapTileset { FirstGid = WallGid, Source = properties.WallPath, },
                    new MapTileset { FirstGid = ContentsGid, Source = properties.ContentsPath },
                    new MapTileset { FirstGid = CeilingGid, Source = properties.CeilingPath, },
                },
                Layers = new List<MapLayer> {
                    new MapLayer
                    {
                        Id = 1,
                        Name = "Floors",
                        Width = map.Width,
                        Height = map.Height,
                        Data = new LayerData { Encoding = "csv", Content = BuildCsvData(map, IsometricMode.Floors) }
                    },
                    new MapLayer
                    {
                        Id = 2,
                        Name = "Walls",
                        Width = map.Width,
                        Height = map.Height,
                        Data = new LayerData { Encoding = "csv", Content = BuildCsvData(map, IsometricMode.Walls) }
                    },
                    new MapLayer
                    {
                        Id = 3,
                        Name = "Contents",
                        Width = map.Width,
                        Height = map.Height,
                        Data = new LayerData { Encoding = "csv", Content = BuildCsvData(map, IsometricMode.Contents) }
                    },
                    new MapLayer
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
                    BuildTriggers(map, properties, eventFormatter, true, ref nextObjectGroupId, ref nextObjectId),
                    // BuildNpcs(map, properties, eventFormatter, npcTileset, ref nextObjectGroupId, ref nextObjectId)
                }.SelectMany(x => x).ToList()
            };
        }

        static IEnumerable<ObjectGroup> BuildTriggers(
            BaseMapData map,
            TilemapProperties properties,
            EventFormatter eventFormatter,
            bool isometric,
            ref int nextObjectGroupId,
            ref int nextObjectId)
        {
            var objectGroups = new List<ObjectGroup>();

            var regions = TriggerZoneBuilder.BuildZones(map);
            var groupedByTriggerType = regions
                .Where(x => x.Item1.Chain != 0xffff)
                .GroupBy(x => x.Item1.Trigger)
                .OrderBy(x => x.Key);

            foreach (var triggerType in groupedByTriggerType)
            {
                objectGroups.Add(BuildTriggerObjectGroup(
                    nextObjectGroupId++,
                    $"T:{triggerType.Key}",
                    triggerType,
                    properties,
                    eventFormatter,
                    isometric,
                    ref nextObjectId));
            }

            return objectGroups;
        }

        static ObjectGroup BuildTriggerObjectGroup(
            int objectGroupId,
            string name,
            IEnumerable<(ZoneKey, Geometry.Polygon)> polygons,
            TilemapProperties properties,
            EventFormatter eventFormatter,
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
                    Name = $"C{r.Item1.Chain} {r.Item1.Trigger}",
                    Type = "Trigger",
                    X = r.Item2.OffsetX * width,
                    Y = r.Item2.OffsetY * properties.TileHeight,
                    Polygon = new Polygon(r.Item2.Points, width, properties.TileHeight),
                    Properties = new List<ObjectProperty>
                    {
                        new ObjectProperty("Script", eventFormatter.FormatChain(r.Item1.Node)),
                        new ObjectProperty("Trigger", r.Item1.Trigger.ToString()),
                        new ObjectProperty("Unk1", r.Item1.Unk1.ToString(CultureInfo.InvariantCulture)),
                    }
                };

            var objectGroup = new ObjectGroup
            {
                Id = objectGroupId,
                Name = name,
                Color = "#" + (name.GetHashCode() & 0x00ffffff).ToString("x", CultureInfo.InvariantCulture),
                Opacity = 0.5f,
                Objects = zonePolygons.ToList(),
            };

            nextObjectId = nextId;
            return objectGroup;
        }

        static IEnumerable<ObjectGroup> BuildNpcs(
            BaseMapData map,
            TilemapProperties properties,
            EventFormatter eventFormatter,
            Tileset npcTileset,
            ref int nextObjectGroupId,
            ref int nextObjectId)
        {
            int nextId = nextObjectId;
            int nextGroupId = nextObjectGroupId;
            var group = new ObjectGroup
            {
                Id = nextGroupId++,
                Name = "NPCs",
                Objects = map.Npcs.Select(x =>
                        BuildNpcObject(
                            properties,
                            eventFormatter,
                            npcTileset,
                            x,
                            ref nextId))
                    .ToList(),
            };

            nextObjectId = nextId;
            nextObjectGroupId = nextGroupId;
            return new[] { group };
        }

        static MapObject BuildNpcObject(
            TilemapProperties properties,
            EventFormatter eventFormatter,
            Tileset npcTileset,
            MapNpc npc,
            ref int nextId)
        {
            var objProps = new List<ObjectProperty>
            {
                new ObjectProperty("Visual", npc.SpriteOrGroup.ToString()),
                new ObjectProperty("Flags", npc.Flags.ToString()),
                new ObjectProperty("Movement", ((int) npc.Movement).ToString(CultureInfo.InvariantCulture)),
                new ObjectProperty("Unk8", npc.Unk8.ToString(CultureInfo.InvariantCulture)),
                new ObjectProperty("Unk9", npc.Unk9.ToString(CultureInfo.InvariantCulture))
            };

            if (!npc.Id.IsNone) objProps.Add(new ObjectProperty("Id", npc.Id.ToString()));
            if (npc.Node != null) objProps.Add(new ObjectProperty("Script", eventFormatter.FormatChain(npc.Node)));
            if (npc.Sound > 0)
                objProps.Add(new ObjectProperty("Sound", npc.Sound.ToString(CultureInfo.InvariantCulture)));
            // TODO: Path

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