using System;

namespace UAlbion.Formats.Assets.Maps
{
    [Flags]
    public enum TileFlags : ushort
    {
        Unused0 = 1, // Not used?
        Unused1 = 1 << 1, // Not used?
        // Set on stairs on 117
        // Set on chairs on 129, 130, 131, 139, 141, 142 (+stairs), 213, 230, 231
        // Rock corners, 134, 143 (+stairs)
        // Cliff edges 200-207, 210
        // Lamps 243
        Unk2 = 1 << 2,
        Unused3 = 1 << 3, // Not used?
        Unused4 = 1 << 4, // Not used?
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
        Unused12 = 1 << 12, // Unused
        Unused13 = 1 << 13, // Unused
        Unused14 = 1 << 14, // Unused
        Unused15 = 1 << 15,

        UnusedMask = Unused0 | Unused1 | Unused3 | Unused4 | Unused11 | Unused12 | Unused13 | Unused14 | Unused15
    }
}