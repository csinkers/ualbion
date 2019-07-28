using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid;
using Veldrid.ImageSharp;
using Veldrid.Utilities;

namespace UAlbion.Core.Objects
{
    public class Sprite : Renderable
    {
        static Vertex2DTextured Vertex(float x, float y, float u, float v)
        {
            return new Vertex2DTextured(new Vector2(x, y), new Vector2(u, v));
        }

        static readonly ushort[] s_indices = {0, 1, 2, 2, 1, 3};
        public Vector2 Position  { get; }
        public Vector2 Size { get; }
        //static readonly Vector2 Position = new Vector2(0.0f, 0.0f);
        //static readonly Vector2 Size = new Vector2(360.0f, 192.0f);

        static readonly Vector2 s_texPos = new Vector2(0.0f, 0.0f);
        static readonly Vector2 s_texSize = new Vector2(1.0f, 1.0f);
        readonly Vertex2DTextured[] _vertices;

        //readonly MeshData _mesh;
        readonly Image<Rgba32> _texture;
        readonly DisposeCollector _disposeCollector = new DisposeCollector();

        // Context objects
        DeviceBuffer _vb;
        DeviceBuffer _ib;
        Pipeline _pipeline;
        ResourceSet _resourceSet;
        ResourceLayout _layout;

        public Sprite(Image<Rgba32> image,  Vector2 position, Vector2 size)
        {
            _texture = image;
            Position = position;
            Size = size;
            _vertices = new[]
            {
                Vertex(Position.X, Position.Y + Size.Y, s_texPos.X, s_texPos.Y),
                Vertex(Position.X + Size.X, Position.Y + Size.Y, s_texPos.X + s_texSize.X, s_texPos.Y),
                Vertex(Position.X, Position.Y, s_texPos.X, s_texPos.Y + s_texSize.Y),
                Vertex(Position.X + Size.X, Position.Y, s_texPos.X + s_texSize.X, s_texPos.Y + s_texSize.Y)
            };
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

            _vb = factory.CreateBuffer(new BufferDescription(_vertices.SizeInBytes(), BufferUsage.VertexBuffer));
            cl.UpdateBuffer(_vb, 0, _vertices);

            _ib = factory.CreateBuffer(new BufferDescription(s_indices.SizeInBytes(), BufferUsage.IndexBuffer));
            cl.UpdateBuffer(_ib, 0, s_indices);

            ImageSharpTexture imageSharpTexture = new ImageSharpTexture(_texture, false);
            Texture deviceTexture = imageSharpTexture.CreateDeviceTexture(gd, factory);
            TextureView textureView = factory.CreateTextureView(new TextureViewDescription(deviceTexture));

            VertexLayoutDescription[] vertexLayouts = {
                new VertexLayoutDescription(
                    new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                    new VertexElementDescription("Texture", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2))
            };

            (Shader vs, Shader fs) = StaticResourceCache.GetShaders(gd, gd.ResourceFactory, "Sprite");

            _layout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("SpriteTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("SpriteSampler", ResourceKind.Sampler, ShaderStages.Fragment)));

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
