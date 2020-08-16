using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace UAlbion.Core.Visual
{
    public struct DungeonTile : IEquatable<DungeonTile>
    {
        public static readonly uint StructSize = (uint)Unsafe.SizeOf<DungeonTile>();
        public Vector2 TilePosition { get; set; }
        public byte Floor { get; set; } // 0 = No floor
        public byte Ceiling { get; set; } // 0 = No Ceiling
        public byte Wall { get; set; } // 0 = No Wall
        public DungeonTileFlags Flags { get; set; }
        public Vector2 WallSize { get; set; }

        public override string ToString() => $"({TilePosition.X}, {TilePosition.Y}): {Floor}.{Ceiling}.{Wall} ({Flags})";

        public override bool Equals(object obj) => obj is DungeonTile other && Equals(other);

        public bool Equals(DungeonTile other) =>
            TilePosition == other.TilePosition &&
            Floor == other.Floor &&
            Ceiling == other.Ceiling && 
            Wall == other.Wall &&
            Flags == other.Flags && 
            WallSize == other.WallSize;

        public override int GetHashCode() => HashCode.Combine(TilePosition, Floor, Ceiling, WallSize, Flags, WallSize);
        public static bool operator ==(DungeonTile left, DungeonTile right) => left.Equals(right);
        public static bool operator !=(DungeonTile left, DungeonTile right) => !(left == right);
    }
}