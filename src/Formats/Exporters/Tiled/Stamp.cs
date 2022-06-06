using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using UAlbion.Api;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;

#pragma warning disable CA2227 // Collection properties should be read only
namespace UAlbion.Formats.Exporters.Tiled;

public class Stamp
{
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonInclude, JsonPropertyName("variations")] public List<Variation> Variations { get; private set; } = new();

    public Stamp() { }
    public Stamp(int blockId, Block block, Tileset tileset)
    {
        if (block == null) throw new ArgumentNullException(nameof(block));
        if (tileset == null) throw new ArgumentNullException(nameof(tileset));

        var underlay = MapTile.ToInts(block.Tiles, false);
        var overlay = MapTile.ToInts(block.Tiles, true);

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
                    new() { FirstGid = 1, Source = tileset.Filename }
                },
                Layers = new List<VariationLayer> 
                {
                    new()
                    {
                        Id = 1,
                        Name = "Underlay",
                        Width = block.Width,
                        Height = block.Height,
                        Data = ZipUtil.Deflate(underlay)
                    },
                    new()
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
        if (underlayLayer == null) throw new FormatException($"A layer named \"Underlay\" was expected in the stamp file");
        if (overlayLayer == null) throw new FormatException($"A layer named \"Overlay\" was expected in the stamp file");

        if (underlayLayer.Compression == CompressionFormat.Zlib)
        {
            var underlay = ZipUtil.Inflate(underlayLayer.Data);
            var overlay = ZipUtil.Inflate(overlayLayer.Data);
            var tiles = MapTile.FromInts(underlay, overlay);
            return new Block((byte) map.Width, (byte) map.Height, tiles);
        }
        else
        {
            throw new NotImplementedException();
        }
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
    public static Stamp Load(string path, IFileSystem disk, IJsonUtil jsonUtil)
    {
        if (disk == null) throw new ArgumentNullException(nameof(disk));
        return Parse(disk.ReadAllBytes(path), jsonUtil);
    }

    public static Stamp Parse(byte[] json, IJsonUtil jsonUtil)
    {
        if (jsonUtil == null) throw new ArgumentNullException(nameof(jsonUtil));
        return jsonUtil.Deserialize<Stamp>(json);
    }

    public void Save(string path, IFileSystem disk, IJsonUtil jsonUtil)
    {
        if (disk == null) throw new ArgumentNullException(nameof(disk));
        disk.WriteAllText(path, Serialize(jsonUtil));
    }

    public string Serialize(IJsonUtil jsonUtil)
    {
        if (jsonUtil == null) throw new ArgumentNullException(nameof(jsonUtil));
        return jsonUtil.Serialize(this);
    }
}

public class Variation
{
    [JsonPropertyName("probability")] public int Probability { get; set; } = 1;
    [JsonPropertyName("map")] public VariationMap Map { get; set; }
}

public class VariationMap
{
    [JsonPropertyName("width")] public int Width { get; set; }
    [JsonPropertyName("height")] public int Height { get; set; }
    [JsonPropertyName("tilewidth")] public int TileWidth { get; set; }
    [JsonPropertyName("tileheight")] public int TileHeight { get; set; }
    [JsonPropertyName("nextlayerid")] public int NextLayerId { get; set; }
    [JsonPropertyName("nextobjectid")] public int NextObjectId { get; set; }
    [JsonPropertyName("layers")] public List<VariationLayer> Layers { get; set; } = new();
    [JsonPropertyName("tilesets")] public List<VariationTileset> Tilesets { get; set; } = new();

    [JsonPropertyName("type")] public string Type { get; set; } = "map";
    [JsonPropertyName("compressionlevel")] public int Compressionlevel { get; set; } = -1;
    [JsonPropertyName("orientation")] public string Orientation { get; set; } = "orthogonal";
    [JsonPropertyName("renderorder")] public string Renderorder { get; set; } = "right-down";
    [JsonPropertyName("tiledversion")] public string TiledVersion { get; set; } = "1.4.2";
    [JsonPropertyName("version")] public float Version { get; set; } = 1.4f;
    [JsonPropertyName("infinite")] public bool Infinite { get; set; } = false;
}

public class VariationLayer
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("width")] public int Width { get; set; }
    [JsonPropertyName("height")] public int Height { get; set; }
    [JsonPropertyName("data")] public byte[] Data { get; set; }

    [JsonPropertyName("x")] public int X { get; set; } = 0;
    [JsonPropertyName("y")] public int Y { get; set; } = 0;
    [JsonPropertyName("type")] public string Type { get; set; } = "tilelayer";
    [JsonPropertyName("compression")] public string Compression { get; set; } = Tiled.CompressionFormat.Zlib;
    [JsonPropertyName("encoding")] public string Encoding { get; set; } = "base64";
    [JsonPropertyName("opacity")] public int Opacity { get; set; } = 1;
    [JsonPropertyName("visible")] public bool Visible { get; set; } = true;
}

public class VariationTileset
{
    [JsonPropertyName("firstgid")] public int FirstGid { get; set; }
    [JsonPropertyName("source")] public string Source { get; set; }
}
#pragma warning restore CA2227 // Collection properties should be read only
