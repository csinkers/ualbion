using System;
using System.Collections.Generic;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Formats.Exporters.Tiled;

public static class NpcPathBuilder
{
    public const string NextNodePropertyName = "Next";
    public const string TimePropertyName = "Time";
    public static IEnumerable<(int x, int y)> FollowRuns(IEnumerable<(int time, int x, int y)> runs)
    {
        yield break; // TODO
    }

    public static IEnumerable<(int time, int x, int y)> CombineRuns<T>(T[] points, Func<T, (int x, int y)> getter)
    {
        var waypoints = IdentifyWaypoints(points, getter);
        int last = -1;
        foreach (var i in waypoints)
        {
            var (x, y) = getter(points[i]);
            if (last != -1)
            {
                var (lastX, lastY) = getter(points[last]);
                if (lastX == x && lastY == y)
                    continue;
            }

            yield return (i, x, y);
            last = i;
        }
    }

    public static IEnumerable<int> IdentifyWaypoints<T>(T[] points, Func<T, (int x, int y)> getter)
    {
        if (getter == null) throw new ArgumentNullException(nameof(getter));
        if (points == null || points.Length == 0) yield break;
        yield return 0;

        for (int i = 1; i < points.Length - 1; i++)
        {
            var lastPoint = getter(points[i - 1]);
            var point     = getter(points[i]);
            var nextPoint = getter(points[i + 1]);

            var dl = (point.x - lastPoint.x, point.y - lastPoint.y);
            var dn = (nextPoint.x - point.x, nextPoint.y - point.y);

            if (dl != dn)
                yield return i;
        }

        if (points.Length > 1)
            yield return points.Length - 1;
    }

    public static List<MapObject> Build(
        MapNpc npc,
        int tileWidth,
        int tileHeight,
        ref int nextId)
    {
        if (npc == null) throw new ArgumentNullException(nameof(npc));
        var results = new List<MapObject>();
        var combined = CombineRuns(npc.Waypoints, x => (x.X, x.Y));

        foreach (var (index, posX, posY) in combined)
        {
            int id = nextId++;
            if (results.Count > 0)
                results[^1].Properties.Add(TiledProperty.Object(NextNodePropertyName, id));

            results.Add(new MapObject
            {
                Id = id,
                Type = "Path",
                X = posX * tileWidth,
                Y = posY * tileHeight,
                Properties = new List<TiledProperty> { new(TimePropertyName, MapNpc.WaypointIndexToTime(index)) },
                Point = TiledPoint.Instance
            });
        }

        return results;
    }

    public static Func<int, NpcWaypoint[]> BuildWaypointLookup(Map map)
    {
        var lookup = new WaypointLookup(map);
        return lookup.GetWaypoints;
    }
}