using System;
using static System.FormattableString;

namespace UAlbion.Formats;

public class CursorHotspot
{
    public float X { get; set; }
    public float Y { get; set; }

    public static CursorHotspot Parse(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        var parts = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 2) throw new FormatException("Expected hotspot location to consist of two ints/floats separated by a space, but was \"{s}\"");
        if (!float.TryParse(parts[0], out var x)) throw new FormatException("Expected hotspot location to consist of two ints/floats separated by a space, but was \"{s}\"");
        if (!float.TryParse(parts[0], out var y)) throw new FormatException("Expected hotspot location to consist of two ints/floats separated by a space, but was \"{s}\"");

        return new CursorHotspot { X = x, Y = y };
    }

    public override string ToString() => Invariant($"{X} {Y}");
}