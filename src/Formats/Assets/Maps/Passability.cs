using System;

namespace UAlbion.Formats.Assets.Maps;

[Flags]
public enum Passability : byte
{
    Open = 0,
    BlockNorth = 0x1,
    BlockEast  = 0x2,
    BlockSouth = 0x4,
    BlockWest  = 0x8,
    Solid = 0x10,
}