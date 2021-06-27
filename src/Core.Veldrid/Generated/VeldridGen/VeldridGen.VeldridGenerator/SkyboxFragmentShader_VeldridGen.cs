using Veldrid;
namespace UAlbion.Core.Veldrid
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

layout(set = 0, binding = 0) uniform sampler uSampler; //!
layout(set = 0, binding = 1) uniform texture2D uTexture; //!

// UAlbion.Core.Veldrid.SkyboxIntermediate
layout(location = 0) in vec2 ioTexPosition;
layout(location = 1) in vec2 ioNormCoords;
layout(location = 2) in vec3 ioWorldPosition;

// UAlbion.Core.Veldrid.Sprites.ColorOnly
layout(location = 0) out vec4 oColor;

");
        }
    }
}
