using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml.Serialization;

namespace UAlbion.Formats.Exporters.Tiled;

public class Polygon
{
    public Polygon() { }

    public Polygon(IList<(int, int)> points, int multX = 1, int multY = 1)
    {
        if (points == null) throw new ArgumentNullException(nameof(points));
        var sb = new StringBuilder();
        foreach (var (x, y) in points)
            sb.Append($"{x * multX},{y * multY} ");
        PointsString = sb.ToString(0, sb.Length - 1);
    }

    [XmlIgnore]
    public IEnumerable<(int x, int y)> Points
    {
        get
        {
            var parts = PointsString.Split(' ');
            foreach (var part in parts)
            {
                var subParts = part.Split(',', StringSplitOptions.RemoveEmptyEntries);
                int x = int.Parse(subParts[0], CultureInfo.InvariantCulture);
                int y = int.Parse(subParts[1], CultureInfo.InvariantCulture);
                yield return (x, y);
            }
        }
    }

    [XmlAttribute("points")] public string PointsString { get; set; }
}