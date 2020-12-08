using System.Collections.Generic;
using System.Xml.Serialization;

#pragma warning disable CA2227 // Collection properties should be read only
namespace UAlbion.Formats.Exporters.Tiled
{
    public class MapObject
    {
        [XmlAttribute("id")] public int Id { get; set; }
        [XmlAttribute("gid")] public int Gid { get; set; }
        [XmlIgnore] public bool GidSpecified => Gid != 0;
        [XmlAttribute("name")] public string Name { get; set; }
        [XmlAttribute("type")] public string Type { get; set; }
        [XmlAttribute("x")] public double X { get; set; }
        [XmlAttribute("y")] public double Y { get; set; }
        [XmlAttribute("width")] public double Width { get; set; }
        [XmlAttribute("height")] public double Height { get; set; }
        [XmlArray("properties")] [XmlArrayItem("property")] public List<ObjectProperty> Properties { get; set; }
        [XmlElement("polygon")] public Polygon Polygon { get; set; }
    }
}
#pragma warning restore CA2227 // Collection properties should be read only