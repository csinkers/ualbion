using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core.Events;
using Veldrid;
using Veldrid.Utilities;

namespace UAlbion.Core.Visual
{
    public class ScreenDuplicator : Component, IRenderable, IRenderer
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<ScreenDuplicator, RenderEvent>((x, e) => e.Add(x)));

        static readonly ushort[] s_quadIndices = { 0, 1, 2, 0, 2, 3 };
        DisposeCollector _disposeCollector;
        Pipeline _pipeline;
        DeviceBuffer _ib;
        DeviceBuffer _vb;

        public ScreenDuplicator() : base(Handlers) { }
        public string Name => "ScreenDuplicator";
        public RenderPasses RenderPasses => RenderPasses.Duplicator;
        public int RenderOrder => int.MaxValue;
        public Type Renderer => typeof(ScreenDuplicator);
        public BoundingBox? Extents => null;
        public Matrix4x4 Transform => Matrix4x4.Identity;
        public event EventHandler ExtentsChanged;

        public void CreateDeviceObjects(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
            DisposeCollectorResourceFactory factory = new DisposeCollectorResourceFactory(gd.ResourceFactory);
            _disposeCollector = factory.DisposeCollector;

            ResourceLayout resourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("vdspv_0_0", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("SourceSampler", ResourceKind.Sampler, ShaderStages.Fragment)));

            (Shader vs, Shader fs) = StaticResourceCache.GetShaders(gd, gd.ResourceFactory, "ScreenDuplicator");

            GraphicsPipelineDescription pd = new GraphicsPipelineDescription(
                new BlendStateDescription(
                    RgbaFloat.Black,
                    BlendAttachmentDescription.OverrideBlend,
                    BlendAttachmentDescription.OverrideBlend),
                gd.IsDepthRangeZeroToOne ? DepthStencilStateDescription.DepthOnlyGreaterEqual : DepthStencilStateDescription.DepthOnlyLessEqual,
                RasterizerStateDescription.Default,
                PrimitiveTopology.TriangleList,
                new ShaderSetDescription(
                    new[]
                    {
                        new VertexLayoutDescription(
                            new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                            new VertexElementDescription("TexCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2))
                    },
                    new[] { vs, fs, },
                    ShaderHelper.GetSpecializations(gd)),
                new ResourceLayout[] { resourceLayout },
                sc.DuplicatorFramebuffer.OutputDescription);
            _pipeline = factory.CreateGraphicsPipeline(ref pd);
            _pipeline.Name = "P_ScreenDuplicator";

            float[] verts = Util.GetFullScreenQuadVerts(gd);

            _vb = factory.CreateBuffer(new BufferDescription(verts.SizeInBytes() * sizeof(float), BufferUsage.VertexBuffer));
            cl.UpdateBuffer(_vb, 0, verts);

            _ib = factory.CreateBuffer(
                new BufferDescription((uint)s_quadIndices.Length * sizeof(ushort), BufferUsage.IndexBuffer));
            cl.UpdateBuffer(_ib, 0, s_quadIndices);
        }

        public void Render(GraphicsDevice gd, CommandList cl, SceneContext sc, RenderPasses renderPass, IRenderable r)
        {
            cl.SetPipeline(_pipeline);
            cl.SetGraphicsResourceSet(0, sc.MainSceneViewResourceSet);
            cl.SetVertexBuffer(0, _vb);
            cl.SetIndexBuffer(_ib, IndexFormat.UInt16);
            cl.DrawIndexed(6, 1, 0, 0, 0);
        }

        public IEnumerable<IRenderable> UpdatePerFrameResources(GraphicsDevice gd, CommandList cl, SceneContext sc, IEnumerable<IRenderable> renderables) => renderables;
        public void DestroyDeviceObjects() { _disposeCollector.DisposeAll(); }
        public void Dispose() { DestroyDeviceObjects(); }
    }
}
