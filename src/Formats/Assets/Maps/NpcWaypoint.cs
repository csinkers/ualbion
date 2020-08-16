namespace UAlbion.Formats.Assets.Maps
{
    public struct NpcWaypoint : System.IEquatable<NpcWaypoint>
    {
        public NpcWaypoint(byte x, byte y)
        {
            X = x;
            Y = y;
        }

        public byte X { get; }
        public byte Y { get; }
        public override string ToString() => $"({X}, {Y})";
        public override bool Equals(object obj) => obj is NpcWaypoint other && Equals(other);
        public override int GetHashCode() => (X << 8) | Y; 
        public static bool operator ==(NpcWaypoint left, NpcWaypoint right) => left.Equals(right);
        public static bool operator !=(NpcWaypoint left, NpcWaypoint right) => !(left == right);
        public bool Equals(NpcWaypoint other) => X == other.X && Y == other.Y;

    }
}