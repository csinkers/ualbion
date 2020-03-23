using System;
using System.Collections.Generic;
using UAlbion.Api;

namespace UAlbion.Formats.Assets
{
    public class TilesetData
    {
        public enum TileLayer : byte // Upper nibble of first byte
        {
            Normal = 0, // Most floors, low and centre EW walls
            Layer1 = 2, // Mid EW walls, Overlay1
            Layer2 = 4, // Overlay2
            Layer3 = 6, // NS walls + Overlay3
            Unk8 = 8,
            Unk10 = 10,
            Unk12 = 12,
            Unk14 = 14,

            Unused1 = 1,
            Unused3 = 3,
            Unused5 = 5,
            Unused7 = 7,
            Unused9 = 9,
            Unused11 = 11,
            Unused13 = 13,
            Unused15 = 15,
        }

        public enum TileType : byte
        {
            Normal = 0,   // Standard issue
            UnderlayIat = 1, // Underlay
            UnderlayCeltFloor = 2, // Underlay, celtic floors, toilet seats, random square next to pumps, top shelves of desks
            Overlay1 = 4, // Overlay
            Overlay2 = 5, // Overlay, only on continent maps?
            Overlay3 = 6, // Overlay
            Unk7 = 7,     // Overlay used on large plants on continent maps.
            Unk8 = 8,     // Underlay used on OkuloKamulos maps around fire
            Unk12 = 12,   // Overlay used on OkuloKamulos maps for covered fire / grilles
            Unk14 = 14,   // Overlay used on OkuloKamulos maps for open fire

            Unused3 = 3,
            Unused9 = 9,
            Unused10 = 10,
            Unused11 = 11,
            Unused13 = 13,
            Unused15 = 15,
        }

        [Flags]
        public enum TileFlags : ushort
        {
            Unused0   = 1, // Not used?
            Unused1   = 1 << 1, // Not used?
            // Set on stairs on 117
            // Set on chairs on 129, 130, 131, 139, 141, 142 (+stairs), 213, 230, 231
            // Rock corners, 134, 143 (+stairs)
            // Cliff edges 200-207, 210
            // Lamps 243
            Unk2 = 1 << 2,
            Unused3   = 1 << 3, // Not used?
            Unused4   = 1 << 4, // Not used?
            // Bed 280, 111, 112
            // Sword on wall 108, 127, 131
            // Trees 207
            // Rando floor tiles 231, 236, 237
            // Comms room shelf 300
            // Computer terminals 301, 302, 305
            Debug = 1 << 5,

            // Direction bits:
            // BGR
            // 000 = no sit
            // 001 = S facing E half of double-wide & top of column?? (300)
            // 010 = S facing W half of double-wide + regular S facing
            // 011 = Terminals & high-backed chair SE section (300)
            // 100 =
            // 101 = W facing
            // 110 = E facing + beds
            // 111 = S facing E part of triple bench (300)

            // Double-wide south-facing seats 230
            // E half of sign 278
            Dir1 = 1 << 6,
            // Seated facing south?
            Dir2 = 1 << 7,
            // Seated facing north? (Beds have bits 7 & 8 set)
            // Also set on shoals on continent 201, 205, 206
            // West facing = bits 6 & 8
            Dir3 = 1 << 8,
            Dir4 = 1 << 9, // Sitting related (primarily south & west facing + beds)
            Dir5 = 1 << 10, // Bed related + seen on bridges & shallows for continent tilesets
            Unused11 = 1 << 11, // Unused?
            Unused12  = 1 << 12, // Unused
            Unused13  = 1 << 13, // Unused
            Unused14  = 1 << 14, // Unused
            Unused15  = 1 << 15,

            UnusedMask = Unused0 | Unused1 | Unused3 | Unused4 | Unused11 | Unused12 | Unused13 | Unused14 | Unused15
        }

        public enum Passability
        {
            Passable = 0,
            Passability1 = 1,
            Passability2 = 2,
            Passability3 = 3,
            Passability4 = 4,
            Passability5 = 5,
            Passability6 = 6,
            Blocked = 8,
            Passability9 = 9,
            Passability10 = 10,
            Passability12 = 12,
            Passability16 = 16,
            Passability24 = 24,
        }

        public class TileData
        {
            public int TileNumber;
            public TileLayer Layer; // Upper nibble of first byte
            public TileType Type; // Lower nibble of first byte
            public Passability Collision;
            public TileFlags Flags;
            public ushort ImageNumber;
            public byte FrameCount;
            public byte Unk7;
            public int GetSubImageForTile(int tickCount)
            {
                int frames = FrameCount;
                if (tickCount > 0 && FrameCount > 1)
                    frames = frames > 6 ? frames : (int) (frames + 0.01);
                if (frames == 0)
                    frames = 1;

                return ImageNumber + tickCount % frames;
            }

            public override string ToString() => $"Tile{TileNumber} {Layer} {Type} {Collision} {Flags} ->{ImageNumber}:{FrameCount} Unk7: {Unk7}";

            public DrawLayer ToDrawLayer()
            {
                var baseLayer = ((int)Type & 0x7) switch
                {
                    (int)TileType.Normal => DrawLayer.Underlay,
                    (int)TileType.Overlay1 => DrawLayer.Overlay1,
                    (int)TileType.Overlay2 => DrawLayer.Overlay2,
                    (int)TileType.Overlay3 => DrawLayer.Overlay3,
                    (int)TileType.Unk7 => DrawLayer.Overlay3,
                    _ => DrawLayer.Underlay
                };

                var adjustment = ((int)Layer & 0x7) switch
                {
                    (int)TileLayer.Normal => 0,
                    (int)TileLayer.Layer1 => 1,
                    (int)TileLayer.Layer2 => 2,
                    (int)TileLayer.Layer3 => 3,
                    _ => 0
                };

                return (DrawLayer)((int)baseLayer + adjustment);
            }
        }

        public bool UseSmallGraphics { get; set; }
        public IList<TileData> Tiles { get; } = new List<TileData>();
    }
}
