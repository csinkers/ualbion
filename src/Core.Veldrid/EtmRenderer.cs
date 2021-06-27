using System;
using System.Numerics;
using UAlbion.Core.Veldrid.Sprites;
using UAlbion.Core.Visual;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid
{
    [VertexShader(typeof(EtmVertexShader))]
    [FragmentShader(typeof(EtmFragmentShader))]
    public partial class EtmPipeline : PipelineHolder
    {
    }

    partial class EtmIntermediate : IVertexFormat
    {
#pragma warning disable 649
        [Vertex("TexCoords")] public Vector2 TextureCordinates;
        [Vertex("Textures", Flat = true)] public uint Textures;
        [Vertex("Flags", Flat = true)] public DungeonTileFlags Flags;
#pragma warning restore 649
    }

    [Name("ExtrudedTileMapSV.vert")]
    [Input(0, typeof(Vertex3DTextured))]
    [Input(1, typeof(DungeonTile), InstanceStep = 1)]
    [ResourceSet(0, typeof(EtmSet))]
    [ResourceSet(1, typeof(CommonSet))]
    [Output(0, typeof(EtmIntermediate))]
    public partial class EtmVertexShader : IVertexShader { }

    [Name( "ExtrudedTileMapSF.frag")]
    [Input(0, typeof(EtmIntermediate))]
    [ResourceSet(0, typeof(EtmSet))]
    [ResourceSet(1, typeof(CommonSet))]
    public partial class EtmFragmentShader : IFragmentShader { }

    public sealed class EtmRenderer : Component, IDisposable
    {
        readonly MultiBuffer<Vertex3DTextured> _vertexBuffer;
        readonly MultiBuffer<ushort> _indexBuffer;
        readonly EtmPipeline _normalPipeline;
        readonly EtmPipeline _nonCullingPipeline;

        public EtmRenderer(IFramebufferHolder framebuffer)
        {
            _vertexBuffer = AttachChild(new MultiBuffer<Vertex3DTextured>(Cube.Vertices, BufferUsage.VertexBuffer) { Name = "TileMapVertexBuffer"});
            _indexBuffer = AttachChild(new MultiBuffer<ushort>(Cube.Indices, BufferUsage.IndexBuffer) { Name = "TileMapIndexBuffer"});
            _normalPipeline = AttachChild(BuildPipeline("P_TileMapRenderer", FaceCullMode.Back, framebuffer));
            _nonCullingPipeline = AttachChild(BuildPipeline("P_TileMapRendererNoCull", FaceCullMode.None, framebuffer));
        }

        static EtmPipeline BuildPipeline(string name, FaceCullMode cullMode, IFramebufferHolder framebuffer) => new()
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

        public void Render(CommandList cl, DungeonTilemap tilemap, CommonSet commonSet, IFramebufferHolder framebuffer)
        {
            if (cl == null) throw new ArgumentNullException(nameof(cl));
            if (tilemap == null) throw new ArgumentNullException(nameof(tilemap));
            if (commonSet == null) throw new ArgumentNullException(nameof(commonSet));
            if (framebuffer == null) throw new ArgumentNullException(nameof(framebuffer));

            cl.PushDebugGroup($"Tiles3D:{tilemap.Name}:{tilemap.RenderOrder}");

            cl.SetPipeline(tilemap.RendererId == DungeonTilemapPipeline.NoCulling ? _nonCullingPipeline.Pipeline : _normalPipeline.Pipeline);
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
}
