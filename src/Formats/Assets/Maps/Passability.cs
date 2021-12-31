using System;

namespace UAlbion.Formats.Assets.Maps;

[Flags]
public enum Passability : byte
{
    Passable = 0,
    Top    = 1 << 1,
    Right  = 1 << 2,
    Bottom = 1 << 3,
    Left   = 1 << 4,
    Solid  = 1 << 5,
}