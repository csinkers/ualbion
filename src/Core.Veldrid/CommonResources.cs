using System.Numerics;
using System.Runtime.InteropServices;
using VeldridGen.Interfaces;
using Veldrid;

#pragma warning disable CA1051 // Do not declare visible instance fields
#pragma warning disable CA1815 // Struct should override equals
#pragma warning disable CS0169
#pragma warning disable IDE0051
namespace UAlbion.Core.Veldrid;

public sealed partial class GlobalSet : ResourceSetHolder
{
    [UniformBuffer("_Shared")]                SingleBuffer<GlobalInfo> _global; 
    [Texture("uDayPalette", ShaderStages.Fragment)]     ITextureHolder _dayPalette;
    [Texture("uNightPalette", ShaderStages.Fragment)]   ITextureHolder _nightPalette;
    [Sampler("uPaletteSampler", ShaderStages.Fragment)] ISamplerHolder _sampler;
}

[StructLayout(LayoutKind.Sequential)]
public struct GlobalInfo : IUniformFormat
{
    [Uniform("uTime")] public float Time;
    [Uniform("uEngineFlags", EnumPrefix = "EF")] public EngineFlags EngineFlags;
    [Uniform("uPaletteBlend")] public float PaletteBlend;
    [Uniform("uPaletteFrame")] public int PaletteFrame;
}

public sealed partial class MainPassSet : ResourceSetHolder
{
    [UniformBuffer("_Camera")] SingleBuffer<CameraUniform> _camera; 
}

#pragma warning disable CA1823 // Avoid unused private fields
public struct CameraUniform : IUniformFormat
{
    [Uniform("uProjection")] public Matrix4x4 Projection;
    [Uniform("uView")] public Matrix4x4 View;

    [Uniform("uWorldSpacePosition")] public Vector3 WorldSpacePosition;
    [Uniform("_globalInfo_pad1")] readonly uint _padding1;

    [Uniform("uCameraLookDirection")] public Vector2 CameraDirection;
    [Uniform("uResolution")] public Vector2 Resolution;
}

#pragma warning restore CA1823 // Avoid unused private fields
#pragma warning restore IDE0051
#pragma warning restore CS0169
#pragma warning restore CA1051 // Do not declare visible instance fields
