using Veldrid;
namespace UAlbion.Core.Veldrid.Meshes
{
    internal partial class MeshVertexShader
    {
        public static (string, string) ShaderSource()
        {
            return ("MeshSV.h.vert", @"// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
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
layout(set = 1, binding = 1) uniform _Projection {
    mat4 uProjection;
};
layout(set = 1, binding = 2) uniform _View {
    mat4 uView;
};

// UAlbion.Core.Veldrid.Meshes.MeshVertex
layout(location = 0) in vec3 iPosition;
layout(location = 1) in vec3 iNormal;
layout(location = 2) in vec2 iTexCoords;

// UAlbion.Core.Veldrid.Meshes.GpuMeshInstanceData
layout(location = 3) in vec3 iInstancePos;
layout(location = 4) in vec3 iInstanceScale;

// UAlbion.Core.Veldrid.Meshes.MeshIntermediate
layout(location = 0) out vec2 oTexCoords;

");
        }
    }
}
