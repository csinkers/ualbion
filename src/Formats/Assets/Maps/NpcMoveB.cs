using System;

namespace UAlbion.Formats.Assets.Maps;

[Flags]
public enum NpcMoveB : byte
{
    None = 0,
    RandomMask = 3,
    Random1 = 1,
    Random2 = 2,
    Unk4 = 4,
    Stationary = 8,
}