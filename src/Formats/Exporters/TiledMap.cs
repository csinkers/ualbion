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
namespace UAlbion.Formats.Exporters
{
    [XmlRoot("map")]
    public class TiledMap
    {
        public class TiledMapTileset
        {
            [XmlAttribute("firstgid")] public int FirstGid { get; set; }
            [XmlAttribute("source")] public string Source { get; set; }
        }

        public class LayerData
        {
            [XmlAttribute("encoding")] public string Encoding { get; set; }
            [XmlText] public string Content { get; set; }
        }

        public class MapLayer
        {
            [XmlAttribute("id")] public int Id { get; set; }
            [XmlAttribute("name")] public string Name { get; set; }
            [XmlAttribute("width")] public int Width { get; set; }
            [XmlAttribute("height")] public int Height { get; set; }
            [XmlElement("data")] public LayerData Data { get; set; }
        }

        public class ObjectProperty
        {
            public ObjectProperty() {}
            public ObjectProperty(string key, string value)
            {
                Name = key;
                if (value.Contains('\n') || value.Contains('\r'))
                    MultiLine = value;
                else 
                    Value = value;
            }

            [XmlAttribute("name")] public string Name { get; set; }
            [XmlAttribute("value")] public string Value { get; set; }
            [XmlText] public string MultiLine { get; set; }
        }

        public class Polygon
        {
            public Polygon() {}

            public Polygon(IList<(int, int)> points)
            {
                if (points == null) throw new ArgumentNullException(nameof(points));
                var sb = new StringBuilder();
                foreach (var (x, y) in points)
                    sb.Append($"{x},{y} ");
                Points = sb.ToString(0, sb.Length - 1);
            }
            [XmlAttribute("points")] public string Points { get; set; }
        }

        public class TiledObject
        {
            [XmlAttribute("id")] public int Id { get; set; }
            [XmlAttribute("gid")] public int Gid { get; set; }
            [XmlIgnore] public bool GidSpecified => Gid != 0;
            [XmlAttribute("name")] public string Name { get; set; }
            [XmlAttribute("type")] public string Type { get; set; }
            [XmlAttribute("x")] public double X { get; set; }
            [XmlAttribute("y")] public double Y { get; set; }
            [XmlAttribute("width")] public double Width { get; set; }
            [XmlAttribute("height")] public double Height { get; set; }
            [XmlArray("properties")] [XmlArrayItem("property")] public List<ObjectProperty> Properties { get; set; }

            [XmlElement("polygon")] public Polygon Polygon { get; set; }
        }

        public class ObjectGroup
        {
            [XmlAttribute("id")] public int Id { get; set; }
            [XmlAttribute("name")] public string Name { get; set; }
            [XmlElement("object")] public List<TiledObject> Objects { get; set; }
        }

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
        [XmlElement("tileset")] public List<TiledMapTileset> Tilesets { get; set; }
        [XmlElement("layer")] public List<MapLayer> Layers { get; set; }
        [XmlElement("objectgroup")] public List<ObjectGroup> ObjectGroups { get; set; }

        public static TiledMap Load(string path)
        {
            using var stream = File.OpenRead(path);
            using var xr = new XmlTextReader(stream);
            var serializer = new XmlSerializer(typeof(TiledMap));
            return (TiledMap)serializer.Deserialize(xr);
        }

        public void Save(string path)
        {
            using var stream = File.OpenWrite(path);
            using var sw = new StreamWriter(stream);
            Serialize(sw);
        }

        public void Serialize(TextWriter tw)
        {
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            var serializer = new XmlSerializer(typeof(TiledMap));
            serializer.Serialize(tw, this, ns);
        }

