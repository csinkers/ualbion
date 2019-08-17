using System;

namespace UAlbion.Core
{
    [Flags]
    public enum RenderPasses : int
    {
        Standard = 1 << 0,
        Overlay = 1 << 1,
        Duplicator = 1 << 2,
        SwapchainOutput = 1 << 3,
    }
}
