using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using static System.FormattableString;

namespace UAlbion.Formats.Assets.Maps;

class NpcWaypointConverter : JsonConverter<NpcWaypoint[]>
{
    NpcWaypointConverter() {}
    public static readonly NpcWaypointConverter Instance = new();
    public override NpcWaypoint[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException($"Expected token type StartArray when deserializing NpcWaypoint array, but was {reader.TokenType}");

        var list = new List<NpcWaypoint>();
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
                return list.ToArray();

            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException($"Expected String when reading {typeToConvert}, but was {reader.TokenType}");

            var (waypoint, count) = ParseWaypoint(reader.GetString());
            for (int i = 0; i < count; i++)
                list.Add(waypoint);
        }

        throw new JsonException($"Expected ']' when reading {typeToConvert}, but all input has been consumed");
    }

    public override void Write(Utf8JsonWriter writer, NpcWaypoint[] value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        foreach (var (x, y, count) in FindCounts(value))
            writer.WriteStringValue(count == 1
                ? Invariant($"({x}, {y})")
                : Invariant($"({x}, {y}, {count})"));

        writer.WriteEndArray();
    }

    static (NpcWaypoint position, int count) ParseWaypoint(string s)
    {
        if (s == null)
            throw new FormatException("NpcWaypoint string was null");

        var parts = s.Trim('(', ')').Split(',');
        int count = 1;
        if (parts.Length == 3)
            count = int.Parse(parts[2]);

        return (new NpcWaypoint(
            byte.Parse(parts[0]),
            byte.Parse(parts[1])), count);
    }

    static IEnumerable<(int x, int y, int count)> FindCounts(NpcWaypoint[] points)
    {
        if (points.Length == 0)
            yield break;

        int count = 1;
        var (x, y) = (points[0].X, points[0].Y);
        for (int i = 1; i < points.Length; i++)
        {
            if (x == points[i].X && y == points[i].Y)
            {
                count++;
                continue;
            }

            yield return (x, y, count);
            (x, y, count) = (points[i].X, points[i].Y, 1);
        }

        yield return (x, y, count);
    }
}
