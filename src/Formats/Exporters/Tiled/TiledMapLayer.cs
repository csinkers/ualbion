using System.Xml.Serialization;

namespace UAlbion.Formats.Exporters.Tiled;

public class TiledMapLayer
{
    [XmlAttribute("id")] public int Id { get; set; }
    [XmlAttribute("name")] public string Name { get; set; }
    [XmlAttribute("width")] public int Width { get; set; }
    [XmlAttribute("height")] public int Height { get; set; }
    [XmlAttribute("opacity")] public double Opacity { get; set; } = 1.0;
    [XmlElement("data")] public LayerData Data { get; set; }
}