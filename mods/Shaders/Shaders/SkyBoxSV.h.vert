// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
// !!! This file was auto-generated using VeldridGen. It should not be edited by hand. !!!
// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
//!#version 450 // Comments with //! are just for the VS GLSL plugin
//!#extension GL_KHR_vulkan_glsl: enable

layout(set = 2, binding = 2) uniform _Uniform {
    float uYaw;
    float uPitch;
    float uVisibleProportion;
    uint _pad1;
};

// UAlbion.Core.Veldrid.Vertex2DTextured
layout(location = 0) in vec2 iPosition;
layout(location = 1) in vec2 iTexCoords;

// UAlbion.Core.Veldrid.Skybox.SkyboxIntermediate
layout(location = 0) out vec2 oTexPosition;
layout(location = 1) out vec2 oNormCoords;
layout(location = 2) out vec3 oWorldPosition;

