using System;
using System.ComponentModel;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.Assets.Labyrinth;

public class Overlay // 0xC bytes = 12
{
    public SpriteId SpriteId { get; set; } // 0, ushort
    [DefaultValue(1)]
    public byte AnimationFrames { get; set; } // 2
    public byte WriteZero { get; set; } // 3
    public ushort YOffset { get; set; } // 4
    public ushort XOffset { get; set; } // 6
    public ushort Width { get; set; }   // 8
    public ushort Height { get; set; }  // A

    public override string ToString() =>
        $"O.{SpriteId}:{AnimationFrames} ({XOffset}, {YOffset}) {Width}x{Height}";

    public static Overlay Serdes(int _, Overlay o, AssetMapping mapping, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        o ??= new Overlay();
        o.SpriteId = SpriteId.SerdesU16(nameof(o.SpriteId), o.SpriteId, AssetType.WallOverlay, mapping, s);
        o.AnimationFrames = s.UInt8(nameof(o.AnimationFrames), o.AnimationFrames);
        o.WriteZero = s.UInt8(nameof(o.WriteZero), o.WriteZero);
        o.XOffset = s.UInt16(nameof(o.XOffset), o.XOffset);
        o.YOffset = s.UInt16(nameof(o.YOffset), o.YOffset);
        o.Width = s.UInt16(nameof(o.Width), o.Width);
        o.Height = s.UInt16(nameof(o.Height), o.Height);
        return o;
    }
}