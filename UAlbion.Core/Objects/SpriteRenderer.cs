using System.Numerics;
using Veldrid;
using Veldrid.SPIRV;
using Veldrid.Utilities;

namespace UAlbion.Core.Objects
{
    public interface ITextureId { }

    public class SpriteRenderer
    {
        static readonly ResourceLayoutDescription LayoutDescription = new ResourceLayoutDescription(
            ResourceLayoutH.Uniform("Projection"),
            ResourceLayoutH.Uniform("View"),
            ResourceLayoutH.Texture("SpriteTexture"),
            ResourceLayoutH.Sampler("SpriteSampler"));

        static Vertex2DTextured Vertex(float x, float y, float u, float v) => new Vertex2DTextured(new Vector2(x, y), new Vector2(u, v));
        static readonly VertexLayoutDescription[] VertexLayouts = { new VertexLayoutDescription(
            VertexLayoutH.Position2D("Position"),
            VertexLayoutH.Texture2D("TexCoords")) };

        const string VertexShader = @"
            #version 450

            layout(set = 0, binding = 0) uniform Projection { mat4 _Proj; };
            layout(set = 0, binding = 1) uniform View { mat4 _View; };

            layout(location = 0) in vec2 Position;
            layout(location = 1) in vec2 TexCoords;
            layout(location = 0) out vec2 fsin_0;

            void main()
            {
                fsin_0 = TexCoords;
                gl_Position = _Proj * _View * vec4(Position.x, Position.y, 0, 1);
            }";

        const string FragmentShader = @"
            #version 450

            layout(set = 0, binding = 2) uniform texture2D SpriteTexture;
            layout(set = 0, binding = 3) uniform sampler SpriteSampler;

            layout(location = 0) in vec2 fsin_0;
            layout(location = 0) out vec4 OutputColor;

            void main()
            {
                OutputColor = texture(sampler2D(SpriteTexture, SpriteSampler), fsin_0);
            }";

        readonly ushort[] _indices;
        readonly DisposeCollector _disposeCollector = new DisposeCollector();

        // Context objects
        DeviceBuffer _vb;
        DeviceBuffer _ib;
        Pipeline _pipeline;
        public ResourceLayout ResourceLayout { get; private set; }

        public SpriteRenderer()
        {
            _indices = new ushort[] { 0, 1, 2, 2, 1, 3 };
        }

        static Vertex2DTextured[] BuildVertices(Sprite sprite)
        {
            return new[]
            {
                Vertex(sprite.Position.X, sprite.Position.Y, sprite.TexPosition.X, sprite.TexPosition.Y),
                Vertex(sprite.Position.X + sprite.Size.X, sprite.Position.Y, sprite.TexPosition.X + sprite.TexSize.X, sprite.TexPosition.Y),
                Vertex(sprite.Position.X, sprite.Position.Y + sprite.Size.Y, sprite.TexPosition.X, sprite.TexPosition.Y + sprite.TexSize.Y),
                Vertex(sprite.Position.X + sprite.Size.X, sprite.Position.Y + sprite.Size.Y, sprite.TexPosition.X + sprite.TexSize.X, sprite.TexPosition.Y + sprite.TexSize.Y)
            };
        }

        public void Render(GraphicsDevice gd, CommandList cl, SceneContext sc, RenderPasses renderPass, Sprite sprite)
        {
            cl.UpdateBuffer(_vb, 0, BuildVertices(sprite));

            cl.SetVertexBuffer(0, _vb);
            cl.SetIndexBuffer(_ib, IndexFormat.UInt16);
            cl.SetPipeline(_pipeline);
            cl.SetGraphicsResourceSet(0, sprite.ResourceSet);
            float depth = gd.IsDepthRangeZeroToOne ? 0 : 1;
            cl.SetViewport(0, new Viewport(0, 0, sc.MainSceneColorTexture.Width, sc.MainSceneColorTexture.Height, depth, depth));
            cl.DrawIndexed((uint)_indices.Length, 1, 0, 0, 0);
            cl.SetViewport(0, new Viewport(0, 0, sc.MainSceneColorTexture.Width, sc.MainSceneColorTexture.Height, 0, 1));
        }

        public void CreateDeviceObjects(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
            if (_vb != null)
                return;

            ResourceFactory factory = gd.ResourceFactory;

            _vb = factory.CreateBuffer(new BufferDescription(new Vertex2DTextured[4].SizeInBytes(), BufferUsage.VertexBuffer));
            _ib = factory.CreateBuffer(new BufferDescription(_indices.SizeInBytes(), BufferUsage.IndexBuffer));
            cl.UpdateBuffer(_ib, 0, _indices);

            //ImageSharpTexture imageSharpTexture = new ImageSharpTexture(_texture, false);
            //Texture deviceTexture = imageSharpTexture.CreateDeviceTexture(gd, factory);
            //TextureView textureView = factory.CreateTextureView(new TextureViewDescription(deviceTexture));

            var shaderSet = new ShaderSetDescription(new[] { Vertex2DTextured.VertexLayout },
                factory.CreateFromSpirv(ShaderH.Vertex(VertexShader), ShaderH.Fragment(FragmentShader)));

            ResourceLayout = factory.CreateResourceLayout(LayoutDescription);

            var pd = new GraphicsPipelineDescription(
                BlendStateDescription.SingleAlphaBlend,
                gd.IsDepthRangeZeroToOne ? DepthStencilStateDescription.DepthOnlyGreaterEqual : DepthStencilStateDescription.DepthOnlyLessEqual,
                new RasterizerStateDescription(FaceCullMode.None, PolygonFillMode.Solid, FrontFace.Clockwise, true, true),
                PrimitiveTopology.TriangleList,
                new ShaderSetDescription(VertexLayouts, shaderSet.Shaders, ShaderHelper.GetSpecializations(gd)),
                new[] { ResourceLayout },
                sc.MainSceneFramebuffer.OutputDescription);

            _pipeline = factory.CreateGraphicsPipeline(ref pd);

            _disposeCollector.Add(_vb, _ib, ResourceLayout, _pipeline);
        }

        public void UpdatePerFrameResources(GraphicsDevice gd, CommandList cl, SceneContext sc) { }
        public void DestroyDeviceObjects() { _disposeCollector.DisposeAll(); }
        public RenderOrderKey GetRenderOrderKey(Vector3 cameraPosition) { return new RenderOrderKey(ulong.MaxValue); }
        public RenderPasses RenderPasses => RenderPasses.Standard;
        //public override BoundingBox BoundingBox { get; }
    }
}

