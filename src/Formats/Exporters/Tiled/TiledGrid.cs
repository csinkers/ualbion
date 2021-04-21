using System.Xml.Serialization;

namespace UAlbion.Formats.Exporters.Tiled
{
    public class TiledGrid
    {
        [XmlAttribute("orientation")] public string Orientation { get; set; }
        [XmlAttribute("width")] public int Width { get; set; }
        [XmlAttribute("height")] public int Height { get; set; }
    }
}