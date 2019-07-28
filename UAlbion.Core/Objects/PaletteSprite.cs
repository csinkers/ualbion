using System;
using System.Numerics;
using Veldrid;
using Veldrid.Utilities;

namespace UAlbion.Core.Objects
{
    public class PaletteSprite : Renderable
    {
        static readonly ushort[] s_indices = {0, 1, 2, 2, 1, 3};
        static readonly VertexPosition[] s_vertices =
        {
            // Top
            new VertexPosition(new Vector3(-1.0f, 1.0f, 0.0f)),
            new VertexPosition(new Vector3( 1.0f, 1.0f, 0.0f)),
            new VertexPosition(new Vector3(-1.0f,-1.0f, 0.0f)),
            new VertexPosition(new Vector3( 1.0f,-1.0f, 0.0f)),
        };

        readonly MeshData _mesh;
        readonly EightBitTexture _texture;
        readonly DisposeCollector _disposeCollector = new DisposeCollector();

        // Context objects
        DeviceBuffer _vb;
        DeviceBuffer _ib;
        Pipeline _pipeline;
        ResourceSet _resourceSet;
        ResourceLayout _layout;

        public PaletteSprite(int width, int height, int frames, byte[] pixelData)
        {
            int mipmapLevels = (int) Math.Log(Math.Max(width, height), 2.0);
            _texture = new EightBitTexture((uint)width, (uint)height, (uint)mipmapLevels, (uint)frames, pixelData);
            _mesh = PrimitiveShapes.Plane(width, height, 1);
        }

        public override void UpdatePerFrameResources(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
        }

        public override void Render(GraphicsDevice gd, CommandList cl, SceneContext sc, RenderPasses renderPass)
        {
            cl.SetVertexBuffer(0, _vb);
            cl.SetIndexBuffer(_ib, IndexFormat.UInt16);
            cl.SetPipeline(_pipeline);
            cl.SetGraphicsResourceSet(0, _resourceSet);
            float depth = gd.IsDepthRangeZeroToOne ? 0 : 1;
            cl.SetViewport(0, new Viewport(0, 0, sc.MainSceneColorTexture.Width, sc.MainSceneColorTexture.Height, depth, depth));
            cl.DrawIndexed((uint)s_indices.Length, 1, 0, 0, 0);

            cl.SetViewport(0, new Viewport(0, 0, sc.MainSceneColorTexture.Width, sc.MainSceneColorTexture.Height, 0, 1));
        }

        public override void CreateDeviceObjects(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
            ResourceFactory factory = gd.ResourceFactory;

            _vb = factory.CreateBuffer(new BufferDescription(s_vertices.SizeInBytes(), BufferUsage.VertexBuffer));
            cl.UpdateBuffer(_vb, 0, s_vertices);

            _ib = factory.CreateBuffer(new BufferDescription(s_indices.SizeInBytes(), BufferUsage.IndexBuffer));
            cl.UpdateBuffer(_ib, 0, s_indices);

            Texture deviceTexture = _texture.CreateDeviceTexture(gd, factory, TextureUsage.Sampled);
            TextureView textureView = factory.CreateTextureView(new TextureViewDescription(deviceTexture));

            VertexLayoutDescription[] vertexLayouts = {
                new VertexLayoutDescription( new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3))
            };

            (Shader vs, Shader fs) = StaticResourceCache.GetShaders(gd, gd.ResourceFactory, "PaletteSprite");

            _layout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("Projection", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("View", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("CubeTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("CubeSampler", ResourceKind.Sampler, ShaderStages.Fragment)));

            GraphicsPipelineDescription pd = new GraphicsPipelineDescription(
                BlendStateDescription.SingleAlphaBlend,
                gd.IsDepthRangeZeroToOne ? DepthStencilStateDescription.DepthOnlyGreaterEqual : DepthStencilStateDescription.DepthOnlyLessEqual,
                new RasterizerStateDescription(FaceCullMode.None, PolygonFillMode.Solid, FrontFace.Clockwise, true, true),
                PrimitiveTopology.TriangleList,
                new ShaderSetDescription(vertexLayouts, new[] { vs, fs }, ShaderHelper.GetSpecializations(gd)),
                new[] { _layout },
                sc.MainSceneFramebuffer.OutputDescription);

            _pipeline = factory.CreateGraphicsPipeline(ref pd);

            _resourceSet = factory.CreateResourceSet(new ResourceSetDescription(
                _layout,
                sc.ProjectionMatrixBuffer,
                sc.ViewMatrixBuffer,
                textureView,
                gd.PointSampler));

            _disposeCollector.Add(_vb, _ib, deviceTexture, textureView, _layout, _pipeline, _resourceSet, vs, fs);
        }

        public override void DestroyDeviceObjects()
        {
            _disposeCollector.DisposeAll();
        }

        public override RenderOrderKey GetRenderOrderKey(Vector3 cameraPosition)
        {
            return new RenderOrderKey(ulong.MaxValue);
        }

        //public override BoundingBox BoundingBox { get; }
        public override RenderPasses RenderPasses => RenderPasses.Standard;
    }
}
