using System.Xml.Serialization;

namespace UAlbion.Formats.Exporters.Tiled
{
    public enum TiledPropertyType
    {
#pragma warning disable CA1720 // Identifier contains type name
        [XmlEnum("string")] String,
        [XmlEnum("int")]    Int,
        [XmlEnum("float")]  Float,
        [XmlEnum("bool")]   Bool,
        [XmlEnum("color")]  Color,
        [XmlEnum("file")]   File,
        [XmlEnum("object")] Object
#pragma warning restore CA1720 // Identifier contains type name
    }
}