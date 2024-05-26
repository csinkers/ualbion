using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UAlbion.Api;
using UAlbion.Config;

#pragma warning disable CA2227 // Collection properties should be read only
namespace UAlbion.Formats.Exporters.Tiled;

[XmlRoot("map")]
public class Map : ITiledPropertySource
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
    [XmlArray("properties")] [XmlArrayItem("property")] public List<TiledProperty> Properties { get; set; }
    [XmlElement("tileset")] public List<MapTileset> Tilesets { get; set; }
    [XmlElement("layer")] public List<TiledMapLayer> Layers { get; set; }
    [XmlElement("objectgroup")] public List<ObjectGroup> ObjectGroups { get; set; }

    public static Map Parse(Stream stream)
    {
        using var xr = new XmlTextReader(stream);
        var serializer = new XmlSerializer(typeof(Map));
        return (Map)serializer.Deserialize(xr);
    }

    public static Map Load(string path, IFileSystem disk)
    {
        ArgumentNullException.ThrowIfNull(disk);
        using var stream = disk.OpenRead(path);
        return Parse(stream);
    }

    public void Save(string path, IFileSystem disk)
    {
        ArgumentNullException.ThrowIfNull(disk);
        var dir = Path.GetDirectoryName(path);
        foreach (var tileset in Tilesets)
            tileset.Source = ConfigUtil.GetRelativePath(tileset.Source, dir, false);

        using var stream = disk.OpenWriteTruncate(path);
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
}
#pragma warning restore CA2227 // Collection properties should be read only
