using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UAlbion.Api;
using UAlbion.Config;

// ReSharper disable StringLiteralTypo
// ReSharper disable UnusedMember.Global
#pragma warning disable CA1034 // Nested types should not be visible
#pragma warning disable CA2227 // Collection properties should be read only

namespace UAlbion.Formats.Exporters.Tiled;

[XmlRoot("tileset")]
public class Tileset
{
    [XmlIgnore] public string Filename { get; set; }
    [XmlIgnore] public int GidOffset { get; set; }
    [XmlElement("tileoffset", Order = 1)] public TileOffset Offset { get; set; }
    [XmlElement("grid", Order = 2)] public TiledGrid Grid { get; set; }
    [XmlElement("image", Order = 3)] public TilesetImage Image { get; set; }
    [XmlArray("terraintypes", Order = 4)] [XmlArrayItem("terrain")] public List<TerrainType> TerrainTypes { get; } = [];
    [XmlIgnore] public bool TerrainTypesSpecified => TerrainTypes is { Count: > 0 };
    [XmlElement("tile", Order = 5)] public List<Tile> Tiles { get; set; }
    [XmlIgnore] public bool TilesSpecified => Tiles is { Count: > 0 };
    [XmlArray("wangsets", Order = 6)] [XmlArrayItem("wangset")] public List<WangSet> WangSets { get; } = [];
    [XmlIgnore] public bool WangSetsSpecified => WangSets is { Count: > 0 };

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

    static TileFrame F(int id, int duration) => new() { Id = id, DurationMs = duration };
    static WangTile W(int id, string wang) => new() { TileId = id, WangId = wang };

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
        test.Tiles.Add(new Tile { Id = 216, Frames = [F(216, 180), F(217, 180), F(218, 180)] });

        test.WangSets.Add(new WangSet
        {
            Corners =
            [
                new() { Name = "", Color = "#ff0000", Tile = -1, Probability = 1 },
                new() { Name = "", Color = "#00ff00", Tile = -1, Probability = 1 },
                new() { Name = "", Color = "#0000ff", Tile = -1, Probability = 1 }
            ],
            Tiles =
            [
                W(8, "0x20202020"),
                W(9, "0x20202020"),
                W(10, "0x20202020"),
                W(11, "0x20202020")
            ]
        });

        return test;
    }

    public static Tileset Parse(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        using var xr = new XmlTextReader(stream);
        var serializer = new XmlSerializer(typeof(Tileset));
        return (Tileset)serializer.Deserialize(xr);
    }

    public static Tileset Load(string path, IFileSystem disk)
    {
        ArgumentNullException.ThrowIfNull(disk);
        using var stream = disk.OpenRead(path);
        var tilemap = Parse(stream);
        tilemap.Filename = path;
        return tilemap;
    }

    public void Save(string path, IFileSystem disk)
    {
        ArgumentNullException.ThrowIfNull(disk);
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
}
#pragma warning restore CA2227 // Collection properties should be read only
#pragma warning restore CA1034 // Nested types should not be visible
