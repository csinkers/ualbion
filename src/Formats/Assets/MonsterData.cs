using System;
using System.Linq;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.Assets;

public class MonsterData
{
    // Guesses based on Ambermoon
    // public CombatAnimation[] Animations { get; set; } = new CombatAnimation[8]; // 0
    // public byte[] AnimationFrames { get; set; } = new byte[8]; // 100
    // public byte[] Unk108 { get; set; } = new byte[16]; // 108
    // public byte[] PaletteMapping { get; set; } = new byte[32]; // 118
    // public ushort Unk138 { get; set; }
    // public ushort Width { get; set; }
    // public ushort Height { get; set; }
    // public ushort DisplayWidth { get; set; }
    // public ushort DisplayHeight { get; set; }

    public SpriteId MonsterGraphics { get; set; }

    // Hardcoded offset of 12. This is pretty nasty, but it's how it was done in the original.
    // TODO: Make this data driven if modding ever requires it.
    public SpriteId TacticalGraphics => new(AssetType.TacticalGfx, MonsterGraphics.Id + 12);

    public byte Unk1     { get; set; }
    public byte[] Unk2   { get; set; }
    public ushort Unk34    { get; set; } // Override MonsterGfx??
    public ushort Unk36    { get; set; }
    public uint Unk38    { get; set; }
    public uint Unk3c    { get; set; }
    public byte[] Unk40  { get; set; }
    public uint Unk58    { get; set; }
    public uint Unk5c    { get; set; }
    public uint Unk60    { get; set; }
    public uint Unk64    { get; set; }
    public uint Unk68    { get; set; }
    public uint Unk6c    { get; set; }
    public uint Unk70    { get; set; }
    public uint Unk74    { get; set; }
    public uint Unk78    { get; set; }
    public uint Unk7c    { get; set; }
    public uint Unk80    { get; set; }
    public uint Unk84    { get; set; }
    public uint Unk88    { get; set; }
    public byte[] Unk8c  { get; set; }
    public uint Unk98    { get; set; }
    public uint Unk9c    { get; set; }
    public uint Unka0    { get; set; }
    public uint Unka4    { get; set; }
    public uint Unka8    { get; set; }
    public uint Unkac    { get; set; }
    public uint Unkb0    { get; set; }
    public uint Unkb4    { get; set; }
    public uint Unkb8    { get; set; }
    public uint Unkbc    { get; set; }
    public byte[] Unkc0  { get; set; }
    public uint Unkd8    { get; set; }
    public uint Unkdc    { get; set; }
    public uint Unke0    { get; set; }
    public uint Unke4    { get; set; }
    public uint Unke8    { get; set; }
    public uint Unkec    { get; set; }
    public ulong Unkf0    { get; set; }
    public uint Unkf8    { get; set; }
    public uint Unkfc    { get; set; }
    public uint Unk100   { get; set; }
    public uint Unk104   { get; set; }
    public uint Unk108   { get; set; }
    public byte[] Unk10c { get; set; }
    public uint Unk118   { get; set; }
    public uint Unk11c   { get; set; }
    public uint Unk120   { get; set; }
    public uint Unk124   { get; set; }
    public uint Unk128   { get; set; }
    public byte[] Unk12c { get; set; }
    public uint Unk138   { get; set; }
    public uint Unk13c   { get; set; }
    public uint Unk140   { get; set; }
    public uint Unk144   { get; set; }

