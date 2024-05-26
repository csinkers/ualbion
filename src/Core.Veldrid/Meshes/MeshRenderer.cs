using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Core.Visual;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid.Meshes;

public sealed class MeshRenderer : Component, IRenderer, IDisposable
{
    readonly MeshPipeline _pipeline;

    public Type[] HandledTypes { get; } = { typeof(MeshBatch) };

    public MeshRenderer(in OutputDescription outputFormat)
    {
        _pipeline = BuildPipeline("P_TileMapRenderer", FaceCullMode.Back, outputFormat);
        AttachChild(_pipeline);
    }

    static MeshPipeline BuildPipeline(string name, FaceCullMode cullMode, in OutputDescription outputFormat)
        => new()
        {
            AlphaBlend = BlendStateDescription.SingleAlphaBlend,
            CullMode = cullMode,
            DepthStencilMode = DepthStencilStateDescription.DepthOnlyLessEqual,
            FillMode = PolygonFillMode.Solid,
            OutputDescription = outputFormat,
            Name = name,
            Topology = PrimitiveTopology.TriangleList,
            UseDepthTest = true,
            UseScissorTest = false,
            Winding = FrontFace.CounterClockwise,
        };

    public void Render(IRenderable renderable, CommandList cl, GraphicsDevice device, IResourceSetHolder set1, IResourceSetHolder set2)
    {
        var globalSet = (GlobalSet)set1 ?? throw new ArgumentNullException(nameof(set1));
        var renderPassSet = (MainPassSet)set2 ?? throw new ArgumentNullException(nameof(set2));

        ArgumentNullException.ThrowIfNull(cl);
        if (renderable is not MeshBatch batch)
            throw new ArgumentException($"{GetType().Name} was passed renderable of unexpected type {renderable?.GetType().Name ?? "null"}", nameof(renderable));

        cl.PushDebugGroup($"Mesh:{batch.Key}");

        cl.SetPipeline(_pipeline.Pipeline);

        cl.SetGraphicsResourceSet(0, globalSet.ResourceSet);
        cl.SetGraphicsResourceSet(1, renderPassSet.ResourceSet);
        cl.SetGraphicsResourceSet(2, batch.ResourceSet.ResourceSet);
        cl.SetVertexBuffer(0, batch.VertexBuffer.DeviceBuffer);
        cl.SetVertexBuffer(1, batch.Instances.DeviceBuffer);
        cl.SetIndexBuffer(batch.IndexBuffer.DeviceBuffer, IndexFormat.UInt16);

        cl.DrawIndexed((uint)batch.IndexBuffer.Count, (uint)batch.Instances.Count, 0, 0, 0);
        cl.PopDebugGroup();
    }

    public void Dispose()
    {
        _pipeline?.Dispose();
    }
}

#pragma warning disable CA1815 // Override equals and operator equals on value types
#pragma warning disable CA1051 // Do not declare visible instance fields
#pragma warning disable 649

[VertexShader(typeof(MeshVertexShader))]
[FragmentShader(typeof(MeshFragmentShader))]
sealed partial class MeshPipeline : PipelineHolder { }

sealed partial class MeshResourceSet : ResourceSetHolder
{
    [Texture("Diffuse", ShaderStages.Fragment)] ITextureHolder _diffuse;
    [Sampler("Sampler", ShaderStages.Fragment)] ISamplerHolder _sampler;
    [UniformBuffer("MeshUniform", ShaderStages.Fragment)] IBufferHolder<MeshUniform> _uniform;
}

[Name("MeshSV.vert")]
[Input(0, typeof(MeshVertex))]
[Input(1, typeof(GpuMeshInstanceData), InstanceStep = 1)]
[ResourceSet(0, typeof(GlobalSet))]
[ResourceSet(1, typeof(MainPassSet))]
[ResourceSet(2, typeof(MeshResourceSet))]
[Output(0, typeof(MeshIntermediate))]
[SuppressMessage("Microsoft.Naming", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Used for code generation")]
sealed partial class MeshVertexShader : IVertexShader { }

[Name("MeshSF.frag")]
[Input(0, typeof(MeshIntermediate))]
[ResourceSet(0, typeof(GlobalSet))]
[ResourceSet(1, typeof(MainPassSet))]
[ResourceSet(2, typeof(MeshResourceSet))]
[Output(0, typeof(SimpleFramebuffer))]
[SuppressMessage("Microsoft.Naming", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Used for code generation")]
sealed partial class MeshFragmentShader : IFragmentShader { }

public partial struct MeshVertex : IVertexFormat
{ // Should match Veldrid.Utilities.VertexPositionNormalTexture exactly as Unsafe.As is used for casting
    [Vertex("Position")]  public readonly Vector3 Position;
    [Vertex("Normal")]    public readonly Vector3 Normal;
    [Vertex("TexCoords")] public readonly Vector2 TextureCoordinates;
}

public partial struct GpuMeshInstanceData : IVertexFormat
{
    [Vertex("InstancePos")] public Vector3 Position;
    [Vertex("InstanceScale")] public Vector3 Scale;
    public override string ToString() => $"Mesh @ {Position}";
}

[SuppressMessage("Microsoft.Naming", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Used for code generation")]
partial struct MeshIntermediate : IVertexFormat
{
    [Vertex("TexCoords")] public Vector2 TextureCordinates;
}

partial struct MeshUniform : IUniformFormat
{
    [Uniform("uAmbRef")]    public Vector3 AmbientReflectivity;
    [Uniform("uOpacity")]   public float Opacity;

    [Uniform("uDiffRef")]   public Vector3 DiffuseReflectivity;
    [Uniform("uSharpness")] public float Sharpness;

    [Uniform("uSpecRef")]   public Vector3 SpecularReflectivity;
    [Uniform("uSpecExp")]   public float SpecularExponent;

    [Uniform("uEmmCoeff")]  public Vector3 EmissiveCoefficient;
    [Uniform("uOpticalDensity")] public float OpticalDensity;

    [Uniform("uTransFilter")] public Vector3 TransmissionFilter;
    [Uniform("uIllumModel")]  public int IlluminationModel;
}

#pragma warning restore 649
#pragma warning restore CA1051
#pragma warning restore CA1815 // Override equals and operator equals on value types