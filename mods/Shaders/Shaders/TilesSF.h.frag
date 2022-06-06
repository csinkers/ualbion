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

// GpuTilesetFlags
#define TSF_USE_ARRAY 0x1U
#define TSF_USE_PALETTE 0x2U
#define TSF_USE_BLEND 0x4U

// GpuTileFlags
#define TF_BOUNCY 0x1U
#define TF_NO_DRAW 0x2U
#define TF_USE_UNDERLAY 0x4U

// GpuTileLayerFlags
#define TLF_DRAW_UNDERLAY 0x1U
#define TLF_DRAW_OVERLAY 0x2U
#define TLF_OPAQUE_UNDERLAY 0x4U

struct GpuTileData
{
    uint Layer;
    uint Type;
    uint FrameCount;
    uint Flags;
    uint DayImage;
    uint NightImage;
    uint Unk7;
    uint PalFrames;
};

struct GpuTextureRegion
{
    vec4 Offset;
};

struct GpuMapTile
{
    uint Tile;
};

layout(set = 0, binding = 0) uniform _Shared {
    vec3 uWorldSpacePosition;
    uint _globalInfo_pad1;
    vec2 uCameraLookDirection;
    vec2 uResolution;
    float uTime;
    uint uEngineFlags;
    float uPaletteBlend;
    int uPaletteFrame;
};
layout(set = 0, binding = 3) uniform texture2D uDayPalette; //!
layout(set = 0, binding = 4) uniform texture2D uNightPalette; //!
layout(set = 0, binding = 5) uniform sampler uPaletteSampler; //!

layout(set = 1, binding = 0) uniform texture2D uTile; //!
layout(set = 1, binding = 1) uniform texture2DArray uTileArray; //!
layout(set = 1, binding = 2) uniform sampler uTileSampler; //!
layout(set = 1, binding = 3) uniform _SetUniform {
    vec2 uTileWorldSize;
    vec2 uTileUvSize;
    uint uTilesetFlags;
    uint uPad1;
    vec2 uPad2;
};
layout(set = 1, binding = 4, std430) readonly buffer TilesBuffer { GpuTileData Tiles[]; };
layout(set = 1, binding = 5, std430) readonly buffer RegionsBuffer { GpuTextureRegion Regions[]; };

layout(set = 2, binding = 0) uniform _LayerUniform {
    uint uMapWidth;
    uint uMapHeight;
    int uFrame;
    uint uLayerFlags;
};
layout(set = 2, binding = 1, std430) readonly buffer MapBuffer { GpuMapTile Map[]; };

// UAlbion.Core.Veldrid.Sprites.TileIntermediateData
layout(location = 0) in vec4 iWorldPosition;
layout(location = 1) in vec2 iTilePosition;

// UAlbion.Core.Veldrid.SimpleFramebuffer
layout(location = 0) out vec4 oColor;

