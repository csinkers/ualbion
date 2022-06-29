using System;

namespace UAlbion.Core;

[Flags]
public enum EngineFlags : uint
{
    ShowBoundingBoxes        =   0x1,
    ShowCameraPosition       =   0x2,
    FlipDepthRange           =   0x4,
    FlipYSpace               =   0x8,
    VSync                    =  0x10,
    HighlightSelection       =  0x20,
    UseCylindricalBillboards =  0x40,
    RenderDepth              =  0x80,
    SuppressLayout           = 0x100,
}