using System;

namespace UAlbion.Core.Visual
{
    [Flags]
    public enum SpriteFlags : uint
    {
        NoTransform = 0x1,
        Highlight = 0x2,
        UsePalette = 0x4,
        OnlyEvenFrames = 0x8,
        RedTint = 0x10,
        GreenTint = 0x20,
        BlueTint = 0x40,
        // Transparent    =   0x80,
        FlipVertical = 0x100,
        Floor = 0x200,
        Billboard = 0x400,
        DropShadow = 0x800,
        LeftAligned = 0x1000, // Multi-sprite flag.
        NoDepthTest = 0x2000,

        OpacityMask = 0xff000000,
    }

    // If we put masks that overlap single flags directly into the flags enum then it messes up the ToString() results, making debugging harder.
    public enum SpriteFlagMask : uint
    {
        SpriteKey = // Within a multi-sprite, all the instances need to share these flags.
            SpriteFlags.LeftAligned | 
            SpriteFlags.NoDepthTest |
            SpriteFlags.NoTransform
    }

    public static class SpriteFlagExtensions
    {
        public static SpriteFlags SetOpacity(this SpriteFlags flags, float opacity) => 
            (SpriteFlags)(((uint)flags & ~(uint)SpriteFlags.OpacityMask) 
                          |
                          ((uint)(Math.Clamp(opacity, 1/255.0f, 1.0f) * 255) << 24));
    }
}
