using System.Numerics;
using System.Runtime.InteropServices;
using VeldridGen.Interfaces;
using Veldrid;

#pragma warning disable CA1051 // Do not declare visible instance fields
namespace UAlbion.Core.Veldrid
{
    public sealed partial class CommonSet : ResourceSetHolder
    {
        [Resource("_Shared")]                          SingleBuffer<GlobalInfo>       _globalInfo; 
        [Resource("_Projection", ShaderStages.Vertex)] SingleBuffer<ProjectionMatrix> _projection; 
        [Resource("_View",       ShaderStages.Vertex)] SingleBuffer<ViewMatrix>       _view; 
        [Resource("uPalette",    ShaderStages.Fragment)] Texture2DHolder              _palette;
    }

    [StructLayout(LayoutKind.Sequential)]
    public partial struct GlobalInfo : IUniformFormat
    {
        [Uniform("uWorldSpacePosition")] public Vector3 WorldSpacePosition;
        [Uniform("_globalInfo_pad1")] readonly uint _padding1;

        [Uniform("uCameraLookDirection")] public Vector3 CameraDirection;
        [Uniform("_globalInfo_pad2")] readonly uint _padding2;

        [Uniform("uResolution")] public Vector2 Resolution;
        [Uniform("uTime")] public float Time;
        [Uniform("uEngineFlags", EnumPrefix = "EF")] public EngineFlags EngineFlags;

        [Uniform("uPaletteBlend")] public float PaletteBlend;
        [Uniform("uSpecial1")] public float Special1;
        [Uniform("uSpecial2")] public float Special2;
        [Uniform("_globalInfo_pad5")] readonly uint _padding5;
    }

    public partial struct ProjectionMatrix : IUniformFormat
    {
        public ProjectionMatrix(Matrix4x4 matrix) => Matrix = matrix;
        [Uniform("uProjection")] public Matrix4x4 Matrix { get; }
    }

    public partial struct ViewMatrix : IUniformFormat
    {
        public ViewMatrix(Matrix4x4 matrix) => Matrix = matrix;
        [Uniform("uView")] public Matrix4x4 Matrix { get; }
    }
}
#pragma warning restore CA1051 // Do not declare visible instance fields
