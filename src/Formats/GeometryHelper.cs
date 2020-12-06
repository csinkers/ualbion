using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace UAlbion.Formats
{
    public readonly struct Triangle : IEquatable<Triangle>
    {
        public Triangle(Vector2 a, Vector2 b, Vector2 c) { A = a; B = b; C = c; }
        public Vector2 A { get; }
        public Vector2 B { get; }
        public Vector2 C { get; }

        public static bool operator ==(Triangle a, Triangle b) => a.Equals(b);
        public static bool operator !=(Triangle a, Triangle b) => !a.Equals(b);
        public bool Equals(Triangle other) => A.Equals(other.A) && B.Equals(other.B) && C.Equals(other.C);
        public override bool Equals(object obj) => obj is Triangle other && Equals(other);
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = A.GetHashCode();
                hashCode = (hashCode * 397) ^ B.GetHashCode();
                hashCode = (hashCode * 397) ^ C.GetHashCode();
                return hashCode;
            }
        }
    }

    public class Polygon
    {
        public int OffsetX { get; set; }
        public int OffsetY { get; set; }
        public IList<(int, int)> Points { get; set; }
    }

    public static class GeometryHelper
    {
        public static IList<Polygon> RegionToPolygons(IList<(int, int)> tiles)
        {
            if (tiles == null) throw new ArgumentNullException(nameof(tiles));
            int offsetX = tiles.Min(x => x.Item1);
            int offsetY = tiles.Min(x => x.Item2);
            int width = tiles.Max(x => x.Item1) + 1 - offsetX;
            int height = tiles.Max(x => x.Item2) + 1 - offsetY;
            var grid = Render(tiles, width, height, offsetX, offsetY);
            return new List<Polygon>();
        }

        static bool[] Render(IList<(int, int)> tiles, int width, int height, int offsetX, int offsetY)
        {
            var grid = new bool[width * height];
            foreach (var (x, y) in tiles)
                grid[x + y * width] = true;
            return grid;
        }

        #if false
        static int Cantor(int x, int y) => (x + y) / 2 * (x + y + 1) + y;
        static (int, int) Decantor(int z)
        {
            var w = (int)Math.Floor((Math.Sqrt(8 * z + 1) - 1) / 2);
            var t = (w * w + w) / 2;
            return (w - (z - t), z - t);
        }

        static (int, int) FindCentre(IList<(int, int)> poly)
        {
            double cx = 0;
            double cy = 0;
            foreach (var (x, y) in poly)
            {
                cx += (double)x / poly.Count;
                cy += (double)y / poly.Count;
            }

            return ((int)cx, (int)cy);
        }

        static bool AllNeighbour(bool[] grid, int width, int index)
        {
            int i = index % width;
            if (i <= 0 || !grid[index - 1]) return false;
            if (i >= width || !grid[index + 1]) return false;
            if (index - width < 0 || !grid[index + width]) return false;
            if (index + width >= grid.Length || !grid[index + width]) return false;
            return true;
        }

        public static IList<(int, int, IList<(int, int)>)> RegionToPolygons2(IList<(int, int)> tiles)
        {
            var polyIndices = new HashSet<int>();
            int side = 1;

            var grid = Render(tiles, width, height);
            for (int g = 0; g < grid.Length; g++)
            {
                if (!grid[g])
                    continue;

                var p = Decantor(g);
                var allNeighbour = true;
                for (var dx = -1; dx <= 1; dx++)
                {
                    for (var dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 || dy == 0)
                            continue;

                        if (!grid[Cantor((int)Math.Floor((double)(p.Item1 + dx)), (int)Math.Floor((double)(p.Item2 + dy)))])
                            allNeighbour = false;
                    }
                }

                if (!AllNeighbour(grid, width, g))
                {
                    polyIndices.Add(g);
                    polyIndices.Add(Cantor(p.Item1 + 1, p.Item2));
                    polyIndices.Add(Cantor(p.Item1, p.Item2 + 1));
                    polyIndices.Add(Cantor(p.Item1 + 1, p.Item2 + 1));
                }
            }

            var poly = polyIndices.Select(z =>
            {
                var (x, y) = Decantor(z);
                return (x * side, y * side);
            }).ToList();

            var center = FindCentre(poly);
            //poly = poly.OrderBy((a, b) =>
            //    -((a.Item1 - center.Item1) * (b.Item2 - center.Item2) 
            //      - 
            //      (b.Item1 - center.Item1) * (a.Item2 - center.Item2))).ToList();
            return new List<(int, int, IList<(int, int)>)> { (offsetX, offsetY, poly) };
        }
