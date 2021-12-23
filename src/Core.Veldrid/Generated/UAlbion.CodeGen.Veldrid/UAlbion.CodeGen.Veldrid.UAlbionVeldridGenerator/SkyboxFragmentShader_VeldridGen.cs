using Veldrid;
namespace UAlbion.Core.Veldrid.Skybox
{
    internal partial class SkyboxFragmentShader
    {
        public static (string, string) ShaderSource()
        {
            return ("SkyBoxSF.h.frag", @"// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
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

layout(set = 0, binding = 0) uniform sampler uSampler; //!
layout(set = 0, binding = 1) uniform texture2D uTexture; //!

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

// UAlbion.Core.Veldrid.Skybox.SkyboxIntermediate
layout(location = 0) in vec2 iTexPosition;
layout(location = 1) in vec2 iNormCoords;
layout(location = 2) in vec3 iWorldPosition;

// UAlbion.Core.Veldrid.SimpleFramebuffer
layout(location = 0) out vec4 oColor;

");
        }
    }
}
