using System;
using System.Numerics;
using UAlbion.Core.Visual;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid.Etm;

public partial struct DungeonTile : IVertexFormat, IEquatable<DungeonTile>
{
    [Vertex("Textures")] public uint Textures { get; set; }
    [Vertex("WallSize")] public Vector2 WallSize { get; set; }
    [Vertex("Flags", EnumPrefix = "TF")] public DungeonTileFlags Flags { get; set; }

    public byte Floor // 0 = No floor
    {
        get => (byte)(Textures & 0xff);
        set => Textures = (Textures & 0xffffff00) | value;
    }

    public byte Ceiling // 0 = No Ceiling
    {
        get => (byte)((Textures >> 8) & 0xff);
        set => Textures = (Textures & 0xffff00ff) | ((uint)value << 8);
    }

    public byte Wall // 0 = No Wall
    {
        get => (byte)((Textures >> 16) & 0xff);
        set => Textures = (Textures & 0xff00ffff) | ((uint)value << 16);
    }

    public byte Overlay // 0 = No overlay
    {
        get => (byte)((Textures >> 24) & 0xff);
        set => Textures = (Textures & 0x00ffffff) | ((uint)value << 24);
    }

    public override string ToString() => $"{Floor}.{Ceiling}.{Wall} ({Flags})";
    public override bool Equals(object obj) => obj is DungeonTile other && Equals(other);

    public bool Equals(DungeonTile other) =>
        Floor == other.Floor &&
        Ceiling == other.Ceiling && 
        Wall == other.Wall &&
        Flags == other.Flags && 
        WallSize == other.WallSize;

    public override int GetHashCode() => HashCode.Combine(Textures, Flags, WallSize);
    public static bool operator ==(DungeonTile left, DungeonTile right) => left.Equals(right);
    public static bool operator !=(DungeonTile left, DungeonTile right) => !(left == right);
}