using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;
using UAlbion.Api.Eventing;
using UAlbion.Core.Visual;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid.Skybox;

public sealed class SkyboxRenderer : Component, IRenderer, IDisposable
{
    static readonly ushort[] Indices = { 0, 1, 2, 2, 1, 3 };
    static readonly Vertex2DTextured[] Vertices =
    {
        new (-1.0f, -1.0f, 0.0f, 0.0f), new (1.0f, -1.0f, 1.0f, 0.0f),
        new (-1.0f, 1.0f, 0.0f, 1.0f), new (1.0f, 1.0f, 1.0f, 1.0f),
    };

    readonly SkyboxPipeline _pipeline;
    readonly MultiBuffer<Vertex2DTextured> _vertexBuffer;
    readonly MultiBuffer<ushort> _indexBuffer;

    public Type[] HandledTypes { get; } = { typeof(SkyboxRenderable) };

    static SkyboxPipeline BuildPipeline(in OutputDescription outputFormat) => new()
    {
        Name = "P_Skybox",
        AlphaBlend = BlendStateDescription.SingleDisabled,
        CullMode = FaceCullMode.None,
        FillMode = PolygonFillMode.Solid,
        OutputDescription = outputFormat,
        DepthStencilMode = DepthStencilStateDescription.Disabled,
        Winding = FrontFace.Clockwise,
        UseDepthTest = false,
        UseScissorTest = true,
        Topology = PrimitiveTopology.TriangleList,
    };

    public SkyboxRenderer(in OutputDescription outputFormat)
    {
        _vertexBuffer = new MultiBuffer<Vertex2DTextured>(Vertices, BufferUsage.VertexBuffer, "SpriteVertexBuffer");
        _indexBuffer = new MultiBuffer<ushort>(Indices, BufferUsage.IndexBuffer, "SpriteIndexBuffer");
        _pipeline = BuildPipeline(outputFormat);
        AttachChild(_vertexBuffer);
        AttachChild(_indexBuffer);
        AttachChild(_pipeline);
    }

    public void Render(IRenderable renderable, CommandList cl, GraphicsDevice device, IResourceSetHolder set1, IResourceSetHolder set2)
    {
        var globalSet = (GlobalSet)set1 ?? throw new ArgumentNullException(nameof(set1));
        var renderPassSet = (MainPassSet)set2 ?? throw new ArgumentNullException(nameof(set2));

        ArgumentNullException.ThrowIfNull(cl);
        if (renderable is not SkyboxRenderable skybox)
            throw new ArgumentException($"{GetType().Name} was passed renderable of unexpected type {renderable?.GetType().Name ?? "null"}", nameof(renderable));

        cl.PushDebugGroup(skybox.Name);

        cl.SetPipeline(_pipeline.Pipeline);

        cl.SetGraphicsResourceSet(0, globalSet.ResourceSet);
        cl.SetGraphicsResourceSet(1, renderPassSet.ResourceSet);
        cl.SetGraphicsResourceSet(2, skybox.ResourceSet.ResourceSet);
        cl.SetVertexBuffer(0, _vertexBuffer.DeviceBuffer);
        cl.SetIndexBuffer(_indexBuffer.DeviceBuffer, IndexFormat.UInt16);

        cl.DrawIndexed((uint)Indices.Length, 1, 0, 0, 0);
        cl.PopDebugGroup();
    }

    public void Dispose()
    {
        _vertexBuffer?.Dispose();
        _indexBuffer?.Dispose();
        _pipeline?.Dispose();
    }
}

[VertexShader(typeof(SkyboxVertexShader))]
[FragmentShader(typeof(SkyboxFragmentShader))]
sealed partial class SkyboxPipeline : PipelineHolder { }

sealed partial class SkyboxResourceSet : ResourceSetHolder
{
    [Sampler("uSampler", ShaderStages.Fragment)] ISamplerHolder _sampler;
    [Texture("uTexture", ShaderStages.Fragment)] ITextureHolder _texture;
    [UniformBuffer("_Uniform", ShaderStages.Vertex)] IBufferHolder<SkyboxUniformInfo> _uniform;
}

[Name("SkyBoxSV.vert")]
[Input(0, typeof(Vertex2DTextured))]
[ResourceSet(2, typeof(SkyboxResourceSet))]
[Output(0, typeof(SkyboxIntermediate))]
[SuppressMessage("Microsoft.Naming", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Used for code generation")]
internal sealed partial class SkyboxVertexShader : IVertexShader { }

[Name("SkyBoxSF.frag")]
[Input(0, typeof(SkyboxIntermediate))]
[ResourceSet(0, typeof(GlobalSet))]
[ResourceSet(1, typeof(MainPassSet))]
[ResourceSet(2, typeof(SkyboxResourceSet))]
[Output(0, typeof(SimpleFramebuffer))]
[SuppressMessage("Microsoft.Naming", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Used for code generation")]
internal sealed partial class SkyboxFragmentShader : IFragmentShader { }

#pragma warning disable 649
[StructLayout(LayoutKind.Sequential)]
struct SkyboxUniformInfo // Length must be multiple of 16
{
    [Uniform("uYaw")] public float uYaw; // 4
    [Uniform("uPitch")] public float uPitch;  // 8
    [Uniform("uVisibleProportion")] public float uVisibleProportion;  // 12
    [Uniform("_pad1")] readonly uint _pad1;   // 16
}

[SuppressMessage("Microsoft.Naming", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Used for code generation")]
partial struct SkyboxIntermediate : IVertexFormat
{
    [Vertex("TexPosition")] public Vector2 TextureCoordinates;
    [Vertex("NormCoords")] public Vector2 NormalisedSpriteCoordinates;
    [Vertex("WorldPosition")] public Vector3 WorldPosition;
}
#pragma warning restore 649