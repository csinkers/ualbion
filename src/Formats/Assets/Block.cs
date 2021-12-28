using System;
using System.Text.Json.Serialization;
using SerdesNet;

namespace UAlbion.Formats.Assets;

public class Block
{
    int[] _underlay;
    int[] _overlay;

    public Block() { }
    public Block(byte width, byte height, int[] underlay, int[] overlay)
    {
        Width = width;
        Height = height;
        _underlay = underlay ?? throw new ArgumentNullException(nameof(underlay));
        _overlay = overlay ?? throw new ArgumentNullException(nameof(overlay));

        if (Width * Height != underlay.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(underlay),
                $"Underlay should consist of {Width * Height} tiles " +
                $"(i.e. {Width}x{Height}) but actually contains {underlay.Length}");
        }
        if (Width * Height != overlay.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(overlay),
                $"Overlay should consist of {Width * Height} tiles " +
                $"(i.e. {Width}x{Height}) but actually contains {overlay.Length}");
        }
    }

    public byte Width { get; set; }
    public byte Height { get; set; }
    [JsonIgnore] public ReadOnlySpan<int> Underlay => _underlay;
    [JsonIgnore] public ReadOnlySpan<int> Overlay => _overlay;
    public override string ToString() => $"BLK {Width}x{Height}";

    public byte[] RawLayout
    {
        get => FormatUtil.ToPacked(_underlay, _overlay, 1);
        set => (_underlay, _overlay) = FormatUtil.FromPacked(value, -1);
    }

    public static Block Serdes(int _, Block b, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        b ??= new Block();
        b.Width = s.UInt8(nameof(Width), b.Width);
        b.Height = s.UInt8(nameof(Height), b.Height);
        b._underlay ??= new int[b.Width * b.Height];
        b._overlay ??= new int[b.Width * b.Height];

        if (s.IsReading())
            b.RawLayout = s.Bytes("Layout", null, 3 * b.Width * b.Height);
        else
            s.Bytes("Layout", b.RawLayout, 3 * b.Width * b.Height);

        return b;
    }
}