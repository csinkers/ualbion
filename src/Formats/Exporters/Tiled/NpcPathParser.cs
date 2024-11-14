using System;
using System.Collections.Generic;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Formats.Exporters.Tiled;

public class NpcPathParser // Functionality needs to line up with NpcPathBuilder.
{
    record struct Waypoint(int Id, int X, int Y, int Time, int? Next);
    readonly Dictionary<int, Waypoint> _waypoints = [];
    public NpcPathParser(IEnumerable<MapObject> objects, int tileWidth, int tileHeight)
    {
        ArgumentNullException.ThrowIfNull(objects);
        foreach (var obj in objects)
            if ("Path".Equals(obj.Type, StringComparison.OrdinalIgnoreCase))
                _waypoints[obj.Id] = ParseWaypoint(obj, tileWidth, tileHeight);
    }

    static Waypoint ParseWaypoint(MapObject obj, int tileWidth, int tileHeight) =>
        new(
            obj.Id,
            (int)(obj.X / tileWidth),
            (int)(obj.Y / tileHeight),
            obj.PropInt(NpcPathBuilder.TimePropertyName) ?? 0,
            obj.PropInt(NpcPathBuilder.NextNodePropertyName)
        );

    public NpcWaypoint[] GetWaypoints(int startingId, int count)
    {
        var result = new NpcWaypoint[count];
        for (int i = 0; i < result.Length; i++)
            result[i] = new NpcWaypoint(255, 255);

        void Fill(in Waypoint a, in Waypoint b)
        {
            var indexA = MapNpc.TimeToWaypointIndex(a.Time);
            var indexB = MapNpc.TimeToWaypointIndex(b.Time);

            if (indexB <= indexA)
                throw new FormatException($"Path time {b.Time} on node {b.Id} was less than or equal to the previous ({a.Time} on node {a.Id})");

            result[indexA] = new NpcWaypoint((byte)a.X, (byte)a.Y);
            var (x, y) = (b.X, b.Y);
            var (dx, dy) = (Math.Sign(a.X - b.X), Math.Sign(a.Y - b.Y));
            for (int i = indexB - 1; i > indexA; i--) // Fill backwards
            {
                if (x == a.X) dx = 0;
                if (y == a.Y) dy = 0;
                x += dx;
                y += dy;
                result[i] = new NpcWaypoint((byte)x, (byte)y);
            }
        }

        var a = _waypoints[startingId];
        while (a.Next.HasValue)
        {
            var b = _waypoints[a.Next.Value];
            Fill(a, b);
            a = b;
        }

        Fill(a, new Waypoint(-1, a.X, a.Y, MapNpc.WaypointIndexToTime(result.Length), null));
        return result;
    }
}