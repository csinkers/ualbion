using System.Xml.Serialization;

namespace UAlbion.Formats.Exporters.Tiled
{
    public class TileProperty
    {
        [XmlAttribute("name")] public string Name { get; set; }
        [XmlAttribute("type")] public string Type { get; set; }
        [XmlAttribute("value")] public string Value { get; set; }
    }
}