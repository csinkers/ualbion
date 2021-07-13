// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
// !!! This file was auto-generated using VeldridGen. It should not be edited by hand. !!!
// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
//!#version 450 // Comments with //! are just for the VS GLSL plugin
//!#extension GL_KHR_vulkan_glsl: enable

// DungeonTileFlags
#define TF_TEXTURE_TYPE_FLOOR 0x0U
#define TF_TEXTURE_TYPE_CEILING 0x1U
#define TF_TEXTURE_TYPE_WALL 0x2U
#define TF_TEXTURE_TYPE_MASK 0x3U
#define TF_USE_PALETTE 0x4U
#define TF_HIGHLIGHT 0x8U
#define TF_RED_TINT 0x10U
#define TF_GREEN_TINT 0x20U
#define TF_BLUE_TINT 0x40U
#define TF_TRANSPARENT 0x80U
#define TF_NO_TEXTURE 0x100U

// EngineFlags
#define EF_SHOW_BOUNDING_BOXES 0x1U
#define EF_SHOW_CAMERA_POSITION 0x2U
#define EF_FLIP_DEPTH_RANGE 0x4U
#define EF_FLIP_YSPACE 0x8U
#define EF_VSYNC 0x10U
#define EF_HIGHLIGHT_SELECTION 0x20U
#define EF_USE_CYLINDRICAL_BILLBOARDS 0x40U
#define EF_RENDER_DEPTH 0x80U

layout(set = 0, binding = 1) uniform texture2DArray DayFloors; //!
layout(set = 0, binding = 2) uniform texture2DArray DayWalls; //!
layout(set = 0, binding = 3) uniform texture2DArray NightFloors; //!
layout(set = 0, binding = 4) uniform texture2DArray NightWalls; //!
layout(set = 0, binding = 5) uniform sampler TextureSampler; //!

layout(set = 1, binding = 0) uniform _Shared {
    vec3 uWorldSpacePosition;
    uint _globalInfo_pad1;
    vec2 uCameraLookDirection;
    vec2 uResolution;
    float uTime;
    uint uEngineFlags;
    float uPaletteBlend;
    float uSpecial1;
};
layout(set = 1, binding = 3) uniform texture2D uPalette; //!

// UAlbion.Core.Veldrid.Etm.EtmIntermediate
layout(location = 0) in vec2 iTexCoords;
layout(location = 1) in flat uint iTextures;
layout(location = 2) in flat uint iFlags;

// UAlbion.Core.Veldrid.Sprites.ColorOnly
layout(location = 0) out vec4 oColor;

