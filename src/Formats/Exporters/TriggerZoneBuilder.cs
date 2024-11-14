using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UAlbion.Api;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Exporters.Tiled;

namespace UAlbion.Formats.Exporters;

public static class TriggerZoneBuilder
{
    public static IList<(ZoneKey, Geometry.Polygon)> BuildZones(BaseMapData map)
    {
        ArgumentNullException.ThrowIfNull(map);

        // Render zones to a grid
        var zoneMap = new MapEventZone[map.Width * map.Height];
        Array.Copy(map.Zones, zoneMap, zoneMap.Length);

        var regions = new List<(ZoneKey key, IList<(int, int)> region)>();
        for (int index = 0; index < zoneMap.Length; index++)
        {
            var current = zoneMap[index];
            if (current == null)
                continue;

            FillZone(regions, zoneMap, map.Width, current, index);
        }

        RemoveVoids(regions);

        var edgeSets = regions.Select(x => (x.key, FindRegionEdges(x.region))).ToList();

        // zoneMap should be empty now
        ApiUtil.Assert(zoneMap.All(x => x == null));

        // Stitch edges together
        for (int i = 0; i < edgeSets.Count; i++)
            edgeSets[i] = (edgeSets[i].Item1, MergeEdges(edgeSets[i].Item2));

        // Follow edges clockwise to build poly
        return edgeSets
            .SelectMany(x =>
                BuildPolygonsFromSortedEdges(x.Item2)
                    .Select(y => (x.Item1, y)))
            .ToList();
    }

    static void FillGrid(int[] grid, int width, int index, int color)
    {
        Fill(width, index, grid.Length, n =>
        {
            if (grid[n] != 0)
                return false;
            grid[n] = color;
            return true;
        });
    }

    static void FillBorders(int[] grid, int width, int color)
    {
        int height = grid.Length / width;
        if (grid.Length != width * height)
            throw new ArgumentException($"Expected grid to be a rectangular array of width {width}, but it has a length that is not an even multiple ({grid.Length})", nameof(grid));

        int lastRow = (height - 1) * width;
        for (int i = 0; i < width; i++)
        {
            FillGrid(grid, width, i, color); // Fill top
            FillGrid(grid, width, i + lastRow, color); // Fill bottom
        }

        for (int j = 0; j < height; j++)
        {
            FillGrid(grid, width, j * width, color);
            FillGrid(grid, width, (j+1) * width - 1, color);
        }
    }

    public static string PrintRegion(IList<(int x, int y)> region)
    {
        ArgumentNullException.ThrowIfNull(region);

        var extents = GetExtents(region);
        var width = 1 + extents.x1 - extents.x0;
        var height = 1 + extents.y1 - extents.y0;
        var grid = new int[width * height];

        foreach (var (x, y) in region)
        {
            int index = (y - extents.y0) * width + (x - extents.x0);
            grid[index] = 1;
        }

        return PrintGrid(grid, width);
    }

    static string PrintGrid(int[] grid, int width)
    {
        var sb = new StringBuilder();
        for (int i = 0, column = 0; i < grid.Length; i++, column++)
        {
            if (column == width)
            {
                sb.AppendLine();
                column = 0;
            }

            if (grid[i] == 0) sb.Append(' ');
            else sb.Append(grid[i]);
        }

        return sb.ToString();
    }

    public static void RemoveVoids(List<(ZoneKey key, IList<(int x, int y)> points)> regions)
    {
        ArgumentNullException.ThrowIfNull(regions);

        for (int regionIndex = 0; regionIndex < regions.Count; regionIndex++)
        {
            var region = regions[regionIndex];
            if (region.points.Count < 4) // Impossible to enclose an internal tile with fewer than 4
                continue;

            var extents = GetExtents(region.points);
            var width = 1 + extents.x1 - extents.x0;
            var height = 1 + extents.y1 - extents.y0;
            var grid = new int[width * height];

            // Populate occupied area with 1, then fill any border edges with 2. Any remaining 0's must be due to an internal void.
            // Iteratively split the region vertically along the left-most X-coordinate of the internal void until no voids remain.
            foreach (var (x, y) in region.points)
            {
                int index = (y - extents.y0) * width + (x - extents.x0);
                grid[index] = 1;
            }

            FillBorders(grid, width, 2);

            for (int gridIndex = 0; gridIndex < grid.Length; gridIndex++)
            {
                if (grid[gridIndex] != 0)
                    continue;

                int cutoff = gridIndex / width + extents.y0;
                if (cutoff == 0)
                    continue;

                // Void detected! Split across the y-coord
                var topRegion = new List<(int x, int y)>();
                var bottomRegion = new List<(int x, int y)>();
                foreach (var (x, y) in region.points)
                {
                    if (y <= cutoff) topRegion.Add((x,y));
                    else bottomRegion.Add((x,y));
                }

                regions[regionIndex] = (region.key, topRegion);
                regions.Add((region.key, bottomRegion));
                break;
            }
        }
    }

