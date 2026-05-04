// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
// !!! This file was auto-generated using VeldridGen. It should not be edited by hand. !!!
// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
//!#version 450 // Comments with //! are just for the VS GLSL plugin
//!#extension GL_KHR_vulkan_glsl: enable

// RenderableType
#define DRT_LINE2_D 0x0
#define DRT_VECTOR2_D 0x1
#define DRT_LINE3_D 0x2
#define DRT_VECTOR3_D 0x3
#define DRT_SPHERE3_D 0x4

// CoordinateSystem
#define CS_UI2_D 0x0
#define CS_WORLD2_D 0x1
#define CS_CAMERA2_D 0x2
#define CS_SCREEN2_D 0x3
#define CS_NORM2_D 0x4
#define CS_TILE3_D 0x5
#define CS_WORLD3_D 0x6
#define CS_CAMERA3_D 0x7
#define CS_NORM3_D 0x8

// EngineFlags
#define EF_SHOW_BOUNDING_BOXES 0x1U
#define EF_SHOW_CAMERA_POSITION 0x2U
#define EF_FLIP_DEPTH_RANGE 0x4U
#define EF_FLIP_YSPACE 0x8U
#define EF_VSYNC 0x10U
#define EF_HIGHLIGHT_SELECTION 0x20U
#define EF_USE_CYLINDRICAL_BILLBOARDS 0x40U
#define EF_RENDER_DEPTH 0x80U
#define EF_SUPPRESS_LAYOUT 0x100U
#define EF_FIXED_TIME_STEP 0x200U

layout(set = 0, binding = 0) uniform _Shared {
    float uTime;
    uint uEngineFlags;
    float uPaletteBlend;
    int uPaletteFrame;
};

layout(set = 1, binding = 0) uniform _Camera {
    mat4 uProjection;
    mat4 uView;
    vec3 uWorldSpacePosition;
    uint _globalInfo_pad1;
    vec2 uCameraLookDirection;
    vec2 uResolution;
};


// UAlbion.Core.Veldrid.DebugVertex
layout(location = 0) in vec3 iPosition;

// UAlbion.Core.Veldrid.GpuDebugInstanceData
layout(location = 1) in int iType;
layout(location = 2) in int iCSys;
layout(location = 3) in vec4 iA;
layout(location = 4) in vec4 iB;

// UAlbion.Core.Veldrid.DebugIntermediate
layout(location = 0) out vec2 oTexCoords;

