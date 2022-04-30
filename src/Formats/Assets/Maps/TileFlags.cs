using System;

namespace UAlbion.Formats.Assets.Maps;

[Flags]
public enum TileFlags : uint
{
    None = 0,
    TypeMask = Type1 | Type2 | Type4,
    LayerMask = Layer1 | Layer2,
    CollMask = CollTop | CollRight | CollBottom | CollLeft | Solid,
    SitMask = Sit1 | Sit2 | Sit4 | Sit8,
    MiscMask = Bouncy | UseUnderlayFlags | Unk12 | Unk18 | NoDraw | DebugDot,
    UnusedMask = ~(TypeMask | LayerMask | CollMask | SitMask | MiscMask),

    // Animation cycle options
    Bouncy = 1, // 0x00000001 - Animation bounces back and forth from first to last frame instead of starting over again
    UseUnderlayFlags = 1U << 1, // 0x00000002 a small orange debug marker shown in top left when this is set on an overlay tile
    Type1      = 1U << 2,  // 0x00000004  
    Type2      = 1U << 3,  // 0x00000008  
    Type4      = 1U << 4,  // 0x00000010  

    // Layering options - Used to set tile ordering relative to the top/middle/bottom of NPC/player sprites
    Layer1     = 1U << 5,  // 0x00000020
    Layer2     = 1U << 6,  // 0x00000040  

    // Collision options
    CollTop    = 1U << 7,  // 0x00000080  Overlay: Orange line marker on top side
    CollRight  = 1U << 8,  // 0x00000100  Overlay: Orange line marker on right side
    CollBottom = 1U << 9,  // 0x00000200  Overlay: Orange line marker on bottom side
    CollLeft   = 1U << 10, // 0x00000400  Overlay: Orange line marker on left side
    Solid      = 1U << 11, // 0x00000800  Underlay: White solid debug marker. Overlay: Orange solid debug marker.

    Unk12      = 1U << 12, // 0x00001000  
    Unused13   = 1U << 13, // 0x00002000  
    Unused14   = 1U << 14, // 0x00004000  
    Unused15   = 1U << 15, // 0x00008000  
    Unused16   = 1U << 16, // 0x00010000  
    Unused17   = 1U << 17, // 0x00020000  
    Unk18      = 1U << 18, // 0x00040000  
    Unused19   = 1U << 19, // 0x00080000  
    Unused20   = 1U << 20, // 0x00100000  

    // Debug options
    NoDraw     = 1U << 21, // 0x00200000  
    DebugDot   = 1U << 22, // 0x00400000  Orange dot in lower right, both layers.

    // Sitting options
    Sit1       = 1U << 23, // 0x00800000  
    Sit2       = 1U << 24, // 0x01000000  
    Sit4       = 1U << 25, // 0x02000000  
    Sit8       = 1U << 26, // 0x04000000  

    Unused27   = 1U << 27, // 0x08000000  
    Unused28   = 1U << 28, // 0x10000000  
    Unused29   = 1U << 29, // 0x20000000  
    Unused30   = 1U << 30, // 0x40000000  
    Unused31   = 1U << 31, // 0x80000000
}
