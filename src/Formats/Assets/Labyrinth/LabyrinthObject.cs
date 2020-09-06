﻿using System;
using SerdesNet;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.Assets.Labyrinth
{
    public class LabyrinthObject
    {
        public LabyrinthObjectFlags Properties { get; set; } // 0
        public byte[] CollisionData { get; set; } // 1, len = 3 bytes
        public DungeonObjectId? TextureNumber { get; set; } // 4, ushort
        public byte AnimationFrames { get; set; } // 6
        public byte Unk7 { get; set; } // 7
        public ushort Width { get; set; } // 8
        public ushort Height { get; set; } // A
        public ushort MapWidth { get; set; } // C
        public ushort MapHeight { get; set; } // E

        public override string ToString() =>
            $"EO.{TextureNumber}:{AnimationFrames} {Width}x{Height} [{MapWidth}x{MapHeight}] {Properties}";

        public static LabyrinthObject Serdes(int _, LabyrinthObject o, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            o ??= new LabyrinthObject();
            o.Properties = s.EnumU8(nameof(o.Properties), o.Properties);
            o.CollisionData = s.ByteArray(nameof(o.CollisionData), o.CollisionData, 3);
            o.TextureNumber = (DungeonObjectId?)s.Transform<ushort, ushort?>(nameof(TextureNumber), (ushort?)o.TextureNumber, S.UInt16, TweakedConverter.Instance);
            o.AnimationFrames = s.UInt8(nameof(o.AnimationFrames), o.AnimationFrames);
            o.Unk7 = s.UInt8(nameof(o.Unk7), o.Unk7);
            o.Width = s.UInt16(nameof(o.Width), o.Width);
            o.Height = s.UInt16(nameof(o.Height), o.Height);
            o.MapWidth = s.UInt16(nameof(o.MapWidth), o.MapWidth);
            o.MapHeight = s.UInt16(nameof(o.MapHeight), o.MapHeight);
            return o;
        }
    }
}