        public static TiledMap FromAlbionMap(
            MapData2D map,
            TilesetData tileset,
            TilemapProperties properties,
            string tilesetPath,
            string npcTilesetPath,
            Func<AssetId, int> getNpcTileId,
            EventFormatter eventFormatter)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));
            if (tileset == null) throw new ArgumentNullException(nameof(tileset));
            if (properties == null) throw new ArgumentNullException(nameof(properties));

            ushort blankTileIndex = (ushort)(tileset.Tiles.Max(x => x.ImageNumber + x.FrameCount - 1) + 1);
            int nextObjectId = 1;
            int npcTileOffset = tileset.Tiles.Count + 1;
            int GetGid(AssetId assetId) => getNpcTileId(assetId) + npcTileOffset;

            return new TiledMap
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
                Tilesets = new List<TiledMapTileset>
                {
                    new TiledMapTileset { FirstGid = 1, Source = tilesetPath, },
                    new TiledMapTileset { FirstGid = npcTileOffset, Source = npcTilesetPath }
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
                ObjectGroups = new List<ObjectGroup>
                {
                    BuildTriggers(3, map, properties, eventFormatter, ref nextObjectId),
                    BuildNpcs(4, map, properties, eventFormatter, GetGid, ref nextObjectId)
                }
            };
        }

        static ObjectGroup BuildTriggers(int objectGroupId, MapData2D map, TilemapProperties properties, EventFormatter eventFormatter, ref int nextObjectId)
        {
            // TODO: Layer/group triggers by trigger type
            // TODO: Coalesce trigger regions
            int nextId = nextObjectId;
            var group = new ObjectGroup
            {
                Id = objectGroupId,
                Name = "Triggers",
                Objects = map.Zones
                    .Where(x => x.Chain != null)
                    .Select((x, i) => new TiledObject
                {
                    Id = nextId++,
                    Name = $"C{x.Chain.Id} {x.Trigger}",
                    Type = "Trigger",
                    X = x.X * properties.TileWidth,
                    Y = x.Y * properties.TileHeight,
                    Polygon = new Polygon(new[]
                    {
                        (0, 0),
                        (properties.TileWidth, 0),
                        (properties.TileWidth, properties.TileHeight),
                        (0, properties.TileHeight),
                    }),
                    Properties = new List<ObjectProperty>
                        {
                            new ObjectProperty("Script", eventFormatter.GetText(x.Chain.FirstEvent)),
                            new ObjectProperty("Trigger", x.Trigger.ToString()),
                            new ObjectProperty("Unk1", x.Unk1.ToString(CultureInfo.InvariantCulture)),
                        }
                }).ToList(),
            };
            nextObjectId = nextId;
            return group;
        }

        static ObjectGroup BuildNpcs(int objectGroupId, MapData2D map, TilemapProperties properties, EventFormatter eventFormatter, Func<AssetId, int> getGid, ref int nextObjectId)
        {
            int nextId = nextObjectId;
            int npcWidth = 32;
            int npcHeight = 48;
            var group = new ObjectGroup
            {
                Id = objectGroupId,
                Name = "NPCs",
                Objects = map.Npcs.Select(x =>
                {
                    var objProps = new List<ObjectProperty>();
                    objProps.Add(new ObjectProperty("Visual", x.Value.SpriteOrGroup.ToString()));
                    objProps.Add(new ObjectProperty("Flags", x.Value.Flags.ToString()));
                    objProps.Add(new ObjectProperty("Movement", ((int)x.Value.Movement).ToString(CultureInfo.InvariantCulture)));
                    objProps.Add(new ObjectProperty("Unk8", x.Value.Unk8.ToString(CultureInfo.InvariantCulture)));
                    objProps.Add(new ObjectProperty("Unk9", x.Value.Unk9.ToString(CultureInfo.InvariantCulture)));

                    if (!x.Value.Id.IsNone) objProps.Add(new ObjectProperty("Id", x.Value.Id.ToString()));
                    if (x.Value.Chain != null) objProps.Add(new ObjectProperty("Script", eventFormatter.GetText(x.Value.Chain.FirstEvent)));
                    if (x.Value.Sound > 0) objProps.Add(new ObjectProperty("Sound", x.Value.Sound.ToString(CultureInfo.InvariantCulture)));
                    // TODO: Path

                    return new TiledObject
                    {
                        Id = nextId++,
                        Gid = getGid(x.Value.SpriteOrGroup),
                        Name = $"NPC{x.Key} {x.Value.Id}",
                        Type = "NPC",
                        X = x.Value.Waypoints[0].X * properties.TileWidth,
                        Y = x.Value.Waypoints[0].Y * properties.TileHeight,
                        Width = npcWidth,
                        Height = npcHeight,
                        Properties = objProps
                    };
                }).ToList(),
            };
            nextObjectId = nextId;
            return group;
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