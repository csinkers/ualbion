using System;

namespace UAlbion.Game.Settings
{
    [Flags]
    public enum DebugFlags : uint
    {
        DrawPositions            = 0x1,
        HighlightTile            = 0x2,
        HighlightSelection       = 0x4,
        HighlightEventChainZones = 0x8,
        HighlightCollision       = 0x10,
        ShowPaths                = 0x20,
        NoMapTileBoundingBoxes   = 0x40,
    }
}
