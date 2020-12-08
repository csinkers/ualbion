using System.Xml.Serialization;

namespace UAlbion.Formats.Exporters.Tiled
{
    public class TilesetImage
    {
        [XmlAttribute("source")] public string Source { get; set; }
        [XmlAttribute("width")] public int Width { get; set; }
        [XmlAttribute("height")] public int Height { get; set; }
    }
}