using System.Xml.Serialization;

namespace UAlbion.Formats.Exporters.Tiled;

public class TileOffset
{
    [XmlAttribute("x")] public int X { get; set; }
    [XmlAttribute("y")] public int Y { get; set; }
}