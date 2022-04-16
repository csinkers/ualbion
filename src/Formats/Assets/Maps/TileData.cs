using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using SerdesNet;
using UAlbion.Api;

namespace UAlbion.Formats.Assets.Maps;

public class TileData
{
    public static TileData FromRaw(TileFlags raw) => new TileData { _raw = raw };
    [JsonIgnore] public ushort Index { get; set; }

    public TileLayer Layer
    {
        get => (TileLayer)(
            ((_raw & TileFlags.Layer1) != 0 ? 1 : 0) |
            ((_raw & TileFlags.Layer2) != 0 ? 2 : 0));
        set => _raw =
            _raw & ~TileFlags.LayerMask
            | (((int)value & 1) != 0 ? TileFlags.Layer1 : 0)
            | (((int)value & 2) != 0 ? TileFlags.Layer2 : 0);
    }

    public TileType Type
    {
        get => (TileType)(
                ((_raw & TileFlags.Type1) != 0 ? 1 : 0) |
                ((_raw & TileFlags.Type2) != 0 ? 2 : 0) |
                ((_raw & TileFlags.Type4) != 0 ? 4 : 0) |
                ((_raw & TileFlags.Type8) != 0 ? 8 : 0));
        set => _raw =
            _raw & ~TileFlags.TypeMask
            | (((int)value & 1) != 0 ? TileFlags.Type1 : 0)
            | (((int)value & 2) != 0 ? TileFlags.Type2 : 0)
            | (((int)value & 4) != 0 ? TileFlags.Type4 : 0)
            | (((int)value & 8) != 0 ? TileFlags.Type8 : 0);
    }

    public Passability Collision
    {
        get =>
            ((_raw & TileFlags.CollTop)    != 0 ? Passability.Top    : 0) |
            ((_raw & TileFlags.CollRight)  != 0 ? Passability.Right  : 0) |
            ((_raw & TileFlags.CollBottom) != 0 ? Passability.Bottom : 0) |
            ((_raw & TileFlags.CollLeft)   != 0 ? Passability.Left   : 0) |
            ((_raw & TileFlags.Solid)     != 0 ? Passability.Solid  : 0);
        set => _raw =
            _raw & ~TileFlags.CollMask
            | ((value & Passability.Top   ) != 0 ? TileFlags.CollTop    : 0)
            | ((value & Passability.Right ) != 0 ? TileFlags.CollRight  : 0)
            | ((value & Passability.Bottom) != 0 ? TileFlags.CollBottom : 0)
            | ((value & Passability.Left  ) != 0 ? TileFlags.CollLeft   : 0)
            | ((value & Passability.Solid ) != 0 ? TileFlags.Solid     : 0);
    }

    public SitMode SitMode
    {
        get => (SitMode)(
                ((_raw & TileFlags.Sit1) != 0 ? 1 : 0) |
                ((_raw & TileFlags.Sit2) != 0 ? 2 : 0) |
                ((_raw & TileFlags.Sit4) != 0 ? 4 : 0) |
                ((_raw & TileFlags.Sit8) != 0 ? 8 : 0));
        set => _raw =
            _raw & ~TileFlags.SitMask
            | (((int)value & 1) != 0 ? TileFlags.Sit1 : 0)
            | (((int)value & 2) != 0 ? TileFlags.Sit2 : 0)
            | (((int)value & 4) != 0 ? TileFlags.Sit4 : 0)
            | (((int)value & 8) != 0 ? TileFlags.Sit8 : 0);
    }

    public bool BackAndForth
    {
        get => (_raw & TileFlags.BackAndForth) != 0;
        set => _raw = (_raw & ~TileFlags.BackAndForth) | (value ? TileFlags.BackAndForth : 0);
    }

    public bool Unk12
    {
        get => (_raw & TileFlags.Unk12) != 0;
        set => _raw = (_raw & ~TileFlags.Unk12) | (value ? TileFlags.Unk12 : 0);
    }

    public bool Unk18
    {
        get => (_raw & TileFlags.Unk18) != 0;
        set => _raw = (_raw & ~TileFlags.Unk18) | (value ? TileFlags.Unk18 : 0);
    }

    public bool NoDraw
    {
        get => (_raw & TileFlags.NoDraw) != 0;
        set => _raw = (_raw & ~TileFlags.NoDraw) | (value ? TileFlags.NoDraw : 0);
    }

    public bool DebugDot
    {
        get => (_raw & TileFlags.DebugDot) != 0;
        set => _raw = (_raw & ~TileFlags.DebugDot) | (value ? TileFlags.DebugDot : 0);
    }

    public ushort ImageNumber { get; set; }
    [DefaultValue(1)] public byte FrameCount { get; set; } // Maximum = 8
    public byte Unk7 { get; set; }
    TileFlags _raw;

    public TileData() { }
    public TileData(TileData other) // Make a copy
    {
        if (other == null) throw new ArgumentNullException(nameof(other));
        _raw       = other._raw;
        Index       = other.Index;
        Layer       = other.Layer;
        Type        = other.Type;
        Collision   = other.Collision;
        ImageNumber = other.ImageNumber;
        FrameCount  = other.FrameCount;
        Unk7        = other.Unk7;
    }

    public TileData(int index, ushort imageNumber, TileType type, TileLayer layer)
    {
        _raw = 0;
        Index = (ushort)index;
        Layer = layer;
        Type = type;
        Collision = Passability.Passable;
        ImageNumber = imageNumber;
        FrameCount = 1;
        Unk7 = 0;
    }

    public int GetSubImageForTile(int tickCount)
    {
        int frames = FrameCount;
        if (tickCount > 0 && FrameCount > 1)
            frames = frames > 6 ? frames : (int)(frames + 0.01);
        if (frames == 0)
            frames = 1;

        return ImageNumber + tickCount % frames;
    }

    public override string ToString() => $"Tile {_raw:X} ->{ImageNumber}:{FrameCount} Unk7: {Unk7}";
    [JsonIgnore] public int Depth => Layer.ToDepthOffset();

    public static TileData Serdes(int _, TileData t, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        t ??= new TileData();
        t._raw = s.EnumU32(nameof(_raw), t._raw); // 0
        t.ImageNumber = s.UInt16(nameof(ImageNumber), t.ImageNumber); // 4
        t.FrameCount = s.UInt8(nameof(FrameCount), t.FrameCount); // 6
        t.Unk7 = s.UInt8(nameof(Unk7), t.Unk7); // 7

        if ((t._raw & TileFlags.UnusedMask) != 0)
            ApiUtil.Assert($"Unused flags set: {t._raw & TileFlags.UnusedMask}");

        return t;
    }

    public bool IsBlank =>
        Collision == 0 &&
        _raw == TileFlags.None &&
        FrameCount == 0 &&
        ImageNumber == 0 &&
        Layer == TileLayer.Normal &&
        Type == TileType.Unk0 &&
        Unk7 == 0;

}
