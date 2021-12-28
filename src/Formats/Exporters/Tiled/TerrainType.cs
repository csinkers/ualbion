using System.Xml.Serialization;

namespace UAlbion.Formats.Exporters.Tiled;

public class TerrainType
{
    [XmlAttribute("name")] public string Name { get; set; }
    [XmlAttribute("tile")] public int IndexTile { get; set; }
}