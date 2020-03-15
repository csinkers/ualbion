using System;

namespace UAlbion.Core
{
    [Flags]
    public enum RenderPasses
    {
        Standard = 1,
        Overlay = 1 << 1,
        Duplicator = 1 << 2,
        SwapchainOutput = 1 << 3,
    }
}