    public static IList<Edge> FindRegionEdges(IList<(int, int)> region)
    {
        ArgumentNullException.ThrowIfNull(region);
        var w = region.Max(x => x.Item1) + 1;
        var h = region.Max(x => x.Item2) + 1;
        var grid = new bool[w * h];
        foreach (var (i, j) in region)
            grid[j * w + i] = true;

        var edges = new List<Edge>();
        for (ushort j = 0; j < h; j++)
        {
            for (ushort i = 0; i < w; i++)
            {
                int n = j * w + i;
                if (!grid[n])
                    continue;

                if (i > 0) // Check left
                {
                    if (!grid[n - 1])
                        edges.Add(new Edge(i, j, i, (ushort)(j + 1)));
                }
                else edges.Add(new Edge(i, j, i, (ushort)(j + 1)));

                if (i < w - 1) // Check right
                {
                    if (!grid[n + 1])
                        edges.Add(new Edge((ushort)(i + 1), j, (ushort)(i + 1), (ushort)(j + 1)));
                }
                else edges.Add(new Edge((ushort)(i + 1), j, (ushort)(i + 1), (ushort)(j + 1)));

                if (j > 0) // Check above
                {
                    if (!grid[n - w])
                        edges.Add(new Edge(i, j, (ushort)(i + 1), j));
                }
                else edges.Add(new Edge(i, j, (ushort)(i + 1), j));

                if (j < h - 1) // Check below
                {
                    if (!grid[n + w])
                        edges.Add(new Edge(i, (ushort)(j + 1), (ushort)(i + 1), (ushort)(j + 1)));
                }
                else edges.Add(new Edge(i, (ushort)(j + 1), (ushort)(i + 1), (ushort)(j + 1)));
            }
        }

        return edges;
    }

    public static IList<Edge> MergeEdges(IList<Edge> edges)
    {
        edges = edges.OrderBy(x => x.ColumnMajorPacked).ToList();
        for (int i = 1; i < edges.Count;)
        {
            var cur = edges[i];
            var last = edges[i - 1];
            if (cur.X1 == last.X2 && 
                cur.Y1 == last.Y2 && 
                cur.IsVertical && 
                last.IsVertical)
            {
                edges[i - 1] = new Edge(last.X1, last.Y1, cur.X2, cur.Y2);
                edges.RemoveAt(i);
            }
            else i++;
        }

        edges = edges.OrderBy(x => x).ToList();
        for (int i = 1; i < edges.Count;)
        {
            var cur = edges[i];
            var last = edges[i - 1];
            if (cur.X1 == last.X2 && 
                cur.Y1 == last.Y2 && 
                cur.IsHorizontal && 
                last.IsHorizontal)
            {
                edges[i - 1] = new Edge(last.X1, last.Y1, cur.X2, cur.Y2);
                edges.RemoveAt(i);
            }
            else i++;
        }

        return edges;
    }

    static void AddAdjacency(EdgeLookup adjacency, ushort x, ushort y, int index)
    {
        if (!adjacency.TryGetValue((x, y), out var indices))
        {
            indices = [];
            adjacency[(x, y)] = indices;
        }
        indices.Add(index);
    }

    sealed class EdgeLookup : Dictionary<(ushort, ushort), List<int>> { }
    public static IList<Geometry.Polygon> BuildPolygonsFromSortedEdges(IList<Edge> edges)
    {
        ArgumentNullException.ThrowIfNull(edges);

        var adjacency = new EdgeLookup(); // Build lookups
        for (int i = 0; i < edges.Count; i++)
        {
            AddAdjacency(adjacency, edges[i].X1, edges[i].Y1, i);
            AddAdjacency(adjacency, edges[i].X2, edges[i].Y2, i);
        }

        var visited = new bool[edges.Count];
        var polygons = new List<Geometry.Polygon>();

        for (int i = 0; i < edges.Count; i++)
        {
            if (visited[i])
                continue;

            polygons.Add(BuildPolygon(edges, visited, adjacency, i));
        }

        return polygons;
    }

    static Geometry.Polygon BuildPolygon(IList<Edge> edges, IList<bool> visited, EdgeLookup adjacency, int initialEdge)
    {
        int edgeIndex = initialEdge;
        var endpoint = (edges[initialEdge].X1, edges[initialEdge].Y1);
        var p2 = endpoint;
        var cur = new Geometry.Polygon { Points = [] };
        for (;;)
        {
            var (x1, y1, x2, y2) = edges[edgeIndex].Tuple;
            bool forward = (x2, y2) == p2;
            var p1 = forward ? (x2, y2) : (x1, y1);
            p2 = forward ? (x1, y1) : (x2, y2);
            cur.Points.Add(p1);

            visited[edgeIndex] = true;
            if (p2 == endpoint)
                break;

            adjacency.TryGetValue(p2, out var destinations);
            if (destinations == null)
                break;

            edgeIndex = -1;
            foreach (var index in destinations)
            {
                if (!visited[index])
                {
                    edgeIndex = index;
                    break;
                }
            }

            if (edgeIndex == -1)
                break;
        }

        cur.CalculateOffset();
        return cur;
    }

