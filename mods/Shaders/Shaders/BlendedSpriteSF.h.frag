// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
// !!! This file was auto-generated using VeldridGen. It should not be edited by hand. !!!
// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
//!#version 450 // Comments with //! are just for the VS GLSL plugin
//!#extension GL_KHR_vulkan_glsl: enable

// SpriteFlags
#define SF_NONE 0x0U
#define SF_TOP_MID 0x0U
#define SF_TOP_LEFT 0x1U
#define SF_LEFT_ALIGNED 0x1U
#define SF_MID_MID 0x2U
#define SF_MID_ALIGNED 0x2U
#define SF_MID_LEFT 0x3U
#define SF_BOTTOM_MID 0x4U
#define SF_BOTTOM_ALIGNED 0x4U
#define SF_BOTTOM_LEFT 0x5U
#define SF_ALIGNMENT_MASK 0x7U
#define SF_FLIP_VERTICAL 0x8U
#define SF_FLOOR 0x10U
#define SF_BILLBOARD 0x20U
#define SF_ONLY_EVEN_FRAMES 0x40U
#define SF_HIGHLIGHT 0x100U
#define SF_RED_TINT 0x200U
#define SF_GREEN_TINT 0x400U
#define SF_BLUE_TINT 0x800U
#define SF_DEBUG_MASK 0xE00U
#define SF_DROP_SHADOW 0x1000U
#define SF_NO_BOUNDING_BOX 0x2000U
#define SF_GRADIENT_PIXELS 0x4000U
#define SF_OPACITY_MASK 0xFF000000U

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

// SpriteKeyFlags
#define SKF_NO_DEPTH_TEST 0x1U
#define SKF_USE_ARRAY_TEXTURE 0x2U
#define SKF_USE_PALETTE 0x4U
#define SKF_NO_TRANSFORM 0x8U
#define SKF_ZERO_OPAQUE 0x10U
#define SKF_CLAMP_EDGES 0x20U

layout(set = 0, binding = 0) uniform _Shared {
    float uTime;
    uint uEngineFlags;
    float uPaletteBlend;
    int uPaletteFrame;
};
layout(set = 0, binding = 1) uniform texture2D uDayPalette; //!
layout(set = 0, binding = 2) uniform texture2D uNightPalette; //!
layout(set = 0, binding = 3) uniform sampler uPaletteSampler; //!

layout(set = 1, binding = 0) uniform _Camera {
    mat4 uProjection;
    mat4 uView;
    vec3 uWorldSpacePosition;
    uint _globalInfo_pad1;
    vec2 uCameraLookDirection;
    vec2 uResolution;
};

layout(set = 2, binding = 0) uniform texture2D uSprite; //!
layout(set = 2, binding = 1) uniform texture2DArray uSpriteArray; //!
layout(set = 2, binding = 2) uniform sampler uSpriteSampler; //!
layout(set = 2, binding = 3) uniform _Uniform {
    vec2 uTexSize;
    uint uFlags;
    uint _pad1;
};

// UAlbion.Core.Veldrid.Sprites.BlendedSpriteIntermediateData
layout(location = 0) in flat uint iFlags;
layout(location = 1) in vec2 iTexPosition1;
layout(location = 2) in flat float iLayer1;
layout(location = 3) in vec4 iUvClamp1;
layout(location = 4) in vec2 iTexPosition2;
layout(location = 5) in flat float iLayer2;
layout(location = 6) in vec4 iUvClamp2;
layout(location = 7) in vec2 iNormCoords;
layout(location = 8) in vec3 iWorldPosition;

// UAlbion.Core.Veldrid.SimpleFramebuffer
layout(location = 0) out vec4 oColor;

