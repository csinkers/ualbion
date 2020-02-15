using System;

namespace UAlbion.Core
{
    [Flags]
    public enum EngineFlags : uint
    {
        ShowBoundingBoxes  = 0x1,
        ShowCameraPosition = 0x2,
        FlipDepthRange     = 0x4,
        FlipYSpace         = 0x8,
        VSync              = 0x10,
    }
}
