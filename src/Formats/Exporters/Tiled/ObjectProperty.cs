using System;
using System.Xml.Serialization;

namespace UAlbion.Formats.Exporters.Tiled
{
    public class ObjectProperty
    {
        public ObjectProperty() { }
        public ObjectProperty(string key, string value)
        {
            Name = key;
            if (value != null && (value.Contains('\n', StringComparison.Ordinal) || value.Contains('\r', StringComparison.Ordinal)))
                MultiLine = value;
            else
                Value = value;
        }

        [XmlAttribute("name")] public string Name { get; set; }
        [XmlAttribute("value")] public string Value { get; set; }
        [XmlText] public string MultiLine { get; set; }
    }
}