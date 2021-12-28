using System;
using UAlbion.Core.Visual;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid.Etm;

public sealed class EtmRenderer : Component, IRenderer, IDisposable
{
    readonly MultiBuffer<Vertex3DTextured> _vertexBuffer;
    readonly MultiBuffer<ushort> _indexBuffer;
    readonly EtmPipeline _normalPipeline;
    readonly EtmPipeline _nonCullingPipeline;

    public EtmRenderer(IFramebufferHolder framebuffer)
    {
        _vertexBuffer = new MultiBuffer<Vertex3DTextured>(Cube.Vertices, BufferUsage.VertexBuffer) { Name = "TileMapVertexBuffer"};
        _indexBuffer = new MultiBuffer<ushort>(Cube.Indices, BufferUsage.IndexBuffer) { Name = "TileMapIndexBuffer"};
        _normalPipeline = BuildPipeline("P_TileMapRenderer", FaceCullMode.Back, framebuffer);
        _nonCullingPipeline = BuildPipeline("P_TileMapRendererNoCull", FaceCullMode.None, framebuffer);
        AttachChild(_vertexBuffer);
        AttachChild(_indexBuffer);
        AttachChild(_normalPipeline);
        AttachChild(_nonCullingPipeline);
    }

    static EtmPipeline BuildPipeline(string name, FaceCullMode cullMode, IFramebufferHolder framebuffer)
        => new()
        {
            AlphaBlend = BlendStateDescription.SingleAlphaBlend,
            CullMode = cullMode,
            DepthStencilMode = DepthStencilStateDescription.DepthOnlyLessEqual,
            FillMode = PolygonFillMode.Solid,
            Framebuffer = framebuffer,
            Name = name,
            Topology = PrimitiveTopology.TriangleList,
            UseDepthTest = true,
            UseScissorTest = false,
            Winding = FrontFace.CounterClockwise,
        };

    public void Render(IRenderable renderable, CommonSet commonSet, IFramebufferHolder framebuffer, CommandList cl,
        GraphicsDevice device)
    {
        if (cl == null) throw new ArgumentNullException(nameof(cl));
        if (commonSet == null) throw new ArgumentNullException(nameof(commonSet));
        if (framebuffer == null) throw new ArgumentNullException(nameof(framebuffer));
        if (renderable is not EtmWindow window)
            throw new ArgumentException($"{GetType().Name} was passed renderable of unexpected type {renderable?.GetType().Name ?? "null"}", nameof(renderable));

        var tilemap = window.Tilemap;

        cl.PushDebugGroup($"Tiles3D:{tilemap.Name}");

        cl.SetPipeline(tilemap.RendererId == DungeonTilemapPipeline.NoCulling 
            ? _nonCullingPipeline.Pipeline 
            : _normalPipeline.Pipeline);

        cl.SetGraphicsResourceSet(0, tilemap.ResourceSet.ResourceSet);
        cl.SetGraphicsResourceSet(1, commonSet.ResourceSet);
        cl.SetVertexBuffer(0, _vertexBuffer.DeviceBuffer);
        cl.SetVertexBuffer(1, tilemap.TileBuffer);
        cl.SetIndexBuffer(_indexBuffer.DeviceBuffer, IndexFormat.UInt16);
        cl.SetFramebuffer(framebuffer.Framebuffer);

        cl.DrawIndexed((uint)Cube.Indices.Length, (uint)tilemap.Tiles.Length, 0, 0, 0);
        cl.PopDebugGroup();
    }

    public void Dispose()
    {
        _vertexBuffer?.Dispose();
        _indexBuffer?.Dispose();
        _normalPipeline?.Dispose();
        _nonCullingPipeline?.Dispose();
    }
}