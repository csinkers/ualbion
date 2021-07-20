using Veldrid;
namespace UAlbion.Core.Veldrid
{
    internal partial class FullscreenQuadFragmentShader
    {
        public static (string, string) ShaderSource()
        {
            return ("FullscreenQuadSF.h.frag", @"// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
// !!! This file was auto-generated using VeldridGen. It should not be edited by hand. !!!
// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
//!#version 450 // Comments with //! are just for the VS GLSL plugin
//!#extension GL_KHR_vulkan_glsl: enable

layout(set = 0, binding = 0) uniform sampler uSampler; //!
layout(set = 0, binding = 1) uniform texture2D uTexture; //!

// UAlbion.Core.Veldrid.FullscreenQuadIntermediate
layout(location = 0) in vec2 iNormCoords;

// UAlbion.Core.Veldrid.SimpleFramebuffer
layout(location = 0) out vec4 oColor;

");
        }
    }
}
