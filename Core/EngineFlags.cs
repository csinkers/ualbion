using System;

namespace UAlbion.Core
{
    [Flags]
    public enum EngineFlags : uint
    {
        ShowBoundingBoxes = 1,
        ShowCameraPosition = 2,
        FlipDepthRange = 4,
        FlipYSpace = 8,
        VSync = 16,
    }
}