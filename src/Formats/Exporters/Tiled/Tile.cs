using System.Collections.Generic;
using System.Xml.Serialization;

#pragma warning disable CA2227 // Collection properties should be read only
namespace UAlbion.Formats.Exporters.Tiled;

public class Tile
{
    [XmlAttribute("id")] public int Id { get; set; }
    [XmlAttribute("type")] public string Type { get; set; }
    [XmlAttribute("terrain")] public string Terrain { get; set; } // e.g. 0,1,0,3
    [XmlArray("animation")] [XmlArrayItem("frame")] public List<TileFrame> Frames { get; set; }
    [XmlIgnore] public bool FramesSpecified => Frames is { Count: > 0 };
    [XmlArray("properties")] [XmlArrayItem("property")] public List<TileProperty> Properties { get; set; }
    [XmlIgnore] public bool PropertiesSpecified => Properties is { Count: > 0 };
    [XmlElement("image")] public TilesetImage Image { get; set; }
}
#pragma warning restore CA2227 // Collection properties should be read only