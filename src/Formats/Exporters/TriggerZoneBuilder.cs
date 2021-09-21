using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Api;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Exporters.Tiled;

namespace UAlbion.Formats.Exporters
{
    public static class TriggerZoneBuilder
    {
        public static IList<(ZoneKey, Geometry.Polygon)> BuildZonesSimple(BaseMapData map)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));
            var zones = new List<(ZoneKey, Geometry.Polygon)>();
            foreach (var zone in map.Zones)
            {
                zones.Add((new ZoneKey(zone), new Geometry.Polygon { Points = new[]
                    {
                        (zone.X, zone.Y),
                        (zone.X+1, zone.Y),
                        (zone.X+1, zone.Y+1),
                        (zone.X, zone.Y+1),
                    }
                }));
            }
            return zones;
        }

        public static IList<(ZoneKey, Geometry.Polygon)> BuildZones(BaseMapData map)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));

            // Render zones to a grid
            var zoneMap = new List<MapEventZone>[map.Width * map.Height];
            foreach (var zone in map.Zones)
            {
                int index = zone.Y * map.Width + zone.X;
                zoneMap[index] ??= new List<MapEventZone>();
                zoneMap[index].Add(zone);
            }

            var regions = new List<(ZoneKey, IList<(int, int)>)>();
            for (int index = 0; index < zoneMap.Length; index++)
            {
                var current = zoneMap[index];
                if (current == null)
                    continue;

                while (current.Count > 0)
                    FillZone(regions, zoneMap, map.Width, current[0], index);
            }

            RemoveVoids(regions);

            var edgeSets = regions.Select(x => (x.Item1, FindRegionEdges(x.Item2))).ToList();

            // zoneMap should be empty now
            ApiUtil.Assert(zoneMap.All(x => x?.Any() != true));

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

        static void RemoveVoids(List<(ZoneKey key, IList<(int x, int y)> points)> regions)
        {
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

                    int cutoff = gridIndex % width + extents.x0;

                    // Void detected! Split across the x-coord
                    var leftRegion = new List<(int x, int y)>();
                    var rightRegion = new List<(int x, int y)>();
                    foreach (var (x, y) in region.points)
                    {
                        if (x <= cutoff) leftRegion.Add((x,y));
                        else rightRegion.Add((x,y));
                    }

                    regions[regionIndex] = (region.key, leftRegion); // Left region is guaranteed to contain no voids
                    regions.Add((region.key, rightRegion));
                    break;
                }
            }
        }

        public static IList<Edge> FindRegionEdges(IList<(int, int)> region)
        {
            if (region == null) throw new ArgumentNullException(nameof(region));
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
                indices = new List<int>();
                adjacency[(x, y)] = indices;
            }
            indices.Add(index);
        }

        class EdgeLookup : Dictionary<(ushort, ushort), List<int>> { }
        public static IList<Geometry.Polygon> BuildPolygonsFromSortedEdges(IList<Edge> edges)
        {
            if (edges == null) throw new ArgumentNullException(nameof(edges));

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
            var cur = new Geometry.Polygon { Points = new List<(int, int)>() };
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

        /*
        static void FillZone(List<(ZoneKey, IList<(int, int)>)> regions, List<MapEventZone>[] zoneMap, int width, MapEventZone zone, int index)
        {
            Queue<int> pending = new Queue<int>();
            pending.Enqueue(index);
            var region = new List<(int, int)>();
            var key = new ZoneKey(zone);
            regions.Add((key, region));

            while (pending.Count > 0)
            {
                var n = pending.Dequeue();
                var targetZones = zoneMap[n];
                if (targetZones == null)
                    continue;

                byte i = (byte)(n % width);
                byte j = (byte)(n / width);
                for (int k = 0; k < targetZones.Count;)
                {
                    var other = targetZones[k];
                    if (new ZoneKey(other) != key)
                    {
                        k++;
                        continue;
                    }

                    if (i > 0)                      pending.Enqueue(n - 1); // Check left
                    if (i < width - 1)              pending.Enqueue(n + 1); // Check right
                    if (n - width >= 0)             pending.Enqueue(n - width); // Check above
                    if (n + width < zoneMap.Length) pending.Enqueue(n + width); // Check below
                    region.Add((i, j));
                    targetZones.RemoveAt(k);
                }
            }
        }
        */
        static void FillZone(List<(ZoneKey, IList<(int, int)>)> regions, List<MapEventZone>[] zoneMap, int width, MapEventZone zone, int index)
        {
            var key = new ZoneKey(zone);
            var region = new List<(int, int)>();
            regions.Add((key, region));

            Fill(width, index, zoneMap.Length, n =>
            {
                var targetZones = zoneMap[n];
                if (targetZones == null || targetZones.Count == 0)
                    return false;

                bool hit = false;
                for (int k = 0; k < targetZones.Count;)
                {
                    var other = targetZones[k];
                    if (new ZoneKey(other) != key)
                    {
                        k++;
                        continue;
                    }

                    hit = true;
                    byte i = (byte)(n % width);
                    byte j = (byte)(n / width);
                    region.Add((i, j));
                    targetZones.RemoveAt(k);
                }

                return hit;
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

        public static IList<(int x, int y)> GetPointsInsideShape(IEnumerable<((int x, int y) from, (int x, int y) to)> shape)
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
}
