using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Formats.Exporters.Tiled;

public static class NpcPathBuilder
{
    public const string NextNodePropertyName = "Next";
    public const string TimePropertyName = "Time";
    public static IEnumerable<(int index, int x, int y)> CombineRuns(NpcWaypoint[] points)
    {
        ArgumentNullException.ThrowIfNull(points);

        var waypoints = IdentifyWaypoints(points).ToList();
        int last = -1;
        foreach (var (i, mustEmit) in waypoints)
        {
            var a = points[i];
            if (!mustEmit && last != -1)
            {
                var b = points[last];
                if (a == b)
                    continue;
            }

            yield return (i, a.X, a.Y);
            last = i;
        }
    }

    public static IEnumerable<(int index, bool mustEmit)> IdentifyWaypoints(NpcWaypoint[] points)
    {
        if (points == null || points.Length == 0) yield break;
        yield return (0, true);

        for (int i = 1; i < points.Length - 1; i++)
        {
            var a = points[i - 1];
            var b = points[i];
            var c = points[i + 1];

            var ab = (b.X - a.X, b.Y - a.Y);
            var bc = (c.X - b.X, c.Y - b.Y);

            bool discontinuous =
                Math.Abs(ab.Item1) > 1 ||
                Math.Abs(ab.Item2) > 1 ||
                Math.Abs(bc.Item1) > 1 ||
                Math.Abs(bc.Item2) > 1;

            if (ab != bc || discontinuous)
                yield return (i, discontinuous);
        }

        if (points.Length > 1)
            yield return (points.Length - 1, true);
    }

    public static List<MapObject> Build(
        int npcNumber,
        NpcWaypoint[] waypoints,
        int tileWidth,
        int tileHeight,
        ref int nextId)
    {
        ArgumentNullException.ThrowIfNull(waypoints);
        var results = new List<MapObject>();
        var combined = CombineRuns(waypoints);

        foreach (var (index, posX, posY) in combined)
        {
            int id = nextId++;
            if (results.Count > 0)
                results[^1].Properties.Add(TiledProperty.Object(NextNodePropertyName, id));

            var time = MapNpc.WaypointIndexToTime(index);
            results.Add(new MapObject
            {
                Id = id,
                Type = "Path",
                Name = $"N{npcNumber} T{time}",
                X = posX * tileWidth,
                Y = posY * tileHeight,
                Properties = [new(TimePropertyName, time)],
                Point = TiledPoint.Instance
            });
        }

        return results;
    }

    public static NpcPathParser BuildParser(IEnumerable<MapObject> objects, int tileWidth, int tileHeight) => new(objects, tileWidth, tileHeight);
}