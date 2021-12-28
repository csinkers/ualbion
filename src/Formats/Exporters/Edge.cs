using System;

namespace UAlbion.Formats.Exporters;

public readonly struct Edge : IEquatable<Edge>, IComparable<Edge>
{
    public Edge(ushort x1, ushort y1, ushort x2, ushort y2)
    {
        Packed = y1 > y2 || y1 == y2 && x1 > x2 // Ensure first vertex always sorts below second
            ? (ulong)y2 << 48 | (ulong)y1 << 32 | (ulong)x2 << 16 | x1
            : (ulong)y1 << 48 | (ulong)y2 << 32 | (ulong)x1 << 16 | x2;
    }

    public ulong Packed { get; }
    public ulong ColumnMajorPacked => 
        (Packed & 0x00000000_ffffffffUL) << 32 | 
        (Packed & 0xffffffff_00000000UL) >> 32;

    // Coordinates packed to ensure a raster-style sort order
    public ushort X1 => (ushort)((Packed >> 16) & 0xffff);
    public ushort Y1 => (ushort)((Packed >> 48) & 0xffff);
    public ushort X2 => (ushort)((Packed >> 0) & 0xffff);
    public ushort Y2 => (ushort)((Packed >> 32) & 0xffff);
    public (ushort, ushort, ushort, ushort) Tuple => (X1, Y1, X2, Y2);

    public static bool operator ==(Edge a, Edge b) => a.Equals(b);
    public static bool operator !=(Edge a, Edge b) => !a.Equals(b);
    public static bool operator <(Edge a, Edge b) => a.Packed < b.Packed;
    public static bool operator >(Edge a, Edge b) => a.Packed > b.Packed;
    public static bool operator <=(Edge a, Edge b) => a.Packed <= b.Packed;
    public static bool operator >=(Edge a, Edge b) => a.Packed >= b.Packed;

    public bool Equals(Edge other) => Packed == other.Packed;
    public override bool Equals(object obj) => obj is Edge other && Equals(other);
    public int CompareTo(Edge other) => Packed.CompareTo(other.Packed);
    public override int GetHashCode() => (int) Packed;
    public override string ToString() => $"({X1}, {Y1})-({X2}, {Y2})";
    public bool IsHorizontal => X1 != X2 && Y1 == Y2;
    public bool IsVertical => X1 == X2 && Y1 != Y2;
}