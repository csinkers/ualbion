using System;

namespace UAlbion.Game.Settings;

[Flags]
public enum DebugFlags
{
    DrawPositions      = 0x1,
    HighlightTile      = 0x2,
    HighlightChain     = 0x4,
    ShowCursorHotspot  = 0x8,
    ShowConsole        = 0x10,
    TraceAttachment    = 0x20,
    CollisionLayer     = 0x40,
    SitLayer           = 0x80,
    ZoneLayer          = 0x100,
    NpcColliderLayer   = 0x200,
    NpcPathLayer       = 0x400,
}