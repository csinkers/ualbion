using System;

namespace UAlbion.Formats.Assets.Maps;

public struct MapTile : IEquatable<MapTile>
{
    public uint Raw { get; private set; }
    public MapTile(uint raw) => Raw = raw;
    public MapTile(ushort underlay, ushort overlay) => Raw = underlay | ((uint)overlay << 16);
    public MapTile(byte b1, byte b2, byte b3)
    {
        ushort overlay = (ushort)((b1 << 4) + (b2 >> 4));
        ushort underlay = (ushort)(((b2 & 0x0F) << 8) + b3);
        Raw = (ushort)(underlay - 1) | ((uint)(overlay - 1) << 16);
    }

    public ushort Underlay
    {
        get => (ushort)(Raw & 0xffff);
        set => Raw = (Raw & 0xffff0000) | value;
    }

    public ushort Overlay
    {
        get => (ushort)((Raw & 0xffff0000) >> 16);
        set => Raw = (Raw & 0xffff) | ((uint)value << 16);
    }

    public override string ToString() => $"Tile U{Underlay} O{Overlay}";
    public bool Equals(MapTile other) => Raw == other.Raw;
    public override bool Equals(object obj) => obj is MapTile other && Equals(other);
    public static bool operator ==(MapTile lhs, MapTile rhs) => lhs.Equals(rhs);
    public static bool operator !=(MapTile lhs, MapTile rhs) => !(lhs == rhs);
    public override int GetHashCode() => (int)Raw;

    public (byte, byte, byte) Packed
    {
        get
        {
            ushort underlay = (ushort)(Underlay + 1);
            ushort overlay = (ushort)(Overlay + 1);

            byte b1 = (byte)(overlay >> 4);
            byte b2 = (byte)(((overlay & 0xf) << 4) | ((underlay & 0xf00) >> 8));
            byte b3 = (byte)(underlay & 0xff);
            return (b1, b2, b3);
        }
    }

    public static int[] ToInts(ReadOnlySpan<MapTile> tiles, bool isOverlay)
    {
        var result = new int[tiles.Length];
        if (isOverlay)
            for (int i = 0; i < tiles.Length; i++)
                result[i] = tiles[i].Overlay;
        else
            for (int i = 0; i < tiles.Length; i++)
                result[i] = tiles[i].Underlay;

        return result;
    }

    public static MapTile[] FromInts(ReadOnlySpan<int> underlay, ReadOnlySpan<int> overlay)
    {
        if (underlay == null) throw new ArgumentNullException(nameof(underlay));
        if (overlay == null) throw new ArgumentNullException(nameof(overlay));

        if (underlay.Length != overlay.Length)
        {
            throw new ArgumentOutOfRangeException(
                "Tried to pack tiledata, but the underlay count " +
                $"({underlay.Length}) differed from the overlay count ({overlay.Length})");
        }

        var result = new MapTile[underlay.Length];
        for (int i = 0; i < underlay.Length; i++)
            result[i] = new MapTile((ushort)underlay[i], (ushort)overlay[i]);

        return result;
    }

    public static byte[] ToPacked(ReadOnlySpan<MapTile> tiles, int sourceWidth, int offsetX, int offsetY)
    {
        if (tiles == null || tiles.Length == 0)
            return Array.Empty<byte>();

        int sourceHeight = tiles.Length / sourceWidth;
        int destWidth = sourceWidth - offsetX;
        int destHeight = sourceHeight - offsetY;

        var buf = new byte[3 * destWidth * destHeight];
        for (int i = 0; i < tiles.Length; i++)
        {
            int x = i % sourceWidth;
            int y = i / sourceWidth;
            if (x < offsetX || y < offsetY)
                continue;

            int destX = x - offsetX;
            int destY = y - offsetY;
            int destIndex = destY * destWidth + destX;

            (
                buf[destIndex * 3],
                buf[destIndex * 3 + 1],
                buf[destIndex * 3 + 2]
            ) = tiles[i].Packed;
        }

        return buf;
    }

    public static MapTile[] FromPacked(ReadOnlySpan<byte> buf, int destWidth, int offsetX, int offsetY)
    {
        if (buf == null || buf.Length == 0)
            return Array.Empty<MapTile>();

        if (buf.Length % 3 != 0)
        {
            throw new InvalidOperationException(
                "Tried to set raw map data with incorrect " +
                "size (expected a multiple of 3, " +
                $"but was given {buf.Length})");
        }

        int sourceWidth = destWidth - offsetX;
        int sourceCount = buf.Length / 3;
        if (sourceCount % sourceWidth != 0)
        {
            throw new InvalidOperationException(
                "Tried to set raw map data with incorrect " +
                $"size (expected a multiple of width ({sourceWidth}), " +
                $"but was given {sourceCount} tiles)");
        }

        int sourceHeight = sourceCount / sourceWidth;
        int destHeight = sourceHeight + offsetY;

        var tiles = new MapTile[destWidth * destHeight];
        for (int i = 0; i < sourceCount; i++)
        {
            int x = i % sourceWidth;
            int y = i / sourceWidth;
            int destX = x + offsetX;
            int destY = y + offsetY;
            int destIndex = destY * destWidth + destX;

            tiles[destIndex] = new MapTile(
                buf[i * 3],
                buf[i * 3 + 1],
                buf[i * 3 + 2]);
        }

        return tiles;
    }
}