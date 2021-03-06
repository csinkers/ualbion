using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UAlbion.Formats.Assets;

#pragma warning disable CA2227 // Collection properties should be read only
namespace UAlbion.Formats.Exporters.Tiled
{
    public class Stamp
    {
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("variations")] public List<Variation> Variations { get; private set; } = new List<Variation>();

        public Stamp() { }
        public Stamp(int blockId, Block block, Tileset tileset)
        {
            if (block == null) throw new ArgumentNullException(nameof(block));
            if (tileset == null) throw new ArgumentNullException(nameof(tileset));

            var (underlay, overlay) = FormatUtil.FromPacked(block.RawLayout);
            for (int i = 0; i < underlay.Length; i++) if (underlay[i] == 1) underlay[i] = 0;
            for (int i = 0; i < overlay.Length; i++) if (overlay[i] == 1) overlay[i] = 0;

            Name = $"{blockId:000}_{block.Width}x{block.Height}";
            Variations.Add(new Variation
            {
                Map = new VariationMap
                {

                    Width = block.Width,
                    Height = block.Height,
                    TileWidth = tileset.TileWidth,
                    TileHeight = tileset.TileHeight,
                    NextLayerId = 3,
                    NextObjectId = 1,
                    Tilesets = new List<VariationTileset> 
                    {
                        new VariationTileset { FirstGid = 1, Source = tileset.Filename }
                    },
                    Layers = new List<VariationLayer> 
                    {
                        new VariationLayer
                        {
                            Id = 1,
                            Name = "Underlay",
                            Width = block.Width,
                            Height = block.Height,
                            Data = ZipUtil.Deflate(underlay)
                        },
                        new VariationLayer
                        {
                            Id = 2,
                            Name = "Overlay",
                            Width = block.Width,
                            Height = block.Height,
                            Data = ZipUtil.Deflate(overlay)
                        }
                    }
                }
            });
        }

        public Block ToBlock()
        {
            var map = Variations[0].Map;
            var underlayLayer = map.Layers.Find(x => x.Name == "Underlay");
            var overlayLayer = map.Layers.Find(x => x.Name == "Overlay");
            var underlay = ZipUtil.Inflate(underlayLayer.Data);
            var overlay = ZipUtil.Inflate(overlayLayer.Data);
            return new Block
            {
                Width = (byte) map.Width,
                Height = (byte) map.Height,
                RawLayout = FormatUtil.ToPacked(underlay, overlay)
            };
        }

        /*
        {
          "name":"schr",
          "variations":[
          {
            "map":
            {
              "compressionlevel":-1,
              "height":7,
              "infinite":false,
              "layers":[
              {
                "compression":"zlib",
                "data":"eJxjYMAEW5gZGLYyI/jbgOztSPwdQPZOKH8XkN4NxHuAeC8Q7wPi/UB8AIgPAvEhID4MxEeg6gEpPwyw",
                "encoding":"base64",
                "height":7,
                "id":1,
                "name":"Under",
                "opacity":1,
                "type":"tilelayer",
                "visible":true,
                "width":4,
                "x":0,
                "y":0
              },
              {
                "compression":"zlib",
                "data":"eJxjYGBgOM3JwHAGiIkBZ5HUnSNSDzIAABF/A1c=",
                "encoding":"base64",
                "height":7,
                "id":2,
                "name":"Over",
                "opacity":1,
                "type":"tilelayer",
                "visible":true,
                "width":4,
                "x":0,
                "y":0
              }],
              "nextlayerid":3,
              "nextobjectid":1,
              "orientation":"orthogonal",
              "renderorder":"right-down",
              "tiledversion":"1.4.2",
              "tileheight":16,
              "tilesets":[
              {
                "firstgid":1,
                "source":"../../Tilesets/3_Indoors.tsx"
              }],
              "tilewidth":16,
              "type":"map",
              "version":1.4,
              "width":4
            },
            "probability":1
          }]
        }
         */
        public static Stamp Load(string path) => Parse(File.ReadAllText(path));
        public static Stamp Parse(string json) => JsonConvert.DeserializeObject<Stamp>(json);

        public void Save(string path)
        {
            using var stream = File.Open(path, FileMode.Create);
            using var sw = new StreamWriter(stream);
            Serialize(sw);
        }

        public void Serialize(TextWriter tw)
        {
            var serializer = new JsonSerializer();
            serializer.Serialize(tw, this);
        }
    }

    public class Variation
    {
        [JsonProperty("probability")] public int Probability { get; set; } = 1;
        [JsonProperty("map")] public VariationMap Map { get; set; }
    }

    public class VariationMap
    {
        [JsonProperty("width")] public int Width { get; set; }
        [JsonProperty("height")] public int Height { get; set; }
        [JsonProperty("tilewidth")] public int TileWidth { get; set; }
        [JsonProperty("tileheight")] public int TileHeight { get; set; }
        [JsonProperty("nextlayerid")] public int NextLayerId { get; set; }
        [JsonProperty("nextobjectid")] public int NextObjectId { get; set; }
        [JsonProperty("layers")] public List<VariationLayer> Layers { get; set; } = new List<VariationLayer>();
        [JsonProperty("tilesets")] public List<VariationTileset> Tilesets { get; set; } = new List<VariationTileset>();

        [JsonProperty("type")] public string Type { get; set; } = "map";
        [JsonProperty("compressionlevel")] public int Compressionlevel { get; set; } = -1;
        [JsonProperty("orientation")] public string Orientation { get; set; } = "orthogonal";
        [JsonProperty("renderorder")] public string Renderorder { get; set; } = "right-down";
        [JsonProperty("tiledversion")] public string TiledVersion { get; set; } = "1.4.2";
        [JsonProperty("version")] public float Version { get; set; } = 1.4f;
        [JsonProperty("infinite")] public bool Infinite { get; set; } = false;
    }

    public class VariationLayer
    {
        [JsonProperty("id")] public int Id { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("width")] public int Width { get; set; }
        [JsonProperty("height")] public int Height { get; set; }
        [JsonProperty("data")] public byte[] Data { get; set; }

        [JsonProperty("x")] public int X { get; set; } = 0;
        [JsonProperty("y")] public int Y { get; set; } = 0;
        [JsonProperty("type")] public string Type { get; set; } = "tilelayer";
        [JsonProperty("compression")] public string Compression { get; set; } = "zlib";
        [JsonProperty("encoding")] public string Encoding { get; set; } = "base64";
        [JsonProperty("opacity")] public int Opacity { get; set; } = 1;
        [JsonProperty("visible")] public bool Visible { get; set; } = true;
    }

    public class VariationTileset
    {
        [JsonProperty("firstgid")] public int FirstGid { get; set; }
        [JsonProperty("source")] public string Source { get; set; }
    }
}
#pragma warning restore CA2227 // Collection properties should be read only
