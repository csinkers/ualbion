using System.Xml.Serialization;

namespace UAlbion.Formats.Exporters.Tiled
{
    public class WangTile
    {
        [XmlAttribute("tileid")] public int TileId { get; set; }
        [XmlAttribute("wangid")] public string WangId { get; set; }
    }
}