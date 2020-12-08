using System.Collections.Generic;
using System.Linq;

#pragma warning disable CA2227 // Collection properties should be read only
namespace UAlbion.Formats.Geometry
{
    public class Polygon
    {
        public int OffsetX { get; set; }
        public int OffsetY { get; set; }
        public IList<(int, int)> Points { get; set; }

        public void CalculateOffset()
        {
            OffsetX += Points.Min(x => x.Item1);
            OffsetY += Points.Min(x => x.Item2);
            for (int i = 0; i < Points.Count; i++)
                Points[i] = (Points[i].Item1 - OffsetX, Points[i].Item2 - OffsetY);
        }

        public override string ToString()
            => $"({OffsetX},{OffsetY}:[{string.Join(" ", Points.Select(x => $"{x.Item1},{x.Item2}"))}]";
    }
}
#pragma warning restore CA2227 // Collection properties should be read only