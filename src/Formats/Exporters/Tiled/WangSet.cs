using System.Collections.Generic;
using System.Xml.Serialization;

#pragma warning disable CA2227 // Collection properties should be read only
namespace UAlbion.Formats.Exporters.Tiled;

public class WangSet
{
    [XmlAttribute("name")] public string Name { get; set; }
    [XmlAttribute("tile")] public int Tile { get; set; }
    [XmlElement("wangcornercolor")] public List<WangCorner> Corners { get; set; }
    [XmlElement("wangtile")] public List<WangTile> Tiles { get; set; }
}
#pragma warning restore CA2227 // Collection properties should be read only