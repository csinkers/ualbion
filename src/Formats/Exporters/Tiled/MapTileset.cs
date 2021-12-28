using System.Xml.Serialization;

namespace UAlbion.Formats.Exporters.Tiled;

public class MapTileset
{
    [XmlAttribute("firstgid")] public int FirstGid { get; set; }
    [XmlAttribute("source")] public string Source { get; set; }
}