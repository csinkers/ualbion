using System;
using System.Collections.Generic;
using UAlbion.Api;
using UAlbion.Core.Events;
using Veldrid;
using Veldrid.Utilities;

namespace UAlbion.Core.Veldrid.Visual
{
    public class ScreenDuplicator : Component, IRenderable, IRenderer
    {
        const string VertexShaderName = "ScreenDuplicatorSV.vert";
        const string FragmentShaderName = "ScreenDuplicatorSF.frag";
        static readonly HandlerSet Handlers = new HandlerSet(
            H<ScreenDuplicator, RenderEvent>((x, e) => e.Add(x)));

        static readonly ushort[] QuadIndices = { 0, 1, 2, 0, 2, 3 };
        DisposeCollector _disposeCollector;
        Pipeline _pipeline;
        DeviceBuffer _ib;
        DeviceBuffer _vb;
        Shader[] _shaders;

        public ScreenDuplicator() : base(Handlers) { }
        public string Name => "ScreenDuplicator";
        public bool CanRender(Type renderable) => renderable == typeof(ScreenDuplicator);
        public RenderPasses RenderPasses => RenderPasses.Duplicator;
        public DrawLayer RenderOrder => DrawLayer.MaxLayer;
        public int PipelineId => 1;

        public void CreateDeviceObjects(IRendererContext context)
        {
            var c = (VeldridRendererContext)context;
            var cl = c.CommandList;
            var gd = c.GraphicsDevice;
            var sc = c.SceneContext;

            DisposeCollectorResourceFactory factory = new DisposeCollectorResourceFactory(gd.ResourceFactory);
            _disposeCollector = factory.DisposeCollector;

            ResourceLayout resourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("SourceTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("SourceSampler", ResourceKind.Sampler, ShaderStages.Fragment)));

            var shaderCache = Resolve<IShaderCache>();
            _shaders = shaderCache.GetShaderPair(gd.ResourceFactory,
                VertexShaderName,
                FragmentShaderName,
                shaderCache.GetGlsl(VertexShaderName),
                shaderCache.GetGlsl(FragmentShaderName));

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
                    _shaders,
                    ShaderHelper.GetSpecializations(gd)),
                new[] { resourceLayout },
                sc.DuplicatorFramebuffer.OutputDescription);
            _pipeline = factory.CreateGraphicsPipeline(ref pd);
            _pipeline.Name = "P_ScreenDuplicator";

            float[] verts = CoreUtil.GetFullScreenQuadVerts(gd.IsClipSpaceYInverted);

            _vb = factory.CreateBuffer(new BufferDescription(verts.SizeInBytes() * sizeof(float), BufferUsage.VertexBuffer));
            cl.UpdateBuffer(_vb, 0, verts);

            _ib = factory.CreateBuffer(
                new BufferDescription((uint)QuadIndices.Length * sizeof(ushort), BufferUsage.IndexBuffer));
            cl.UpdateBuffer(_ib, 0, QuadIndices);
        }

        public void Render(IRendererContext context, RenderPasses renderPass, IRenderable r)
        {
            var c = (VeldridRendererContext)context;
            var cl = c.CommandList;
            var sc = c.SceneContext;

            cl.SetPipeline(_pipeline);
            cl.SetGraphicsResourceSet(0, sc.MainSceneViewResourceSet);
            cl.SetVertexBuffer(0, _vb);
            cl.SetIndexBuffer(_ib, IndexFormat.UInt16);
            cl.DrawIndexed(6, 1, 0, 0, 0);
        }

        public IEnumerable<IRenderable> UpdatePerFrameResources(IRendererContext context, IEnumerable<IRenderable> renderables) => renderables;

        public void DestroyDeviceObjects()
        {
            if (_shaders != null)
                foreach (var shader in _shaders)
                    shader.Dispose();
            _shaders = null;

            _disposeCollector?.DisposeAll();
        }
        public void Dispose() => DestroyDeviceObjects();
    }
}
