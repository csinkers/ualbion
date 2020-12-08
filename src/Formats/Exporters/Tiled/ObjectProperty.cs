using System.Linq;
using System.Xml.Serialization;

namespace UAlbion.Formats.Exporters.Tiled
{
    public class ObjectProperty
    {
        public ObjectProperty() { }
        public ObjectProperty(string key, string value)
        {
            Name = key;
            if (value.Contains('\n') || value.Contains('\r'))
                MultiLine = value;
            else
                Value = value;
        }

        [XmlAttribute("name")] public string Name { get; set; }
        [XmlAttribute("value")] public string Value { get; set; }
        [XmlText] public string MultiLine { get; set; }
    }
}