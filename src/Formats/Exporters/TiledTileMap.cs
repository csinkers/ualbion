using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using UAlbion.Formats.Assets.Maps;

// ReSharper disable StringLiteralTypo
// ReSharper disable UnusedMember.Global
#pragma warning disable CA1034 // Nested types should not be visible
#pragma warning disable CA2227 // Collection properties should be read only

namespace UAlbion.Formats.Exporters
{
    public class TilemapProperties
    {
        public int TileWidth { get; set; }
        public int TileHeight { get; set; }
        public int Margin { get; set; }
        public int Spacing { get; set; }
        public string SheetPath { get; set; }
        public int SheetWidth { get; set; }
        public int SheetHeight { get; set; }
        public int FrameDurationMs { get; set; }
    }

    public class TileProperties
    {
        public string Name { get; set; }
        public string Source { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Frames { get; set; }
    }

    [XmlRoot("tileset")]
    public class TiledTileMap
    {
        public class TileFrame
        {
            [XmlAttribute("tileid")] public int Id { get; set; }
            [XmlAttribute("duration")] public int DurationMs { get; set; }
        }

        public class TileProperty
        {
            [XmlAttribute("name")] public string Name { get; set; }
            [XmlAttribute("type")] public string Type { get; set; }
            [XmlAttribute("value")] public string Value { get; set; }
        }

        public class TiledTile
        {
            [XmlAttribute("id")] public int Id { get; set; }
            [XmlAttribute("type")] public string Type { get; set; }
            [XmlAttribute("terrain")] public string Terrain { get; set; } // e.g. 0,1,0,3
            [XmlArray("animation")] [XmlArrayItem("frame")] public List<TileFrame> Frames { get; set; }
            [XmlIgnore] public bool FramesSpecified => Frames != null && Frames.Count > 0;
            [XmlArray("properties")] [XmlArrayItem("property")] public List<TileProperty> Properties { get; set; }
            [XmlIgnore] public bool PropertiesSpecified => Properties != null && Properties.Count > 0;
            [XmlElement("image")] public TilesetImage Image { get; set; }
        }

        public class TerrainType
        {
            [XmlAttribute("name")] public string Name { get; set; }
            [XmlAttribute("tile")] public int IndexTile { get; set; }
        }

        public class WangCorner
        {
            [XmlAttribute("name")] public string Name { get; set; }
            [XmlAttribute("color")] public string Color { get; set; }
            [XmlAttribute("tile")] public int Tile { get; set; }
            [XmlAttribute("probability")] public int Probability { get; set; }
        }

        public class WangTile
        {
            [XmlAttribute("tileid")] public int TileId { get; set; }
            [XmlAttribute("wangid")] public string WangId { get; set; }
        }

        public class WangSet
        {
            [XmlAttribute("name")] public string Name { get; set; }
            [XmlAttribute("tile")] public int Tile { get; set; }
            [XmlElement("wangcornercolor")] public List<WangCorner> Corners { get; set; }
            [XmlElement("wangtile")] public List<WangTile> Tiles { get; set; }
        }

        public class TilesetImage
        {
            [XmlAttribute("source")] public string Source { get; set; }
            [XmlAttribute("width")] public int Width { get; set; }
            [XmlAttribute("height")] public int Height { get; set; }
        }

        [XmlElement("image", Order = 1)] public TilesetImage Image { get; set; }
        [XmlArray("terraintypes", Order = 2)] [XmlArrayItem("terrain")] public List<TerrainType> TerrainTypes { get; } = new List<TerrainType>();
        [XmlIgnore] public bool TerrainTypesSpecified => TerrainTypes != null && TerrainTypes.Count > 0;
        [XmlElement("tile", Order = 3)] public List<TiledTile> Tiles { get; set; }
        [XmlIgnore] public bool TilesSpecified => Tiles != null && Tiles.Count > 0;
        [XmlArray("wangsets", Order = 4)] [XmlArrayItem("wangset")] public List<WangSet> WangSets { get; } = new List<WangSet>();
        [XmlIgnore] public bool WangSetsSpecified => WangSets != null && WangSets.Count > 0;

        [XmlAttribute("margin")] public int Margin { get; set; }
        [XmlIgnore] public bool MarginSpecified => Margin != 0;
        [XmlAttribute("name")] public string Name { get; set; }
        [XmlAttribute("spacing")] public int Spacing { get; set; }
        [XmlIgnore] public bool SpacingSpecified => Spacing != 0;
        [XmlAttribute("tilewidth")] public int TileWidth { get; set; }
        [XmlAttribute("tileheight")] public int TileHeight { get; set; }
        [XmlAttribute("columns")] public int Columns { get; set; }
        [XmlAttribute("tilecount")] public int TileCount { get; set; }
        [XmlAttribute("version")] public string Version { get; set; }
        [XmlAttribute("tiledversion")] public string TiledVersion { get; set; }
        [XmlAttribute("backgroundcolor")] public string BackgroundColor { get; set; }

        static TileFrame F(int id, int duration) => new TileFrame { Id = id, DurationMs = duration };
        static WangTile W(int id, string wang) => new WangTile { TileId = id, WangId = wang };

