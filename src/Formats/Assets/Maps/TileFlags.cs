using System;

namespace UAlbion.Formats.Assets.Maps;

[Flags]
public enum TileFlags : uint
{
    None = 0,
    LayerMask = Layer1 | Layer2,
    TypeMask = Type1 | Type2 | Type4 | Type8,
    CollMask = CollTop | CollRight | CollBottom | CollLeft | Solid,
    SitMask = Sit1 | Sit2 | Sit4 | Sit8,
    MiscMask = Unk12 | Unk18 | NoDraw | DebugDot,
    UnusedMask = ~(LayerMask | TypeMask | CollMask | SitMask | MiscMask),

    Type1      = 1,       // 0x00000001
    Type2      = 1 << 1,  // 0x00000002  Overlay: Small orange debug marker in top left
    Type4      = 1 << 2,  // 0x00000004  
    Type8      = 1 << 3,  // 0x00000008  
    Unused4    = 1 << 4,  // 0x00000010  
    Layer1     = 1 << 5,  // 0x00000020  
    Layer2     = 1 << 6,  // 0x00000040  
    CollTop    = 1 << 7,  // 0x00000080  Overlay: Orange line marker on top side
    CollRight  = 1 << 8,  // 0x00000100  Overlay: Orange line marker on right side
    CollBottom = 1 << 9,  // 0x00000200  Overlay: Orange line marker on bottom side
    CollLeft   = 1 << 10, // 0x00000400  Overlay: Orange line marker on left side
    Solid      = 1 << 11, // 0x00000800  Underlay: White solid debug marker. Overlay: Orange solid debug marker.
    Unk12      = 1 << 12, // 0x00001000  
    Unused13   = 1 << 13, // 0x00002000  
    Unused14   = 1 << 14, // 0x00004000  
    Unused15   = 1 << 15, // 0x00008000  
    Unused16   = 1 << 16, // 0x00010000  
    Unused17   = 1 << 17, // 0x00020000  
    Unk18      = 1 << 18, // 0x00040000  
    Unused19   = 1 << 19, // 0x00080000  
    Unused20   = 1 << 20, // 0x00100000  
    NoDraw     = 1 << 21, // 0x00200000  
    DebugDot   = 1 << 22, // 0x00400000  Orange dot in lower right, both layers.
    Sit1       = 1 << 23, // 0x00800000  
    Sit2       = 1 << 24, // 0x01000000  
    Sit4       = 1 << 25, // 0x02000000  
    Sit8       = 1 << 26, // 0x04000000  
    Unused27   = 1 << 27, // 0x08000000  
    Unused28   = 1 << 28, // 0x10000000  
    Unused29   = 1 << 29, // 0x20000000  
    Unused30   = 1 << 30, // 0x40000000  
    Unused31   = 0x8000000, // 80000000
}
