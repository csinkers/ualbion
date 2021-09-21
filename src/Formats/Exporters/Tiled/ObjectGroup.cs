using System;
using System.Collections.Generic;
using System.Xml.Serialization;

#pragma warning disable CA2227 // Collection properties should be read only
namespace UAlbion.Formats.Exporters.Tiled
{
    public class ObjectGroup
    {
        [XmlIgnore] public bool Hidden { get => Visible == 0; set => Visible = value ? 0 : 1; }
        [XmlAttribute("id")] public int Id { get; set; }
        [XmlAttribute("name")] public string Name { get; set; }
        [XmlAttribute("color")] public string Color { get; set; }
        [XmlAttribute("visible")] public int Visible { get; set; } = 1; // 0 = hidden
        [XmlIgnore] public bool VisibleSpecified => Visible == 0;
        [XmlAttribute("tintcolor")] public string TintColor { get; set; }
        [XmlAttribute("opacity")] public float Opacity { get; set; } = 1.0f;
        [XmlIgnore] public bool OpacitySpecified => Math.Abs(Opacity - 1.0f) > float.Epsilon;
        [XmlElement("object")] public List<MapObject> Objects { get; set; }
    }
}
#pragma warning restore CA2227 // Collection properties should be read only