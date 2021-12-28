using System.Xml.Serialization;

namespace UAlbion.Formats.Exporters.Tiled;

public class WangCorner
{
    [XmlAttribute("name")] public string Name { get; set; }
    [XmlAttribute("color")] public string Color { get; set; }
    [XmlAttribute("tile")] public int Tile { get; set; }
    [XmlAttribute("probability")] public int Probability { get; set; }
}