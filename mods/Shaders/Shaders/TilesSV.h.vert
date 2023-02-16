// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
// !!! This file was auto-generated using VeldridGen. It should not be edited by hand. !!!
// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
//!#version 450 // Comments with //! are just for the VS GLSL plugin
//!#extension GL_KHR_vulkan_glsl: enable

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

// GpuTilesetFlags
#define TSF_USE_ARRAY 0x1U
#define TSF_USE_PALETTE 0x2U
#define TSF_USE_BLEND 0x4U

// GpuTileFlags
#define TF_BOUNCY 0x1U
#define TF_USE_UNDERLAY_FLAGS 0x2U
#define TF_TYPE1 0x4U
#define TF_TYPE2 0x8U
#define TF_TYPE4 0x10U
#define TF_TYPE_MASK 0x1CU
#define TF_LAYER1 0x20U
#define TF_LAYER2 0x40U
#define TF_LAYER_MASK 0x60U
#define TF_COLL_TOP 0x80U
#define TF_COLL_RIGHT 0x100U
#define TF_COLL_BOTTOM 0x200U
#define TF_COLL_LEFT 0x400U
#define TF_SOLID 0x800U
#define TF_COLL_MASK 0xF80U
#define TF_UNK12 0x1000U
#define TF_UNK18 0x40000U
#define TF_NO_DRAW 0x200000U
#define TF_DEBUG_DOT 0x400000U
#define TF_MISC_MASK 0x641003U
#define TF_SIT1 0x800000U
#define TF_SIT2 0x1000000U
#define TF_SIT4 0x2000000U
#define TF_SIT8 0x4000000U
#define TF_SIT_MASK 0x7800000U

// GpuTileLayerFlags
#define TLF_DRAW_UNDERLAY 0x1U
#define TLF_DRAW_OVERLAY 0x2U
#define TLF_OPAQUE_UNDERLAY 0x4U
#define TLF_DRAW_COLLISION 0x8U
#define TLF_DRAW_SIT_STATE 0x10U
#define TLF_DRAW_MISC 0x20U
#define TLF_DRAW_ZONES 0x40U
#define TLF_DRAW_DEBUG 0x80U

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

layout(set = 2, binding = 3) uniform _SetUniform {
    vec2 uTileWorldSize;
    vec2 uTileUvSize;
    uint uTilesetFlags;
    uint uPad1;
    vec2 uPad2;
};

layout(set = 3, binding = 0) uniform _LayerUniform {
    uint uMapWidth;
    uint uMapHeight;
    int uFrame;
    uint uLayerFlags;
};

// UAlbion.Core.Veldrid.Vertex2D
layout(location = 0) in vec2 iPosition;

// UAlbion.Core.Veldrid.Sprites.TileIntermediateData
layout(location = 0) out vec4 oWorldPosition;
layout(location = 1) out vec2 oTilePosition;

