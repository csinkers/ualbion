using System;
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

    static SkyboxPipeline BuildPipeline(IFramebufferHolder framebuffer) => new()
    {
        Name = "P_Skybox",
        AlphaBlend = BlendStateDescription.SingleDisabled,
        CullMode = FaceCullMode.None,
        FillMode = PolygonFillMode.Solid,
        Framebuffer = framebuffer,
        DepthStencilMode = DepthStencilStateDescription.Disabled,
        Winding = FrontFace.Clockwise,
        UseDepthTest = false,
        UseScissorTest = true,
        Topology = PrimitiveTopology.TriangleList,
    };

    public SkyboxRenderer(IFramebufferHolder framebuffer)
    {
        _vertexBuffer = new MultiBuffer<Vertex2DTextured>(Vertices, BufferUsage.VertexBuffer, "SpriteVertexBuffer");
        _indexBuffer = new MultiBuffer<ushort>(Indices, BufferUsage.IndexBuffer, "SpriteIndexBuffer");
        _pipeline = BuildPipeline(framebuffer);
        AttachChild(_vertexBuffer);
        AttachChild(_indexBuffer);
        AttachChild(_pipeline);
    }

    public void Render(IRenderable renderable, CommonSet commonSet, IFramebufferHolder framebuffer, CommandList cl, GraphicsDevice device)
    {
        if (cl == null) throw new ArgumentNullException(nameof(cl));
        if (commonSet == null) throw new ArgumentNullException(nameof(commonSet));
        if (framebuffer == null) throw new ArgumentNullException(nameof(framebuffer));
        if (renderable is not SkyboxRenderable skybox)
            throw new ArgumentException($"{GetType().Name} was passed renderable of unexpected type {renderable?.GetType().Name ?? "null"}", nameof(renderable));

        cl.PushDebugGroup(skybox.Name);

        cl.SetPipeline(_pipeline.Pipeline);
        cl.SetGraphicsResourceSet(0, skybox.ResourceSet.ResourceSet);
        cl.SetGraphicsResourceSet(1, commonSet.ResourceSet);
        cl.SetVertexBuffer(0, _vertexBuffer.DeviceBuffer);
        cl.SetIndexBuffer(_indexBuffer.DeviceBuffer, IndexFormat.UInt16);
        cl.SetFramebuffer(framebuffer.Framebuffer);

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