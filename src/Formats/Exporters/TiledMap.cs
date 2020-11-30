using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
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
        [XmlElement("tileset")] public TiledMapTileset Tileset { get; set; }
        [XmlElement("layer")] public List<MapLayer> Layers { get; set; }

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

        public static TiledMap FromAlbionMap(MapData2D map, TilesetData tileset, TilemapProperties properties, string tilesetPath)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));
            if (tileset == null) throw new ArgumentNullException(nameof(tileset));
            if (properties == null) throw new ArgumentNullException(nameof(properties));

            ushort blankTileIndex = (ushort)(tileset.Tiles.Max(x => x.ImageNumber + x.FrameCount - 1) + 1);
            var layers = new List<MapLayer>();
            layers.Add(new MapLayer
            {
                Id = 1,
                Name = "Underlay",
                Width = map.Width,
                Height = map.Height,
                Data = new LayerData { Encoding = "csv", Content = BuildCsvData(map, tileset, false, blankTileIndex) }
            });
            layers.Add(new MapLayer
            {
                Id = 2,
                Name = "Overlay",
                Width = map.Width,
                Height = map.Height,
                Data = new LayerData { Encoding = "csv", Content = BuildCsvData(map, tileset, true, blankTileIndex) }
            });

            return new TiledMap
            {
                TiledVersion = "1.4.2",
                Version = "1.4",
                Width = map.Width,
                Height = map.Height,
                TileWidth = properties.TileWidth,
                TileHeight = properties.TileHeight,
                Infinite = 0,
                NextLayerId = layers.Count + 1,
                NextObjectId = 1,
                Orientation = "orthogonal",
                RenderOrder = "right-down",
                Tileset = new TiledMapTileset { FirstGid = 1, Source = tilesetPath },
                Layers = layers
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