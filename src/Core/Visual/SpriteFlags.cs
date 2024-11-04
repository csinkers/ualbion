﻿using System;
using System.Diagnostics.CodeAnalysis;

namespace UAlbion.Core.Visual;

/// <summary>
/// Flags that can vary per-instance.
/// </summary>
[Flags]
[SuppressMessage("", "CA2217")]
public enum SpriteFlags : uint
{
    None           =        0,
    AlignmentMask  =      0x7,
    DebugMask      =    0xe00,
    OpacityMask  = 0xff000000, // MSB is opacity

    TopMid = None,
    TopLeft = LeftAligned,
    MidMid = MidAligned,
    MidLeft = MidAligned |  LeftAligned,
    BottomMid = BottomAligned,
    BottomLeft = BottomAligned | LeftAligned,

#pragma warning disable CA1069 // Enums values should not be duplicated

    LeftAligned    =      0x1, // Horizontal alignment (default is mid aligned)
    MidAligned     =      0x2, // Vertical alignment (default is top aligned)
    BottomAligned  =      0x4, // Vertical alignment

#pragma warning restore CA1069 // Enums values should not be duplicated

    FlipVertical   =      0x8, // Flip vertical texture coordinate
    Floor          =     0x10, // On the floor rather than standing upright
    Billboard      =     0x20, // Autorotate to face the camera
    OnlyEvenFrames =     0x40, // Used for monsters etc where the odd frames are shadows
    // 0x80 Unused
    Highlight      =    0x100,
    RedTint        =    0x200, // Debug Flag
    GreenTint      =    0x400, // Debug Flag
    BlueTint       =    0x800, // Debug Flag
    DropShadow     =   0x1000, // Render the whole sprite area in black, for UI drop shadow effects
    NoBoundingBox  =   0x2000, // Make this sprite an exemption when the bounding box uniform is set.
    GradientPixels =   0x4000, // Used for large portraits on inventory screen, renders each pixel as a box with gradients.
    //                 0x8000,
    //                0x10000,
    //                0x20000,
    //                0x40000,
    //                0x80000,
    //               0x100000,
    //               0x200000,
    //               0x400000,
    //               0x800000

    // 0x800000 // Max flag before opacity mask
}

public static class SpriteFlagExtensions
{
    public static SpriteFlags SetOpacity(this SpriteFlags flags, float opacity) =>
        (SpriteFlags)(((uint)flags & ~(uint)SpriteFlags.OpacityMask)
                      |
                      ((uint)(Math.Clamp(opacity, 1/255.0f, 1.0f) * 255) << 24));
}