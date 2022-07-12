using System.Xml.Serialization;

namespace UAlbion.Formats.Exporters.Tiled;

public class LayerData
{
    [XmlAttribute("encoding")] public string Encoding { get; set; }
    [XmlAttribute("compression")] public string Compression { get; set; }
    [XmlText] public string Content { get; set; }
}