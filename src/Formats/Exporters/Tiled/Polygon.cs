using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace UAlbion.Formats.Exporters.Tiled
{
    public class Polygon
    {
        public Polygon() { }

        public Polygon(IList<(int, int)> points, int multX = 1, int multY = 1)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));
            var sb = new StringBuilder();
            foreach (var (x, y) in points)
                sb.Append($"{x * multX},{y * multY} ");
            Points = sb.ToString(0, sb.Length - 1);
        }
        [XmlAttribute("points")] public string Points { get; set; }
    }
}