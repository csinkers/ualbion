using System;

namespace UAlbion.Game.Settings;

[Flags]
public enum DebugFlags
{
    DrawPositions      = 0x1,
    HighlightTile      = 0x2,
    HighlightChain     = 0x4,
    ShowCursorHotspot  = 0x8,
    TraceAttachment    = 0x10,
    CollisionLayer     = 0x20,
    SitLayer           = 0x40,
    MiscLayer          = 0x80,
    ZoneLayer          = 0x100,
    NpcColliderLayer   = 0x200,
    NpcPathLayer       = 0x400,
    ShowDebugTiles     = 0x800,
    FastMovement       = 0x1000,
}