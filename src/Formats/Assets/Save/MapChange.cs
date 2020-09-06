﻿using System;
using SerdesNet;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets.Labyrinth;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Formats.Assets.Save
{
    public class MapChange
    {
        public const int SizeOnDisk = 8;
        public enum Enum2 : byte
        {
            Common,
            Rare1,
            Rare2,
            Norm,
        }

        public byte X { get; set; }
        public byte Y { get; set; }
        public IconChangeType ChangeType { get; set; }
        public Enum2 Unk3 { get; set; } // Ranges over [0..3], 3 very popular, 0 moderately, 1 and 2 ~1% each.
        public ushort Value { get; set; }
        public MapDataId MapId { get; set; }

        public override string ToString() => $"MapΔ {X:X2} {Y:X2} {ChangeType} {Unk3} {Value:X4} {MapId}";
        public static MapChange Serdes(int i, MapChange u, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            u ??= new MapChange();
            u.X = s.UInt8(nameof(X), u.X);
            u.Y = s.UInt8(nameof(Y), u.Y);
            u.ChangeType = s.EnumU8(nameof(ChangeType), u.ChangeType);
            u.Unk3 = s.EnumU8(nameof(Unk3), u.Unk3);
            u.Value = s.UInt16(nameof(Value), u.Value);
            u.MapId = s.EnumU16(nameof(Overlay), u.MapId);
            return u;
        }
    }
}
