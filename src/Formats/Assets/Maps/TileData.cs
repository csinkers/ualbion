using System;
using SerdesNet;
using UAlbion.Api;

namespace UAlbion.Formats.Assets.Maps
{
    public class TileData
    {
        public int TileNumber { get; set; }
        public TileLayer Layer { get; set; } // Upper nibble of first byte
        public TileType Type { get; set; } // Lower nibble of first byte
        public Passability Collision { get; set; }
        public TileFlags Flags { get; set; }
        public ushort ImageNumber { get; set; }
        public byte FrameCount { get; set; }
        public byte Unk7 { get; set; }

        public int GetSubImageForTile(int tickCount)
        {
            int frames = FrameCount;
            if (tickCount > 0 && FrameCount > 1)
                frames = frames > 6 ? frames : (int)(frames + 0.01);
            if (frames == 0)
                frames = 1;

            return ImageNumber + tickCount % frames;
        }

        public override string ToString() => $"Tile{TileNumber} {Layer} {Type} {Collision} {Flags} ->{ImageNumber}:{FrameCount} Unk7: {Unk7}";
        public int Depth => Type.ToDepthOffset() + Layer.ToDepthOffset();

        public static TileData Serdes(int i, TileData t, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            t ??= new TileData { TileNumber = i };

            byte firstByte = (byte)((int) t.Layer << 4 | (int) t.Type); 
            firstByte = s.UInt8("LayerAndType", firstByte); // 0
            t.Layer = (TileLayer)(firstByte >> 4); // Upper nibble of first byte (0h)
            t.Type = (TileType)(firstByte & 0xf); // Lower nibble of first byte (0l)

            t.Collision = s.EnumU8(nameof(Collision), t.Collision); // 1
            t.Flags = s.EnumU16(nameof(Flags), t.Flags); // 2
            ApiUtil.Assert((t.Flags & TileFlags.UnusedMask) == 0, "Unused flags set");
            t.ImageNumber = s.UInt16(nameof(ImageNumber), t.ImageNumber); // 4
            t.FrameCount = s.UInt8(nameof(FrameCount), t.FrameCount); // 6
            t.Unk7 = s.UInt8(nameof(Unk7), t.Unk7); // 7
            return t;
        }
    }
}