#endif

        public static void RadixSort(int[] values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));

            int[] temp = new int[values.Length];
            for (int shift = 31; shift > -1; --shift)
            {
                var j = 0;
                for (int i = 0; i < values.Length; ++i)
                {
                    bool move = (values[i] << shift) >= 0;
                    if (shift == 0 ? !move : move)   
                        values[i-j] = values[i];
                    else                             
                        temp[j++] = values[i];
                }

                Array.Copy(temp,
                    0,
                    values,
                    values.Length-j,
                    j);
            }
        }

        public static IList<Triangle> Delauney(IList<Vector2> vertices) // Bowyer-Watson 1981
        {
            vertices = vertices.ToList();
            var triangles = new List<(int, int, int)>();

            Triangle superTriangle = new Triangle(Vector2.Zero, Vector2.Zero, Vector2.Zero);
            int l1 = vertices.Count;
            vertices.Add(superTriangle.A);
            vertices.Add(superTriangle.B);
            vertices.Add(superTriangle.C);
            triangles.Add((l1, l1+1, l1+2));

            foreach (var v in vertices)
            {
                var edges = new List<(int, int)>();
                for (int i = 0; i < triangles.Count;)
                {
                    var (a,b,c) = triangles[i];
                    var (circumCentre, circumRadiusSquared) = Circumcircle(
                        new Triangle(
                            vertices[a],
                            vertices[b],
                            vertices[c]));

                    if ((v - circumCentre).LengthSquared() < circumRadiusSquared)
                    {
                        edges.Add((a, b));
                        edges.Add((b, c));
                        edges.Add((c, b));
                        triangles.RemoveAt(i);
                    }
                    else i++;
                }

                edges = edges // Delete doubly specified edges
                    .GroupBy(x => x)
                    .Where(x => x.Count() == 1)
                    .Select(x => x.Key)
                    .ToList();
            }

            return triangles
                .Select(x => new Triangle(
                    vertices[x.Item1],
                    vertices[x.Item2],
                    vertices[x.Item3]))
                .ToList();
        }

        static (Vector2, Vector2) Extents(IList<Vector2> vertices)
        {
            if(vertices.Count == 0)
                throw new InvalidOperationException($"Cannot calculate the extents of an empty vertex list");

            Vector2 min = vertices[0];
            Vector2 max = vertices[0];

            foreach (var v in vertices)
            {
                if (v.X < min.X) min.X = v.X;
                if (v.Y < min.Y) min.Y = v.Y;
                if (v.X > max.X) max.X = v.X;
                if (v.Y > max.Y) max.Y = v.Y;
            }

            return (min, max);
        }

        static Triangle Supertriangle(IList<Vector2> vertices)
        {
            var (min, max) = Extents(vertices);
            return new Triangle(new Vector2(-1000, -255), new Vector2(0, 1250), new Vector2(1000, -255));
        }

        /// <summary>
        /// Calculates the circumcircle for a given triangle.
        /// </summary>
        /// <param name="triangle">The triangle to calculate the circumcircle of</param>
        /// <returns>The circumcircle centre position and squared radius</returns>
        public static (Vector2, double) Circumcircle(Triangle triangle)
        {
            var b = triangle.B - triangle.A;
            var c = triangle.C - triangle.A;

            var d = 2 * (b.X * c.Y - b.Y * c.X);
            var lenB2 = b.X * b.X + b.Y * b.Y;
            var lenC2 = c.X * c.X + c.Y * c.Y;
            var ux = (c.Y * lenB2 - b.Y * lenC2) / d;
            var uy = (b.X * lenC2 - c.X * lenB2) / d;
            var radius2 = ux * ux + uy * uy;
            var centre = new Vector2(ux, uy) + triangle.A;
            return (centre, radius2);
        }
    }
}
