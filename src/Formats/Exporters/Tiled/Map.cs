using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using UAlbion.Config;
using UAlbion.Formats.Assets.Maps;

#pragma warning disable CA1034 // Nested types should not be visible
#pragma warning disable CA2227 // Collection properties should be read only
namespace UAlbion.Formats.Exporters.Tiled
{
    [XmlRoot("map")]
    public class Map
    {
        [XmlAttribute("version")] public string Version { get; set; }
        [XmlAttribute("tiledversion")] public string TiledVersion { get; set; }
        [XmlAttribute("orientation")] public string Orientation { get; set; }
        [XmlAttribute("renderorder")] public string RenderOrder { get; set; }
        [XmlAttribute("width")] public int Width { get; set; }
        [XmlAttribute("height")] public int Height { get; set; }
        [XmlAttribute("tilewidth")] public int TileWidth { get; set; }
        [XmlAttribute("tileheight")] public int TileHeight { get; set; }
        [XmlAttribute("infinite")] public int Infinite { get; set; }
        [XmlAttribute("nextlayerid")] public int NextLayerId { get; set; }
        [XmlAttribute("nextobjectid")] public int NextObjectId { get; set; }
        [XmlAttribute("backgroundcolor")] public string BackgroundColor { get; set; }
        [XmlElement("tileset")] public List<MapTileset> Tilesets { get; set; }
        [XmlElement("layer")] public List<MapLayer> Layers { get; set; }
        [XmlElement("objectgroup")] public List<ObjectGroup> ObjectGroups { get; set; }

        public static Map Load(string path)
        {
            using var stream = File.OpenRead(path);
            using var xr = new XmlTextReader(stream);
            var serializer = new XmlSerializer(typeof(Map));
            return (Map)serializer.Deserialize(xr);
        }

        public void Save(string path)
        {
            var dir = Path.GetDirectoryName(path);
            foreach (var tileset in Tilesets)
                tileset.Source = ConfigUtil.GetRelativePath(tileset.Source, dir, false);

            using var stream = File.Open(path, FileMode.Create);
            using var sw = new StreamWriter(stream);
            Serialize(sw);
        }

        public void Serialize(TextWriter tw)
        {
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            var serializer = new XmlSerializer(typeof(Map));
            serializer.Serialize(tw, this, ns);
        }

        public static Map FromAlbionMap(
            MapData2D map,
            TilesetData tileset,
            TilemapProperties properties,
            string tilesetPath,
            Tileset npcTileset,
            EventFormatter eventFormatter)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));
            if (tileset == null) throw new ArgumentNullException(nameof(tileset));
            if (properties == null) throw new ArgumentNullException(nameof(properties));
            if (npcTileset == null) throw new ArgumentNullException(nameof(npcTileset));

            ushort blankTileIndex = (ushort)(tileset.Tiles.Max(x => x.ImageNumber + x.FrameCount - 1) + 1);
            int nextObjectId = 1;
            int nextObjectGroupId = 3; // 1 & 2 are always underlay & overlay.
            npcTileset.GidOffset = tileset.Tiles.Count + 1;

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
                    new MapTileset { FirstGid = 1, Source = tilesetPath, },
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
                    BuildTriggers(map, properties, eventFormatter, ref nextObjectGroupId, ref nextObjectId),
                    BuildNpcs(map, properties, eventFormatter, npcTileset, ref nextObjectGroupId, ref nextObjectId)
                }.SelectMany(x => x).ToList()
            };
        }

        static IEnumerable<ObjectGroup> BuildTriggers(
            MapData2D map,
            TilemapProperties properties,
            EventFormatter eventFormatter,
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
            ref int nextObjectId)
        {
            int nextId = nextObjectId;
            var zonePolygons =
                from r in polygons
                select new MapObject
                {
                    Id = nextId++,
                    Name = $"C{r.Item1.Chain} {r.Item1.Trigger}",
                    Type = "Trigger",
                    X = r.Item2.OffsetX * properties.TileWidth,
                    Y = r.Item2.OffsetY * properties.TileHeight,
                    Polygon = new Polygon(r.Item2.Points, properties.TileWidth, properties.TileHeight),
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
            MapData2D map,
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
                    var gfxIndex = tileset.Tiles[tileIndex].ImageNumber;
                    if (gfxIndex == 0xffff)
                        gfxIndex = blankTileIndex;
                    else
                        gfxIndex++;

                    sb.Append(gfxIndex);
                    sb.Append(',');
                }

                sb.AppendLine();
            }
            return sb.ToString(0, sb.Length - (Environment.NewLine.Length + 1));
        }
    }
}
#pragma warning restore CA2227 // Collection properties should be read only
#pragma warning restore CA1034 // Nested types should not be visible