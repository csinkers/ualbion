using System;
using UAlbion.Api.Eventing;
using UAlbion.Core.Visual;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid.Sprites;

public sealed class SpriteRenderer : Component, IRenderer, IDisposable
{
    readonly MultiBuffer<Vertex2DTextured> _vertexBuffer;
    readonly MultiBuffer<ushort> _indexBuffer;
    readonly SpritePipeline _pipeline;
    readonly SpritePipeline _noCullPipeline;

    static readonly ushort[] Indices = { 0, 1, 2, 2, 1, 3 };
    static readonly Vertex2DTextured[] Vertices =
    {
        new(-0.5f, 0.0f, 0.0f, 0.0f), new(0.5f, 0.0f, 1.0f, 0.0f),
        new(-0.5f, 1.0f, 0.0f, 1.0f), new(0.5f, 1.0f, 1.0f, 1.0f),
    };

    public Type[] HandledTypes { get; } = { typeof(VeldridSpriteBatch<SpriteInfo, GpuSpriteInstanceData>) };

    public SpriteRenderer(IFramebufferHolder framebuffer)
    {
        _vertexBuffer = new MultiBuffer<Vertex2DTextured>(Vertices, BufferUsage.VertexBuffer, "SpriteVertexBuffer");
        _indexBuffer = new MultiBuffer<ushort>(Indices, BufferUsage.IndexBuffer, "SpriteIndexBuffer");
        _pipeline = BuildPipeline(
            new DepthStencilStateDescription
            {
                DepthTestEnabled = true,
                DepthWriteEnabled = true,
                DepthComparison = ComparisonKind.LessEqual
            }, framebuffer);

        _noCullPipeline = BuildPipeline(
            new DepthStencilStateDescription
            {
                DepthTestEnabled = true,
                DepthWriteEnabled = true,
                DepthComparison = ComparisonKind.Always
            }, framebuffer);
        AttachChild(_vertexBuffer);
        AttachChild(_indexBuffer);
        AttachChild(_pipeline);
        AttachChild(_noCullPipeline);
    }

    static SpritePipeline BuildPipeline(DepthStencilStateDescription depthMode, IFramebufferHolder framebuffer)
    {
        return new()
        {
            Name = "P:Sprite",
            AlphaBlend = BlendStateDescription.SingleAlphaBlend,
            CullMode = FaceCullMode.None,
            DepthStencilMode = depthMode,
            FillMode = PolygonFillMode.Solid,
            Framebuffer = framebuffer,
            Topology = PrimitiveTopology.TriangleList,
            UseDepthTest = true,
            UseScissorTest = true,
            Winding = FrontFace.Clockwise,
        };
    }

    public void Render(IRenderable renderable, CommonSet commonSet, IFramebufferHolder framebuffer, CommandList cl,
        GraphicsDevice device)
    {
        if (cl == null) throw new ArgumentNullException(nameof(cl));
        if (commonSet == null) throw new ArgumentNullException(nameof(commonSet));
        if (framebuffer == null) throw new ArgumentNullException(nameof(framebuffer));
        if (renderable is not VeldridSpriteBatch<SpriteInfo, GpuSpriteInstanceData> batch)
            throw new ArgumentException($"{GetType().Name} was passed renderable of unexpected type {renderable?.GetType().Name ?? "null"}", nameof(renderable));

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

        cl.SetGraphicsResourceSet(0, commonSet.ResourceSet);
        cl.SetGraphicsResourceSet(1, batch.SpriteResources.ResourceSet);
        cl.SetVertexBuffer(0, _vertexBuffer.DeviceBuffer);
        cl.SetVertexBuffer(1, batch.Instances.DeviceBuffer);
        cl.SetIndexBuffer(_indexBuffer.DeviceBuffer, IndexFormat.UInt16);
        cl.SetFramebuffer(framebuffer.Framebuffer);

        cl.DrawIndexed((uint)Indices.Length, (uint)batch.ActiveInstances, 0, 0, 0);

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