using System;
using System.Collections.Generic;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Formats.Exporters.Tiled;

public class WaypointLookup // Functionality needs to line up with NpcPathBuilder. TODO: Remove round-tripping discrepancies
{
    readonly Dictionary<int, (int x, int y, int time, int? next)> _waypoints = new();
    public WaypointLookup(Map map)
    {
        foreach (var objectGroup in map.ObjectGroups)
        foreach (var obj in objectGroup.Objects)
            if ("Path".Equals(obj.Type, StringComparison.OrdinalIgnoreCase))
                _waypoints[obj.Id] = ParseWaypoint(obj, map);
    }

    static (int x, int y, int time, int? next) ParseWaypoint(MapObject obj, Map map) =>
    (
        (int)(obj.X / map.TileWidth),
        (int)(obj.Y / map.TileHeight),
        obj.PropInt(NpcPathBuilder.TimePropertyName) ?? 0,
        obj.PropInt(NpcPathBuilder.NextNodePropertyName)
    );

    public NpcWaypoint[] GetWaypoints(int startingId)
    {
        var result = new NpcWaypoint[MapNpc.WaypointCount];

        int? node = startingId;
        var (index, x, y, lastX, lastY, lastId) = (0, 0, 0, 0, 0, node.Value);
        while (node.HasValue && _waypoints.TryGetValue(node.Value, out var info))
        {
            var (newX, newY, newTime, next) = info;
            var newIndex = MapNpc.TimeToWaypointIndex(newTime);

            if (newIndex < index)
                throw new FormatException($"Path time {newTime} on node {node} was less than the previous ({MapNpc.WaypointIndexToTime(index)} on node {lastId})");

            if (newX == 255 || newY == 255) (x, y) = (newX, newY);
            if (lastX == 255 || lastY == 255) (x, y) = (newX, newY);

            var dist = Math.Max(Math.Abs(newX - lastX), Math.Abs(newY - lastY));
            while (index <= newIndex - dist)
                result[index++] = new NpcWaypoint((byte)x, (byte)y);

            while (index < newIndex)
            {
                if (newX > x) x++;
                if (newX < x) x--;
                if (newY > y) y++;
                if (newY < y) y--;

                result[index++] = new NpcWaypoint((byte)x, (byte)y);
            }

            (lastId, x, y, lastX, lastY) = (node.Value, newX, newY, newX, newY);
            node = next;
        }

        while (index < result.Length)
            result[index++] = new NpcWaypoint((byte)x, (byte)y);

        return result;
    }
}