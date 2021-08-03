using System;
using System.Globalization;

namespace UAlbion.Formats.Assets.Maps
{
    public struct NpcWaypoint : IEquatable<NpcWaypoint>
    {
        public NpcWaypoint(byte x, byte y)
        {
            X = x;
            Y = y;
        }

        public byte X { get; }
        public byte Y { get; }
        public override string ToString() => $"({X}, {Y})";

        public static NpcWaypoint Parse(string s)
        {
            if (s == null)
                throw new FormatException("NpcWaypoint was null");
            var parts = s.Trim('(', ')').Split(',');
            return new NpcWaypoint(
                byte.Parse(parts[0], CultureInfo.InvariantCulture),
                byte.Parse(parts[1], CultureInfo.InvariantCulture));
        }

        public override bool Equals(object obj) => obj is NpcWaypoint other && Equals(other);
        public override int GetHashCode() => (X << 8) | Y; 
        public static bool operator ==(NpcWaypoint left, NpcWaypoint right) => left.Equals(right);
        public static bool operator !=(NpcWaypoint left, NpcWaypoint right) => !(left == right);
        public bool Equals(NpcWaypoint other) => X == other.X && Y == other.Y;

    }
}
