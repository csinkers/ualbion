using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Core.Visual;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid.Sprites;

public sealed class BlendedSpriteRenderer : Component, IRenderer, IDisposable
{
    readonly MultiBuffer<Vertex2DTextured> _vertexBuffer;
    readonly MultiBuffer<ushort> _indexBuffer;
    readonly BlendedSpritePipeline _pipeline;
    readonly BlendedSpritePipeline _noCullPipeline;

    static readonly ushort[] Indices = [0, 1, 2, 2, 1, 3];
    static readonly Vertex2DTextured[] Vertices =
    [
        new(-0.5f, 0.0f, 0.0f, 0.0f), new(0.5f, 0.0f, 1.0f, 0.0f),
        new(-0.5f, 1.0f, 0.0f, 1.0f), new(0.5f, 1.0f, 1.0f, 1.0f)
    ];

    public Type[] HandledTypes { get; } = [typeof(VeldridSpriteBatch<BlendedSpriteInfo, GpuBlendedSpriteInstanceData>)];

    public BlendedSpriteRenderer(in OutputDescription outputFormat)
    {
        _vertexBuffer = new MultiBuffer<Vertex2DTextured>(Vertices, BufferUsage.VertexBuffer, "BSpriteVBuffer");
        _indexBuffer = new MultiBuffer<ushort>(Indices, BufferUsage.IndexBuffer, "BSpriteIBuffer");
        _pipeline = BuildPipeline(
            new DepthStencilStateDescription
            {
                DepthTestEnabled = true,
                DepthWriteEnabled = true,
                DepthComparison = ComparisonKind.LessEqual
            }, outputFormat);

        _noCullPipeline = BuildPipeline(
            new DepthStencilStateDescription
            {
                DepthTestEnabled = true,
                DepthWriteEnabled = true,
                DepthComparison = ComparisonKind.Always
            }, outputFormat);
        AttachChild(_vertexBuffer);
        AttachChild(_indexBuffer);
        AttachChild(_pipeline);
        AttachChild(_noCullPipeline);
    }

    static BlendedSpritePipeline BuildPipeline(DepthStencilStateDescription depthMode, in OutputDescription outputFormat)
    {
        return new()
        {
            Name = "P:Sprite",
            AlphaBlend = BlendStateDescription.SingleAlphaBlend,
            CullMode = FaceCullMode.None,
            DepthStencilMode = depthMode,
            FillMode = PolygonFillMode.Solid,
            OutputDescription = outputFormat,
            Topology = PrimitiveTopology.TriangleList,
            UseDepthTest = true,
            UseScissorTest = true,
            Winding = FrontFace.Clockwise,
        };
    }

    public void Render(IRenderable renderable, CommandList cl, GraphicsDevice device, IResourceSetHolder set1, IResourceSetHolder set2)
    {
        var globalSet = (GlobalSet)set1 ?? throw new ArgumentNullException(nameof(set1));
        var renderPassSet = (MainPassSet)set2 ?? throw new ArgumentNullException(nameof(set2));

        ArgumentNullException.ThrowIfNull(cl);
        if (renderable is not VeldridSpriteBatch<BlendedSpriteInfo, GpuBlendedSpriteInstanceData> batch)
            throw new ArgumentException($"{GetType().Name} was passed renderable of unexpected type {renderable?.GetType().Name ?? "null"}", nameof(renderable));

        batch.ShrinkIfNeeded();

        cl.PushDebugGroup(batch.Name);
        if (batch.Key.ScissorRegion.HasValue)
        {
            var rect = batch.Key.ScissorRegion.Value;
            cl.SetScissorRect(0, (uint)rect.X, (uint)rect.Y, (uint)rect.Width, (uint)rect.Height);
        }

        cl.SetPipeline(
            (batch.Key.Flags & SpriteKeyFlags.NoDepthTest) != 0
                ? _noCullPipeline.Pipeline 
                : _pipeline.Pipeline);

        cl.SetGraphicsResourceSet(0, globalSet.ResourceSet);
        cl.SetGraphicsResourceSet(1, renderPassSet.ResourceSet);
        cl.SetGraphicsResourceSet(2, batch.SpriteResources.ResourceSet);
        cl.SetVertexBuffer(0, _vertexBuffer.DeviceBuffer);
        cl.SetVertexBuffer(1, batch.Instances.DeviceBuffer);
        cl.SetIndexBuffer(_indexBuffer.DeviceBuffer, IndexFormat.UInt16);

        cl.DrawIndexed((uint)Indices.Length, (uint)batch.AssignedCount, 0, 0, 0);

        if (batch.Key.ScissorRegion.HasValue)
            cl.SetFullScissorRect(0);
        cl.PopDebugGroup();
    }

