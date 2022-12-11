using Veldrid;
namespace UAlbion.Core.Veldrid.Meshes
{
    internal partial class MeshFragmentShader
    {
        public static (string, string) ShaderSource()
        {
            return ("MeshSF.h.frag", @"// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
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

layout(set = 0, binding = 0) uniform texture2D Diffuse; //!
layout(set = 0, binding = 1) uniform sampler Sampler; //!
layout(set = 0, binding = 2) uniform MeshUniform {
    vec3 uAmbRef;
    float uOpacity;
    vec3 uDiffRef;
    float uSharpness;
    vec3 uSpecRef;
    float uSpecExp;
    vec3 uEmmCoeff;
    float uOpticalDensity;
    vec3 uTransFilter;
    int uIllumModel;
};

layout(set = 1, binding = 0) uniform _Shared {
    vec3 uWorldSpacePosition;
    uint _globalInfo_pad1;
    vec2 uCameraLookDirection;
    vec2 uResolution;
    float uTime;
    uint uEngineFlags;
    float uPaletteBlend;
    int uPaletteFrame;
};
layout(set = 1, binding = 3) uniform texture2D uDayPalette; //!
layout(set = 1, binding = 4) uniform texture2D uNightPalette; //!
layout(set = 1, binding = 5) uniform sampler uPaletteSampler; //!

// UAlbion.Core.Veldrid.Meshes.MeshIntermediate
layout(location = 0) in vec2 iTexCoords;

// UAlbion.Core.Veldrid.SimpleFramebuffer
layout(location = 0) out vec4 oColor;

");
        }
    }
}
