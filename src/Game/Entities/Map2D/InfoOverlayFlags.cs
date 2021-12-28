using System;

namespace UAlbion.Game.Entities.Map2D;

[Flags]
public enum InfoOverlayFlags : byte
{
    VerbExamine    = 0x1,
    VerbManipulate = 0x2,
    VerbTalk       = 0x4,
    VerbTake       = 0x8,
}