using System.Xml.Serialization;

namespace UAlbion.Formats.Exporters.Tiled
{
    public class LayerData
    {
        [XmlAttribute("encoding")] public string Encoding { get; set; }
        [XmlText] public string Content { get; set; }
    }
}