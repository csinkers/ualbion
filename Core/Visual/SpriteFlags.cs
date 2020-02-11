using System;

namespace UAlbion.Core.Visual
{
    /// <summary>
    /// Flags that can vary per-instance.
    /// </summary>
    [Flags]
    public enum SpriteFlags : uint
    {
        None = 0,
        // LSB is SpriteFlags
        AlignmentMask = 0x7,
        DebugFlags = 0xe00,
        OpacityMask = 0xff000000, // MSB is opacity

        LeftAligned    =    0x1, // Horizontal alignment
        MidAligned     =    0x2, // Vertical alignment
        BottomAligned  =    0x4, // Vertical alignment

        FlipVertical   =    0x8, // Flip vertical texture coordinate
        Floor          =   0x10,
        Billboard      =   0x20,
        OnlyEvenFrames =   0x40,
        Transparent    =   0x80,
        Highlight      =  0x100,
        RedTint        =  0x200, // Multi-sprite flag.
        GreenTint      =  0x400,
        BlueTint       =  0x800,
        DropShadow     = 0x1000,
    }

    public static class SpriteFlagExtensions
    {
        public static SpriteFlags SetOpacity(this SpriteFlags flags, float opacity) => 
            (SpriteFlags)(((uint)flags & ~(uint)SpriteFlags.OpacityMask) 
                          |
                          ((uint)(Math.Clamp(opacity, 1/255.0f, 1.0f) * 255) << 24));
    }
}
