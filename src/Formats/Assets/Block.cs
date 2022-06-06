using System;
using System.Text.Json.Serialization;
using SerdesNet;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Formats.Assets;

public class Block
{
    MapTile[] _tiles;

    public Block() { }
    public Block(byte width, byte height, MapTile[] tiles)
    {
        Width = width;
        Height = height;
        _tiles = tiles ?? throw new ArgumentNullException(nameof(tiles));

        if (Width * Height != tiles.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(tiles),
                $"{nameof(tiles)} should contain {Width * Height} tiles " +
                $"(i.e. {Width}x{Height}) but actually contains {tiles.Length}");
        }
    }

    public byte Width { get; set; }
    public byte Height { get; set; }
    [JsonIgnore] public ReadOnlySpan<MapTile> Tiles => _tiles;
    public override string ToString() => $"BLK {Width}x{Height}";

    public byte[] RawLayout
    {
        get => MapTile.ToPacked(_tiles);
        set => _tiles = MapTile.FromPacked(value);
    }

    public static Block Serdes(int _, Block b, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        b ??= new Block();
        b.Width = s.UInt8(nameof(Width), b.Width);
        b.Height = s.UInt8(nameof(Height), b.Height);
        b._tiles ??= new MapTile[b.Width * b.Height];

        if (s.IsReading())
            b.RawLayout = s.Bytes("Layout", null, 3 * b.Width * b.Height);
        else
            s.Bytes("Layout", b.RawLayout, 3 * b.Width * b.Height);

        return b;
    }
}