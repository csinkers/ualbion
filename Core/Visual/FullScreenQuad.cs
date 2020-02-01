using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core.Events;
using Veldrid;
using Veldrid.Utilities;

namespace UAlbion.Core.Visual
{
    public class FullScreenQuad : Component, IRenderer, IRenderable
    {
        const string VertexShaderName = "FullScreenQuadSV.vert";
        const string FragmentShaderName = "FullScreenQuadSF.frag";

        static readonly HandlerSet Handlers = new HandlerSet(
            H<FullScreenQuad, RenderEvent>((x, e) => e.Add(x)));

        static readonly ushort[] QuadIndices = { 0, 1, 2, 0, 2, 3 };
        public string Name => "FullScreenQuad";
        public RenderPasses RenderPasses => RenderPasses.SwapchainOutput;
        public int RenderOrder { get; set; } = int.MaxValue;
        public Type Renderer => typeof(FullScreenQuad);
        public Matrix4x4 Transform => Matrix4x4.Identity;

        DisposeCollector _disposeCollector;
        Pipeline _pipeline;
        DeviceBuffer _ib;
        DeviceBuffer _vb;
        Shader[] _shaders;


        public FullScreenQuad() : base(Handlers) { }

        public void CreateDeviceObjects(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
            var factory = new DisposeCollectorResourceFactory(gd.ResourceFactory);
            _disposeCollector = factory.DisposeCollector;

            var layout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                ResourceLayoutHelper.Texture("SourceTexture"),
                ResourceLayoutHelper.Sampler("SourceSampler")));

            var shaderCache = Resolve<IShaderCache>();
            _shaders = shaderCache.GetShaderPair(gd.ResourceFactory,
                VertexShaderName,
                FragmentShaderName,
                shaderCache.GetGlsl(VertexShaderName),
                shaderCache.GetGlsl(FragmentShaderName));

            var rasterizerState = new RasterizerStateDescription(
                FaceCullMode.Back, PolygonFillMode.Solid, FrontFace.Clockwise, 
                true, false);

            var pd = new GraphicsPipelineDescription(
                new BlendStateDescription(RgbaFloat.Black, BlendAttachmentDescription.OverrideBlend),
                DepthStencilStateDescription.Disabled,
                rasterizerState,
                PrimitiveTopology.TriangleList,
                new ShaderSetDescription(new[] { Vertex2DTextured.VertexLayout }, _shaders, ShaderHelper.GetSpecializations(gd)),
                new[] { layout },
                gd.SwapchainFramebuffer.OutputDescription);
            _pipeline = factory.CreateGraphicsPipeline(ref pd);
            _pipeline.Name = "P_FullScreenQuad";

            float[] verts = Util.GetFullScreenQuadVerts(gd);
            _vb = factory.CreateBuffer(new BufferDescription(verts.SizeInBytes() * sizeof(float), BufferUsage.VertexBuffer));
            cl.UpdateBuffer(_vb, 0, verts);

            _ib = factory.CreateBuffer(new BufferDescription(QuadIndices.SizeInBytes(), BufferUsage.IndexBuffer));
            cl.UpdateBuffer(_ib, 0, QuadIndices);
        }

        public IEnumerable<IRenderable> UpdatePerFrameResources(GraphicsDevice gd, CommandList cl, SceneContext sc, IEnumerable<IRenderable> renderables) => renderables;

        public void Render(GraphicsDevice gd, CommandList cl, SceneContext sc, RenderPasses renderPass, IRenderable r)
        {
            cl.SetPipeline(_pipeline);
            cl.SetGraphicsResourceSet(0, sc.DuplicatorTargetSet0);
            cl.SetVertexBuffer(0, _vb);
            cl.SetIndexBuffer(_ib, IndexFormat.UInt16);
            cl.DrawIndexed(6, 1, 0, 0, 0);
        }

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
