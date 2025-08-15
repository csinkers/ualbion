﻿using System;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.Assets.Labyrinth;

public class FloorAndCeiling // Length = 0xA (10) bytes
{
    [Flags]
    public enum FcFlags : byte
    {
        Bouncy = 1,
        SelfIlluminating = 1 << 1,
        NonWalkable = 1 << 2,
        Unknown3 = 1 << 3,
        Unknown4 = 1 << 4,
        Walkable = 1 << 5,
        Grayed = 1 << 6,
        SelfIlluminatingColour = 1 << 7,
    }

    public FcFlags Properties { get; set; }
    public byte Unk1 { get; set; }
    public byte Unk2 { get; set; }
    public byte Unk3 { get; set; }
    public byte FrameCount { get; set; } = 1;
    public byte Unk5 { get; set; }
    public SpriteId SpriteId { get; set; } // ushort
    public ushort Unk8 { get; set; }
    public override string ToString() => $"FC.{SpriteId}:{FrameCount} {Properties}";

    public static FloorAndCeiling Serdes(SerdesName _, FloorAndCeiling existing, AssetMapping mapping, ISerdes s)
    {
        ArgumentNullException.ThrowIfNull(s);
        var fc = existing ?? new FloorAndCeiling();
        fc.Properties = s.EnumU8(nameof(fc.Properties), fc.Properties);
        fc.Unk1 = s.UInt8(nameof(fc.Unk1), fc.Unk1);
        fc.Unk2 = s.UInt8(nameof(fc.Unk2), fc.Unk2);
        fc.Unk3 = s.UInt8(nameof(fc.Unk3), fc.Unk3);
        fc.FrameCount = s.UInt8(nameof(fc.FrameCount), fc.FrameCount);
        fc.Unk5 = s.UInt8(nameof(fc.Unk5), fc.Unk5);
        fc.SpriteId = SpriteId.SerdesU16(nameof(SpriteId), fc.SpriteId, AssetType.Floor, mapping, s);
        fc.Unk8 = s.UInt16(nameof(fc.Unk8), fc.Unk8);
        return fc;
    }
}
