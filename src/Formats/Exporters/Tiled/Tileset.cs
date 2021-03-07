using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets.Maps;

// ReSharper disable StringLiteralTypo
// ReSharper disable UnusedMember.Global
#pragma warning disable CA1034 // Nested types should not be visible
#pragma warning disable CA2227 // Collection properties should be read only

namespace UAlbion.Formats.Exporters.Tiled
{
    [XmlRoot("tileset")]
    public class Tileset
    {
        [XmlIgnore] public string Filename { get; set; }
        [XmlIgnore] public int GidOffset { get; set; }
        [XmlElement("image", Order = 1)] public TilesetImage Image { get; set; }
        [XmlArray("terraintypes", Order = 2)] [XmlArrayItem("terrain")] public List<TerrainType> TerrainTypes { get; } = new List<TerrainType>();
        [XmlIgnore] public bool TerrainTypesSpecified => TerrainTypes != null && TerrainTypes.Count > 0;
        [XmlElement("tile", Order = 3)] public List<Tile> Tiles { get; set; }
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

        public static Tileset BuildExample()
        {
            var test = new Tileset
            {
                Name = "Test",
                TileWidth = 16,
                TileHeight = 16,
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

            test.Tiles.Add(new Tile { Id = 8, Terrain = "0,0,0,0" });
            test.Tiles.Add(new Tile { Id = 216, Frames = new List<TileFrame> { F(216, 180), F(217, 180), F(218, 180) } });

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

        public static Tileset Load(string path, IFileSystem disk)
        {
            if (disk == null) throw new ArgumentNullException(nameof(disk));
            using var stream = disk.OpenRead(path);
            using var xr = new XmlTextReader(stream);
            var serializer = new XmlSerializer(typeof(Tileset));
            var tilemap = (Tileset)serializer.Deserialize(xr);
            tilemap.Filename = path;
            return tilemap;
        }

        public void Save(string path, IFileSystem disk)
        {
            if (disk == null) throw new ArgumentNullException(nameof(disk));
            var dir = Path.GetDirectoryName(path);
            foreach (var tile in Tiles)
                if (!string.IsNullOrEmpty(tile.Image?.Source))
                    tile.Image.Source = ConfigUtil.GetRelativePath(tile.Image.Source, dir, false);

            if (!string.IsNullOrEmpty(Image?.Source))
                Image.Source = ConfigUtil.GetRelativePath(Image.Source, dir, false);

            using var stream = disk.OpenWriteTruncate(path);
            using var sw = new StreamWriter(stream);
            Serialize(sw);
        }

        public void Serialize(TextWriter tw)
        {
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            var serializer = new XmlSerializer(typeof(Tileset));
            serializer.Serialize(tw, this, ns);
        }

        static TileProperty Prop(string name, string value, string type = null) => new TileProperty { Name = name, Type = type, Value = value };
        public static Tileset FromTileset(TilesetData tileset, TilemapProperties properties)
        {
            if (tileset == null) throw new ArgumentNullException(nameof(tileset));
            if (properties == null) throw new ArgumentNullException(nameof(properties));
            // +1 to go from max index to count, then + another so we have a blank tile at the end.

            static List<TileProperty> Props(TileData x)
            {
                var properties = new List<TileProperty>();
                if (x.Flags != 0) properties.Add(Prop("Flags", x.Flags.ToString()));
                properties.Add(Prop("Layer", x.Layer.ToString()));
                if (x.Collision != 0) properties.Add(Prop("Passability", ((int)x.Collision).ToString(CultureInfo.InvariantCulture), "int"));
                properties.Add(Prop("Type", x.Type.ToString()));
                properties.Add(Prop("Unk7", x.Unk7.ToString(CultureInfo.InvariantCulture), "int"));
                return properties;
            }

            List<Tile> tiles = 
                tileset.Tiles
                .Where(x => !x.IsBlank)
                .Select(x => new Tile
                {
                    Id = x.Index,
                    Properties = Props(x),
                    Image = new TilesetImage
                    {
                        Width = properties.TileWidth,
                        Height = properties.TileHeight,
                        Source = x.ImageNumber == 0xffff  
                            ? properties.BlankTilePath 
                            : string.Format(CultureInfo.InvariantCulture,
                                properties.GraphicsTemplate,
                                properties.TilesetId, 
                                x.ImageNumber)
                    },
                }).ToList();

            // Add tiles for the extra frames of animated tiles
            int nextId = tileset.Tiles[tileset.Tiles.Count - 1].Index + 1;
            int maxTile = tiles.Count;
            for (int i = 0; i < maxTile; i++)
            {
                var tile = tiles[i];
                var sourceTile = tileset.Tiles[tile.Id];
                if (sourceTile.FrameCount <= 1)
                    continue;

                tile.Frames = new List<TileFrame> { new TileFrame(tile.Id, properties.FrameDurationMs) };
                for (int f = 1; f < sourceTile.FrameCount; f++)
                {
                    tiles.Add(new Tile
                    {
                        Id = nextId,
                        Properties = new List<TileProperty> { Prop("Frame", "true", "boolean") },
                        Image = new TilesetImage
                        {
                            Width = properties.TileWidth,
                            Height = properties.TileHeight,
                            Source = string.Format(CultureInfo.InvariantCulture,
                                properties.GraphicsTemplate,
                                properties.TilesetId,
                                sourceTile.ImageNumber + f)
                        }
                    });
                    tile.Frames.Add(new TileFrame(nextId, properties.FrameDurationMs));
                    nextId++;
                }
            }

            return new Tileset
            {
                Name = tileset.Id.ToString(),
                Version = "1.4",
                TiledVersion = "1.4.2",
                TileCount = tiles.Count,
                TileWidth = properties.TileWidth,
                TileHeight = properties.TileHeight,
                // Margin = properties.Margin,
                // Spacing = properties.Spacing,
                // Columns = columns,
                BackgroundColor = "#000000",
                Tiles = tiles,
                // TODO: Terrain
                // wang sets
            };
        }

        public static Tileset FromSprites(string name, string type, IList<TileProperties> tiles) // (name, source, w, h)
        {
            if (tiles == null) throw new ArgumentNullException(nameof(tiles));
            return new Tileset
            {
                Name = name,
                Version = "1.4",
                TiledVersion = "1.4.2",
                TileCount = tiles.Count,
                TileWidth = tiles.Max(x => x.Width),
                TileHeight = tiles.Max(x => x.Height),
                Columns = 1,
                BackgroundColor = "#000000",
                Tiles = tiles.Select((x, i) => new Tile
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
