using System;

namespace UAlbion.Core.Veldrid
{
    [Flags]
    public enum Renderers
    {
        Sprite = 1,
        ExtrudedTilemap = 2,
        Skybox = 4,
        DebugGui = 8,

        All = 0x7fffffff
    }
}