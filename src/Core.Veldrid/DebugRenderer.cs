using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Core.Visual;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid;

public sealed class DebugRenderer : Component, IRenderer, IRenderable, IRenderableSource
{
    public string Name => "Debug";
    public DrawLayer RenderOrder => DrawLayer.Debug;
    public Type[] HandledTypes { get; } = [typeof(DebugRenderer)];

    public void Render(IRenderable renderable, CommandList cl, GraphicsDevice device, IResourceSetHolder set1, IResourceSetHolder set2)
    {
    }

    public void Collect(List<IRenderable> renderables)
    {
        if (DebugUi.Count > 0)
            renderables.Add(this);
    }
}

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
// ReSharper disable UnassignedReadonlyField
[VertexShader(typeof(DebugVertexShader))]
[FragmentShader(typeof(DebugFragmentShader))]
sealed partial class DebugPipeline : PipelineHolder { }
sealed partial class DebugResourceSet : ResourceSetHolder
{
    [UniformBuffer("DebugUniform", ShaderStages.Fragment)] IBufferHolder<DebugUniform> _uniform;
}

[Name("DebugSV.vert")]
[Input(0, typeof(DebugVertex))]
[Input(1, typeof(GpuDebugInstanceData), InstanceStep = 1)]
[ResourceSet(0, typeof(GlobalSet))]
[ResourceSet(1, typeof(MainPassSet))]
[ResourceSet(2, typeof(DebugResourceSet))]
[Output(0, typeof(DebugIntermediate))]
sealed partial class DebugVertexShader : IVertexShader { }

[Name("DebugSF.frag")]
[Input(0, typeof(DebugIntermediate))]
[ResourceSet(0, typeof(GlobalSet))]
[ResourceSet(1, typeof(MainPassSet))]
[ResourceSet(2, typeof(DebugResourceSet))]
[Output(0, typeof(SimpleFramebuffer))]
sealed partial class DebugFragmentShader : IFragmentShader { }

partial struct DebugVertex : IVertexFormat
{
    [Vertex("Position")]  public readonly Vector3 Position;
}

partial struct GpuDebugInstanceData : IVertexFormat // Needs to exactly match DebugUi.Renderable
{
    [Vertex("Type", EnumPrefix = "DRT")] public DebugUi.RenderableType Type;
    [Vertex("CSys", EnumPrefix = "CS")] public CoordinateSystem CoordinateSystem;
    [Vertex("A")] public Vector4 A;
    [Vertex("B")] public Vector4 B;
}

partial struct DebugIntermediate : IVertexFormat
{
    [Vertex("TexCoords")] public Vector2 TextureCordinates;
}

struct DebugUniform : IUniformFormat
{
    [Uniform("uAmbRef")]    public Vector3 AmbientReflectivity;
}