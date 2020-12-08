using System.Xml.Serialization;

namespace UAlbion.Formats.Exporters.Tiled
{
    public class MapLayer
    {
        [XmlAttribute("id")] public int Id { get; set; }
        [XmlAttribute("name")] public string Name { get; set; }
        [XmlAttribute("width")] public int Width { get; set; }
        [XmlAttribute("height")] public int Height { get; set; }
        [XmlElement("data")] public LayerData Data { get; set; }
    }
}