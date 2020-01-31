using System;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Parsers;

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

        public static void Serialize(FloorAndCeiling fc, ISerializer s)
        {
            s.EnumU8(nameof(fc.Properties), () => fc.Properties, x => fc.Properties = x, x => ((byte)x, x.ToString()));
            s.Dynamic(fc, nameof(fc.Unk1));
            s.Dynamic(fc, nameof(fc.Unk2));
            s.Dynamic(fc, nameof(fc.Unk3));
            s.Dynamic(fc, nameof(fc.AnimationCount));
            s.Dynamic(fc, nameof(fc.Unk5));
            s.UInt16(nameof(fc.TextureNumber),
                () => FormatUtil.Untweak((ushort?)fc.TextureNumber),
                x => fc.TextureNumber = (DungeonFloorId?)FormatUtil.Tweak(x));

            s.Dynamic(fc, nameof(fc.Unk8));
        }
    }
}