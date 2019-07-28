using System;

namespace UAlbion.Core
{
    [Flags]
    public enum RenderPasses : int
    {
        Standard = 1 << 0,
        AlphaBlend = 1 << 1,
        Overlay = 1 << 2,
        Duplicator = 1 << 6,
        SwapchainOutput = 1 << 7,
    }
}
