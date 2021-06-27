using System;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid.Sprites
{
    public class SpriteRenderer : Component, IDisposable
    {
        readonly MultiBuffer<Vertex2DTextured> _vertexBuffer;
        readonly MultiBuffer<ushort> _indexBuffer;
        readonly SpritePipeline _pipeline;

        static readonly ushort[] Indices = { 0, 1, 2, 2, 1, 3 };
        static readonly Vertex2DTextured[] Vertices =
        {
            new(-0.5f, 0.0f, 0.0f, 0.0f), new(0.5f, 0.0f, 1.0f, 0.0f),
            new(-0.5f, 1.0f, 0.0f, 1.0f), new(0.5f, 1.0f, 1.0f, 1.0f),
        };

        public SpriteRenderer(IFramebufferHolder framebuffer)
        {
            _vertexBuffer = AttachChild(new MultiBuffer<Vertex2DTextured>(Vertices, BufferUsage.VertexBuffer, "SpriteVertexBuffer"));
            _indexBuffer = AttachChild(new MultiBuffer<ushort>(Indices, BufferUsage.IndexBuffer, "SpriteIndexBuffer"));
            _pipeline = AttachChild(new SpritePipeline
            {
                Name = "P:Sprite",
                AlphaBlend = BlendStateDescription.SingleAlphaBlend,
                CullMode = FaceCullMode.None,
                DepthStencilMode = DepthStencilStateDescription.DepthOnlyLessEqual,
                FillMode = PolygonFillMode.Solid,
                Framebuffer = framebuffer,
                Topology = PrimitiveTopology.TriangleList,
                UseDepthTest = true,
                UseScissorTest = true,
                Winding = FrontFace.Clockwise,
            });
        }

        public void Render(CommandList cl, SpriteBatch batch, CommonSet commonSet, IFramebufferHolder framebuffer)
        {
            if (cl == null) throw new ArgumentNullException(nameof(cl));
            if (batch == null) throw new ArgumentNullException(nameof(batch));
            if (commonSet == null) throw new ArgumentNullException(nameof(commonSet));
            if (framebuffer == null) throw new ArgumentNullException(nameof(framebuffer));

            cl.PushDebugGroup(batch.Name);
            if (batch.Key.ScissorRegion.HasValue)
            {
                var rect = batch.Key.ScissorRegion.Value;
                cl.SetScissorRect(0, (uint)rect.X, (uint)rect.Y, (uint)rect.Width, (uint)rect.Height);
            }

            cl.SetPipeline(_pipeline.Pipeline);
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
            _vertexBuffer?.Dispose();
            _indexBuffer?.Dispose();
            _pipeline?.Dispose();
        }
    }
}