        public static TiledTileMap BuildExample()
        {
            var test = new TiledTileMap
            {
                Name = "Test",
                TileWidth = 18,
                TileHeight = 18,
                Columns = 56,
                Version = "1.4",
                TiledVersion = "1.4.2",
                TileCount = 3136,
                BackgroundColor = "#000000",
                Image = new TilesetImage { Source = "0_0_Outdoors.png", Width = 1024, Height = 1024 }
            };

            test.TerrainTypes.Add(new TerrainType { Name = "Grass", IndexTile = 9 });
            test.TerrainTypes.Add(new TerrainType { Name = "Water", IndexTile = 240 });
            test.TerrainTypes.Add(new TerrainType { Name = "Cliffs", IndexTile = 439 });
            test.TerrainTypes.Add(new TerrainType { Name = "Dirt", IndexTile = 205 });
            test.TerrainTypes.Add(new TerrainType { Name = "Road", IndexTile = 301 });

            test.Tiles.Add(new TiledTile { Id = 8, Terrain = "0,0,0,0" });
            test.Tiles.Add(new TiledTile { Id = 216, Frames = new List<TileFrame> { F(216, 180), F(217, 180), F(218, 180) } });

            test.WangSets.Add(new WangSet
            {
                Corners = new List<WangCorner>
            {
                new WangCorner { Name = "", Color = "#ff0000", Tile = -1, Probability = 1 },
                new WangCorner { Name = "", Color = "#00ff00", Tile = -1, Probability = 1 },
                new WangCorner { Name = "", Color = "#0000ff", Tile = -1, Probability = 1 },
            },
                Tiles = new List<WangTile>
            {
                W(8, "0x20202020"),
                W(9, "0x20202020"),
                W(10, "0x20202020"),
                W(11, "0x20202020"),
            }
            });

            return test;
        }

        public static TiledTileMap Load(string path)
        {
            using var stream = File.OpenRead(path);
            using var xr = new XmlTextReader(stream);
            var serializer = new XmlSerializer(typeof(TiledTileMap));
            return (TiledTileMap)serializer.Deserialize(xr);
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
            var serializer = new XmlSerializer(typeof(TiledTileMap));
            serializer.Serialize(tw, this, ns);
        }

        static TileProperty Prop(string name, string value, string type = null) => new TileProperty { Name = name, Type = type, Value = value };
        public static TiledTileMap FromTileset(TilesetData tileset, TilemapProperties properties)
        {
            if (tileset == null) throw new ArgumentNullException(nameof(tileset));
            if (properties == null) throw new ArgumentNullException(nameof(properties));
            // +1 to go from max index to count, then + another so we have a blank tile at the end.

            int count = tileset.Tiles.Where(x => x.ImageNumber != 0xffff).Max(x => x.ImageNumber + x.FrameCount - 1) + 2;
            int columns = (properties.SheetWidth - 2 * properties.Margin) / (properties.TileWidth + properties.Spacing);

            static List<TileProperty> Props(TileData x)
            {
                var properties = new List<TileProperty>();
                if (x.Flags != 0) properties.Add(Prop("Flags", x.Flags.ToString()));
                properties.Add(Prop("Layer", x.Layer.ToString()));
                if (x.Collision != 0) properties.Add(Prop("Passability", ((int)x.Collision).ToString(CultureInfo.InvariantCulture), "int"));
                properties.Add(Prop("Type", x.Layer.ToString()));
                properties.Add(Prop("Unk7", x.Unk7.ToString(CultureInfo.InvariantCulture), "int"));
                return properties;
            }

            var grouped = tileset.Tiles.GroupBy(x => x.ImageNumber).Where(x => x.Count() > 1);

            return new TiledTileMap
            {
                Name = tileset.Id.ToString(),
                Version = "1.4",
                TiledVersion = "1.4.2",
                TileCount = count,
                TileWidth = properties.TileWidth,
                TileHeight = properties.TileHeight,
                Margin = properties.Margin,
                Spacing = properties.Spacing,
                Columns = columns,
                BackgroundColor = "#000000",
                Image = new TilesetImage
                {
                    Source = properties.SheetPath,
                    Width = properties.SheetWidth,
                    Height = properties.SheetHeight
                },
                Tiles = tileset.Tiles
                    .Where(x => x.ImageNumber != 0xffff && x.ImageNumber != 0)
                    .Select(x => new TiledTile
                    {
                        Id = x.ImageNumber,
                        Frames = x.FrameCount == 1 
                            ? null 
                            : Enumerable.Range(x.ImageNumber, x.FrameCount)
                              .Select(f => new TileFrame { Id = f, DurationMs = properties.FrameDurationMs })
                              .ToList(),
                        Properties = Props(x)
                    }).ToList(),
                // terrain
                // wang sets
            };
        }

        public static TiledTileMap FromSprites(string name, string type, IList<TileProperties> tiles) // (name, source, w, h)
        {
            if (tiles == null) throw new ArgumentNullException(nameof(tiles));
            return new TiledTileMap
            {
                Name = name,
                Version = "1.4",
                TiledVersion = "1.4.2",
                TileCount = tiles.Count,
                TileWidth = tiles.Max(x => x.Width),
                TileHeight = tiles.Max(x => x.Height),
                Columns = 1,
                BackgroundColor = "#000000",
                Tiles = tiles.Select((x, i) => new TiledTile
                    {
                        Id = i,
                        Type = type,
                        Image = new TilesetImage
                        {
                            Source = x.Source,
                            Width = x.Width,
                            Height = x.Height
                        },
                        Properties = new List<TileProperty>
                        {
                            Prop("Visual", x.Name)
                        }
                    }).ToList(),
            };
        }
    }
}
#pragma warning restore CA2227 // Collection properties should be read only
#pragma warning restore CA1034 // Nested types should not be visible
