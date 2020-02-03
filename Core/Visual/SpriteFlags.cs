using System;

namespace UAlbion.Core.Visual
{
    [Flags]
    public enum SpriteFlags : uint
    {
        OpacityMask    = 0xff000000,
        AlignmentMask  = 0x7,

        LeftAligned    =    0x1, // Horizontal alignment
        MidAligned     =    0x2, // Vertical alignment
        BottomAligned  =    0x4, // Vertical alignment
        FlipVertical   =    0x8, // Flip vertical texture coordinate
        NoTransform    =   0x10,
        Floor          =   0x20,
        Billboard      =   0x40,
        NoDepthTest    =   0x80,
        UsePalette     =  0x100,
        OnlyEvenFrames =  0x200,
        Transparent    =  0x400,
        Highlight      =  0x800,
        RedTint        = 0x1000, // Multi-sprite flag.
        GreenTint      = 0x2000,
        BlueTint       = 0x4000,
        DropShadow     = 0x8000,
    }

    // If we put masks that overlap single flags directly into the flags enum then it messes up the ToString() results, making debugging harder.
    public enum SpriteFlagMask : uint
    {
        SpriteKey = // Within a multi-sprite, all the instances need to share these flags.
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
