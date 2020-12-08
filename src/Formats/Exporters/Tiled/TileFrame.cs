using System.Xml.Serialization;

namespace UAlbion.Formats.Exporters.Tiled
{
    public class TileFrame
    {
        [XmlAttribute("tileid")] public int Id { get; set; }
        [XmlAttribute("duration")] public int DurationMs { get; set; }
    }
}