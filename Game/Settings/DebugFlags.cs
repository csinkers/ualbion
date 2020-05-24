using System;

namespace UAlbion.Game.Settings
{
    [Flags]
    public enum DebugFlags : uint
    {
        DrawPositions            = 0x1,
        HighlightTile            = 0x2,
        HighlightEventChainZones = 0x4,
        HighlightCollision       = 0x8,
        ShowPaths                = 0x10,
        NoMapTileBoundingBoxes   = 0x20,
        ShowCursorHotspot        = 0x40,
        ShowConsole              = 0x80,
        TraceAttachment         = 0x100,
    }
}