    public void Dispose()
    {
        _vertexBuffer.Dispose();
        _indexBuffer.Dispose();
        _pipeline.Dispose();
        _noCullPipeline.Dispose();
    }
}

[VertexShader(typeof(BlendedSpriteVertexShader))]
[FragmentShader(typeof(BlendedSpriteFragmentShader))]
internal sealed partial class BlendedSpritePipeline : PipelineHolder { }

[Name("BlendedSpriteSF.frag")]
[Input(0, typeof(BlendedSpriteIntermediateData))]
[ResourceSet(0, typeof(GlobalSet))]
[ResourceSet(1, typeof(MainPassSet))]
[ResourceSet(2, typeof(SpriteSet))]
[Output(0, typeof(SimpleFramebuffer))]
[SuppressMessage("Microsoft.Naming", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Used for code generation")]
internal sealed partial class BlendedSpriteFragmentShader : IFragmentShader { }

[Name("BlendedSpriteSV.vert")]
[Input(0, typeof(Vertex2DTextured))]
[Input(1, typeof(GpuBlendedSpriteInstanceData), InstanceStep = 1)]
[ResourceSet(0, typeof(GlobalSet))]
[ResourceSet(1, typeof(MainPassSet))]
[ResourceSet(2, typeof(SpriteSet))]
[Output(0, typeof(BlendedSpriteIntermediateData))]
[SuppressMessage("Microsoft.Naming", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Used for code generation")]
internal sealed partial class BlendedSpriteVertexShader : IVertexShader { }

#pragma warning disable 649 // CS0649 Field is never assigned to, and will always have its default value
internal partial struct BlendedSpriteIntermediateData : IVertexFormat
{
    [Vertex("Flags", Flat = true, EnumPrefix = "SF")] public SpriteFlags Flags;

    [Vertex("TexPosition1")] public Vector2 TexturePosition1;
    [Vertex("Layer1", Flat = true)] public float TextureLayer1;
    [Vertex("UvClamp1")] public Vector4 UvClamp1;

    [Vertex("TexPosition2")] public Vector2 TexturePosition2;
    [Vertex("Layer2", Flat = true)] public float TextureLayer2;
    [Vertex("UvClamp2")] public Vector4 UvClamp2;

    [Vertex("NormCoords")] public Vector2 NormalisedSpriteCoordinates;
    [Vertex("WorldPosition")] public Vector3 WorldPosition;
}

#pragma warning disable CA1051 // Do not declare visible instance fields
internal partial struct GpuBlendedSpriteInstanceData : IVertexFormat
{
    [Vertex("Flags", EnumPrefix = "SF")] public SpriteFlags Flags;
    [Vertex("InstancePos")] public Vector4 Position;
    [Vertex("Size")]        public Vector2 Size;
    [Vertex("TexOffset1")]  public Vector2 TexPosition1; // Normalised texture coordinates
    [Vertex("TexSize1")]    public Vector2 TexSize1; // Normalised texture coordinates
    [Vertex("TexLayer1")]   public uint TexLayer1;
    [Vertex("TexOffset2")]  public Vector2 TexPosition2; // Normalised texture coordinates
    [Vertex("TexSize2")]    public Vector2 TexSize2; // Normalised texture coordinates
    [Vertex("TexLayer2")]   public uint TexLayer2;
}
#pragma warning restore CA1051 // Do not declare visible instance fields
#pragma warning restore 649