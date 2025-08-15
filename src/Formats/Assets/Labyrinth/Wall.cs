using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.Assets.Labyrinth;

public class Wall // Length = 0x12 + overlayCount * 0xC
{
    [Flags]
    public enum WallFlags : byte
    {
        Bouncy = 1,
        SelfIlluminating = 2,
        WriteOverlay = 4,
        Unk3 = 8,
        Unk4 = 0x10,
        AlphaTested = 0x20,
        Transparent = 0x40,
        SelfIlluminatingColour = 0x80,
    }

    public WallFlags Properties { get; set; } // 0
    public uint Collision { get; set; } // 1, len = 3 bytes
    public SpriteId SpriteId { get; set; } // 4, ushort
    public byte FrameCount { get; set; } = 1; // 6
    public byte AutoGfxType { get; set; } // 7
    public byte TransparentColour { get; set; } // 8 (PaletteId??)
    public byte Unk9 { get; set; } // 9
    public ushort Width { get; set; } // A
    public ushort Height { get; set; } // C
    [JsonInclude] public IList<Overlay> Overlays { get; private set; } = [];

    public override string ToString() =>
        $"Wall.{SpriteId}:{FrameCount} {Width}x{Height} ({Properties}) [ {string.Join(", ", Overlays.Select(x => x.ToString()))} ]";

    public static Wall Serdes(SerdesName _, Wall w, AssetMapping mapping, ISerdes s)
    {
        ArgumentNullException.ThrowIfNull(s);
        w ??= new Wall();
        w.Properties = s.EnumU8(nameof(w.Properties), w.Properties); // 0

        // Either a 24 bit int or a 3 byte array, annoying to serialize either way.
        w.Collision = (w.Collision & 0xffff00) | s.UInt8(nameof(w.Collision), (byte)(w.Collision & 0xff));
        w.Collision = (w.Collision & 0xff00ff) | (uint)s.UInt8(nameof(w.Collision), (byte)((w.Collision >> 8) & 0xff)) << 8;
        w.Collision = (w.Collision & 0x00ffff) | (uint)s.UInt8(nameof(w.Collision), (byte)((w.Collision >> 16) & 0xff)) << 16;

        w.SpriteId = SpriteId.SerdesU16(nameof(w.SpriteId), w.SpriteId, AssetType.Wall, mapping, s); // 4
        w.FrameCount = s.UInt8(nameof(w.FrameCount), w.FrameCount); // 6
        w.AutoGfxType = s.UInt8(nameof(w.AutoGfxType), w.AutoGfxType); // 7
        w.TransparentColour = s.UInt8(nameof(w.TransparentColour), w.TransparentColour); // 8
        w.Unk9 = s.UInt8(nameof(w.Unk9), w.Unk9); // 9
        w.Width = s.UInt16(nameof(w.Width), w.Width); // A
        w.Height = s.UInt16(nameof(w.Height), w.Height); // C

        ushort overlayCount = s.UInt16("overlayCount", (ushort)w.Overlays.Count); // E
        s.ListWithContext(nameof(w.Overlays), w.Overlays, mapping, overlayCount, Overlay.Serdes); // 10
        return w;
    }
}