    static void FillZone(List<(ZoneKey, IList<(int, int)>)> regions, MapEventZone[] zoneMap, int width, MapEventZone zone, int index)
    {
        var key = new ZoneKey(zone);
        var region = new List<(int, int)>();
        regions.Add((key, region));

        Fill(width, index, zoneMap.Length, n =>
        {
            var other = zoneMap[n];
            if (other == null)
                return false;

            if (new ZoneKey(other) != key)
                return false;

            byte i = (byte)(n % width);
            byte j = (byte)(n / width);
            region.Add((i, j));
            zoneMap[n] = null;

            return true;
        });
    }

    static void Fill(int width, int startIndex, int arraySize, Func<int, bool> matchFunc)
    {
        Queue<int> pending = new Queue<int>();
        pending.Enqueue(startIndex);
        while (pending.Count > 0)
        {
            var n = pending.Dequeue();

            bool isMatch = matchFunc(n);
            if (!isMatch)
                continue;

            byte i = (byte)(n % width);
            if (i > 0)                 pending.Enqueue(n - 1); // Check left
            if (i < width - 1)         pending.Enqueue(n + 1); // Check right
            if (n - width >= 0)        pending.Enqueue(n - width); // Check above
            if (n + width < arraySize) pending.Enqueue(n + width); // Check below
        }
    }

    static (int x0, int y0, int x1, int y1) GetExtents(IEnumerable<(int x, int y)> points)
    {
        (int x, int y) min = (int.MaxValue, int.MaxValue);
        (int x, int y) max = (int.MinValue, int.MinValue);
        foreach (var point in points)
        {
            if (point.x < min.x) min.x = point.x;
            if (point.y < min.y) min.y = point.y;
            if (point.x > max.x) max.x = point.x;
            if (point.y > max.y) max.y = point.y;
        }
        return (min.x, min.y, max.x, max.y);
    }

    static (int x0, int y0, int x1, int y1) GetExtents(IEnumerable<((int x, int y) from, (int x, int y) to)> shape) // Assume edges are already sorted
    {
        (int x, int y) min = (int.MaxValue, int.MaxValue);
        (int x, int y) max = (int.MinValue, int.MinValue);
        foreach (var (from, to) in shape)
        {
            if (from.x < min.x) min.x = from.x;
            if (from.y < min.y) min.y = from.y;

            if (to.x > max.x) max.x = to.x;
            if (to.y > max.y) max.y = to.y;
        }
        return (min.x, min.y, max.x, max.y);
    }

    static ((int x, int y) from, (int x, int y) to)[] SortEdges(IEnumerable<((int x, int y) from, (int x, int y) to)> shape)
    {
        return shape.Select(edge =>
        {
            if (edge.to.x < edge.from.x) return (edge.to, edge.from);
            if (edge.to.x == edge.from.x && edge.to.y < edge.from.y) return (edge.to, edge.from);
            return (edge.from, edge.to);
        }).OrderBy(x => x).ToArray();
    }

    static float Lerp(float t, float a, float b) => a + (b - a) * t;

    static bool TestEdge(int i, int j, ((int x, int y) from, (int x, int y) to) edge)
    {
        if (edge.from.x == edge.to.x) return false;
        if (edge.from.x > i) return false;
        if (edge.to.x <= i) return false;
        if (edge.to.y == edge.from.y) return edge.to.y <= j;

        float t = (float)(i - edge.from.x) / (edge.to.x - edge.from.x);
        float yIntercept = Lerp(t, edge.from.y, edge.to.y);
        return yIntercept <= j;
    }

    static bool TestPoint(int i, int j, IEnumerable<((int x, int y) from, (int x, int y) to)> shape)
    {
        int intersectionsAbove = 0;

        foreach (var edge in shape)
            if (TestEdge(i, j, edge))
                intersectionsAbove++;

        return (intersectionsAbove & 1) == 1;
    }

    public static IList<(int x, int y)> GetPointsInsidePolygon(IEnumerable<(int, int)> polygon)
    {
        ArgumentNullException.ThrowIfNull(polygon);
        var shape = PolygonToShape(polygon);
        return GetPointsInsideShape(shape);
    }

    static IEnumerable<((int x, int y) from, (int x, int y) to)> PolygonToShape(IEnumerable<(int x, int y)> polygon)
    {
        bool first = true;
        (int x, int y) firstPoint = (0, 0);
        (int x, int y) lastPoint = (0, 0);

        foreach (var point in polygon)
        {
            if (first)
                firstPoint = point;
            else
                yield return (lastPoint, point);

            lastPoint = point;
            first = false;
        }

        yield return (lastPoint, firstPoint);
    }

    static List<(int x, int y)> GetPointsInsideShape(IEnumerable<((int x, int y) from, (int x, int y) to)> shape)
    {
        var results = new List<(int x, int y)>();
        shape = SortEdges(shape);
        var rect = GetExtents(shape);
        for (int j = rect.y0; j < rect.y1; j++)
        for (int i = rect.x0; i < rect.x1; i++)
            if (TestPoint(i, j, shape))
                results.Add((i, j));
        return results;
    }
}