    public static MonsterData Serdes(MonsterData m, AssetMapping mapping, ISerializer s)
    {
        m ??= new MonsterData();
        var initial = s.Offset;
        m.MonsterGraphics = SpriteId.SerdesU8(nameof(MonsterGraphics), m.MonsterGraphics, AssetType.MonsterGfx, mapping, s);

        // 0x148 total length
        m.Unk1   = s.UInt8(nameof(Unk1), m.Unk1);
        m.Unk2   = s.Bytes(nameof(Unk2), m.Unk2, 0x32);
        m.Unk34  = s.UInt16(nameof(Unk34), m.Unk34);
        m.Unk36  = s.UInt16(nameof(Unk36), m.Unk36);
        m.Unk38  = s.UInt32(nameof(Unk38), m.Unk38);
        m.Unk3c  = s.UInt32(nameof(Unk3c), m.Unk3c);
        m.Unk40  = s.Bytes(nameof(Unk40), m.Unk40, 0x18);
        m.Unk58  = s.UInt32(nameof(Unk58), m.Unk58);
        m.Unk5c  = s.UInt32(nameof(Unk5c), m.Unk5c);
        m.Unk60  = s.UInt32(nameof(Unk60), m.Unk60);
        m.Unk64  = s.UInt32(nameof(Unk64), m.Unk64);
        m.Unk68  = s.UInt32(nameof(Unk68), m.Unk68);
        m.Unk6c  = s.UInt32(nameof(Unk6c), m.Unk6c);
        m.Unk70  = s.UInt32(nameof(Unk70), m.Unk70);
        m.Unk74  = s.UInt32(nameof(Unk74), m.Unk74);
        m.Unk78  = s.UInt32(nameof(Unk78), m.Unk78);
        m.Unk7c  = s.UInt32(nameof(Unk7c), m.Unk7c);
        m.Unk80  = s.UInt32(nameof(Unk80), m.Unk80);
        m.Unk84  = s.UInt32(nameof(Unk84), m.Unk84);
        m.Unk88  = s.UInt32(nameof(Unk88), m.Unk88);
        m.Unk8c  = s.Bytes(nameof(Unk8c), m.Unk8c, 0xc);
        m.Unk98  = s.UInt32(nameof(Unk98), m.Unk98);
        m.Unk9c  = s.UInt32(nameof(Unk9c), m.Unk9c);
        m.Unka0  = s.UInt32(nameof(Unka0), m.Unka0);
        m.Unka4  = s.UInt32(nameof(Unka4), m.Unka4);
        m.Unka8  = s.UInt32(nameof(Unka8), m.Unka8);
        m.Unkac  = s.UInt32(nameof(Unkac), m.Unkac);
        m.Unkb0  = s.UInt32(nameof(Unkb0), m.Unkb0);
        m.Unkb4  = s.UInt32(nameof(Unkb4), m.Unkb4);
        m.Unkb8  = s.UInt32(nameof(Unkb8), m.Unkb8);
        m.Unkbc  = s.UInt32(nameof(Unkbc), m.Unkbc);
        m.Unkc0  = s.Bytes(nameof(Unkc0), m.Unkc0, 0x18);
        m.Unkd8  = s.UInt32(nameof(Unkd8), m.Unkd8);
        m.Unkdc  = s.UInt32(nameof(Unkdc), m.Unkdc);
        m.Unke0  = s.UInt32(nameof(Unke0), m.Unke0);
        m.Unke4  = s.UInt32(nameof(Unke4), m.Unke4);
        m.Unke8  = s.UInt32(nameof(Unke8), m.Unke8);
        m.Unkec  = s.UInt32(nameof(Unkec), m.Unkec);
        m.Unkf0  = s.UInt64(nameof(Unkf0), m.Unkf0);
        m.Unkf8  = s.UInt32(nameof(Unkf8), m.Unkf8);
        m.Unkfc  = s.UInt32(nameof(Unkfc), m.Unkfc);
        m.Unk100 = s.UInt32(nameof(Unk100), m.Unk100);
        m.Unk104 = s.UInt32(nameof(Unk104), m.Unk104);
        m.Unk108 = s.UInt32(nameof(Unk108), m.Unk108);
        m.Unk10c = s.Bytes(nameof(Unk10c), m.Unk10c, 0xc);
        m.Unk118 = s.UInt32(nameof(Unk118), m.Unk118);
        m.Unk11c = s.UInt32(nameof(Unk11c), m.Unk11c);
        m.Unk120 = s.UInt32(nameof(Unk120), m.Unk120);
        m.Unk124 = s.UInt32(nameof(Unk124), m.Unk124);
        m.Unk128 = s.UInt32(nameof(Unk128), m.Unk128);
        m.Unk12c = s.Bytes(nameof(Unk12c), m.Unk12c, 0xc);
        m.Unk138 = s.UInt32(nameof(Unk138), m.Unk138);
        m.Unk13c = s.UInt32(nameof(Unk13c), m.Unk13c);
        m.Unk140 = s.UInt32(nameof(Unk140), m.Unk140);
        m.Unk144 = s.UInt32(nameof(Unk144), m.Unk144);

        var totalLength = s.Offset - initial;
        if (totalLength != 0x148)
            throw new FormatException($"Expected to process 0x148 bytes, but actually got {totalLength}");

        return m;
    }

    public MonsterData DeepCopy() => new MonsterData().CopyFrom(this);
    public MonsterData CopyFrom(MonsterData other)
    {
        ArgumentNullException.ThrowIfNull(other);

        MonsterGraphics = other.MonsterGraphics;
        Unk1   = other.Unk1;
        Unk2   = other.Unk2.ToArray();
        Unk1   = other.Unk1;
        Unk2   = other.Unk2.ToArray();
        Unk34  = other.Unk34;
        Unk36  = other.Unk36;
        Unk38  = other.Unk38;
        Unk3c  = other.Unk3c;
        Unk40  = other.Unk40.ToArray();
        Unk58  = other.Unk58;
        Unk5c  = other.Unk5c;
        Unk60  = other.Unk60;
        Unk64  = other.Unk64;
        Unk68  = other.Unk68;
        Unk6c  = other.Unk6c;
        Unk70  = other.Unk70;
        Unk74  = other.Unk74;
        Unk78  = other.Unk78;
        Unk7c  = other.Unk7c;
        Unk80  = other.Unk80;
        Unk84  = other.Unk84;
        Unk88  = other.Unk88;
        Unk8c  = other.Unk8c.ToArray();
        Unk98  = other.Unk98;
        Unk9c  = other.Unk9c;
        Unka0  = other.Unka0;
        Unka4  = other.Unka4;
        Unka8  = other.Unka8;
        Unkac  = other.Unkac;
        Unkb0  = other.Unkb0;
        Unkb4  = other.Unkb4;
        Unkb8  = other.Unkb8;
        Unkbc  = other.Unkbc;
        Unkc0  = other.Unkc0.ToArray();
        Unkd8  = other.Unkd8;
        Unkdc  = other.Unkdc;
        Unke0  = other.Unke0;
        Unke4  = other.Unke4;
        Unke8  = other.Unke8;
        Unkec  = other.Unkec;
        Unkf0  = other.Unkf0;
        Unkf8  = other.Unkf8;
        Unkfc  = other.Unkfc;
        Unk100 = other.Unk100;
        Unk104 = other.Unk104;
        Unk108 = other.Unk108;
        Unk10c = other.Unk10c.ToArray();
        Unk118 = other.Unk118;
        Unk11c = other.Unk11c;
        Unk120 = other.Unk120;
        Unk124 = other.Unk124;
        Unk128 = other.Unk128;
        Unk12c = other.Unk12c.ToArray();
        Unk138 = other.Unk138;
        Unk13c = other.Unk13c;
        Unk140 = other.Unk140;
        Unk144 = other.Unk144;
        return this;
    }
}
