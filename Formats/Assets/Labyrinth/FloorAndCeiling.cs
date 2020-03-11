using System;
using SerdesNet;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.Assets.Labyrinth
{
    public class FloorAndCeiling
    {
        [Flags]
        public enum FcFlags : byte
        {
            Unknown0 = 1 << 0,
            SelfIlluminating = 1 << 1,
            NotWalkable = 1 << 2,
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
        public byte AnimationCount { get; set; }
        public byte Unk5 { get; set; }
        public DungeonFloorId? TextureNumber { get; set; } // ushort
        public ushort Unk8 { get; set; }
        public override string ToString() => $"FC.{TextureNumber}:{AnimationCount} {Properties}";

        public static FloorAndCeiling Serdes(int _, FloorAndCeiling existing, ISerializer s)
        {
            var fc = existing ?? new FloorAndCeiling();
            fc.Properties = s.EnumU8(nameof(fc.Properties), fc.Properties);
            fc.Unk1 = s.UInt8(nameof(fc.Unk1), fc.Unk1);
            fc.Unk2 = s.UInt8(nameof(fc.Unk2), fc.Unk2);
            fc.Unk3 = s.UInt8(nameof(fc.Unk3), fc.Unk3);
            fc.AnimationCount = s.UInt8(nameof(fc.AnimationCount), fc.AnimationCount);
            fc.Unk5 = s.UInt8(nameof(fc.Unk5), fc.Unk5);
            fc.TextureNumber = (DungeonFloorId?)Tweak.Serdes(nameof(TextureNumber), (ushort?)fc.TextureNumber, s.UInt16);
            fc.Unk8 = s.UInt16(nameof(fc.Unk8), fc.Unk8);
            return fc;
        }
    }
}
