using System;
using System.Numerics;
using Veldrid;
using Veldrid.SPIRV;
using Veldrid.Utilities;

namespace UAlbion.Core.Objects
{
    public class SpriteRenderer : IRenderer
    {
        public interface ISprite : IRenderable
        {
            ITexture Texture { get; }
            Vector2 Position { get; }
            Vector2 Size { get; }
            Vector2 TexPosition { get; }
            Vector2 TexSize { get; }
            SpriteFlags Flags { get; }
        }

        static class Shader
        {
            public static Vertex2DTextured Vertex(float x, float y, float u, float v) => new Vertex2DTextured(x, y, u, v);

            public static readonly VertexLayoutDescription[] VertexLayouts = {
                new VertexLayoutDescription(
                    VertexLayoutH.Texture2D("Position"),
                    VertexLayoutH.Texture2D("TexCoords")) };

            public static readonly ResourceLayoutDescription PerSpriteLayoutDescription = new ResourceLayoutDescription(
                ResourceLayoutH.Uniform("Projection"),
                ResourceLayoutH.Uniform("View"),
                ResourceLayoutH.Uniform("Flags"),
                ResourceLayoutH.Sampler("SpriteSampler"),
                ResourceLayoutH.Texture("SpriteTexture"),
                ResourceLayoutH.Texture("PaletteView"));

            public const string VertexShader = @"
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

            public const string FragmentShader = @"
            #version 450

            layout(set = 0, binding = 2) uniform SpriteFlags { mat4 _Flags; };
            layout(set = 0, binding = 3) uniform sampler SpriteSampler;
            layout(set = 0, binding = 4) uniform texture2D SpriteTexture;
            layout(set = 0, binding = 5) uniform texture2D PaletteView;

            layout(location = 0) in vec2 fsin_0;
            layout(location = 0) out vec4 OutputColor;

            void main()
            {
                if (_Flags[0][0] > 0)
                {
                    float redChannel = texture(sampler2D(SpriteTexture, SpriteSampler), fsin_0)[0];
                    float index = 255.0f * redChannel;
                    vec4 color = texture(sampler2D(PaletteView, SpriteSampler), vec2(redChannel, 0.0f));
                    OutputColor = vec4(0.0f, 1.0f, 0.0f, 1.0f);
                    //OutputColor = (vec4(index / 255.0f, index / 255.0f, index / 255.0f, 1.0f) + color) / 2;
                }
                else
                {
                    OutputColor = vec4(0.0f, 0.0f, 1.0f, 1.0f);
                    //OutputColor = texture(sampler2D(SpriteTexture, SpriteSampler), fsin_0);
                }
            }";
        }

        static readonly ushort[] Indices = { 0, 1, 2, 2, 1, 3 };
        readonly DisposeCollector _disposeCollector = new DisposeCollector();
        readonly ITextureManager _textureManager;

        // Context objects
        DeviceBuffer _vb;
        DeviceBuffer _ib;
        DeviceBuffer _flagsBuffer;
        Pipeline _pipeline;
        ResourceLayout _perSpriteResourceLayout;

        public SpriteRenderer(ITextureManager textureManager)
        {
            _textureManager = textureManager ?? throw new ArgumentNullException(nameof(textureManager));
        }

        public RenderPasses RenderPasses => RenderPasses.Standard;
        public Sprite CreateSprite() { return new Sprite(); } // TODO: Sprite pooling / reuse?

        static Vertex2DTextured[] BuildVertices(ISprite sprite)
        {
            return new[]
            {
                Shader.Vertex(sprite.Position.X, sprite.Position.Y, sprite.TexPosition.X, sprite.TexPosition.Y),
                Shader.Vertex(sprite.Position.X + sprite.Size.X, sprite.Position.Y, sprite.TexPosition.X + sprite.TexSize.X, sprite.TexPosition.Y),
                Shader.Vertex(sprite.Position.X, sprite.Position.Y + sprite.Size.Y, sprite.TexPosition.X, sprite.TexPosition.Y + sprite.TexSize.Y),
                Shader.Vertex(sprite.Position.X + sprite.Size.X, sprite.Position.Y + sprite.Size.Y, sprite.TexPosition.X + sprite.TexSize.X, sprite.TexPosition.Y + sprite.TexSize.Y)
            };
        }

        public void CreateDeviceObjects(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
            ResourceFactory factory = gd.ResourceFactory;

            _vb = factory.CreateBuffer(new BufferDescription(new Vertex2DTextured[4].SizeInBytes(), BufferUsage.VertexBuffer));
            _ib = factory.CreateBuffer(new BufferDescription(Indices.SizeInBytes(), BufferUsage.IndexBuffer));
            _flagsBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            _vb.Name = "SpriteVertexBuffer";
            _ib.Name = "SpriteIndexBuffer";
            _flagsBuffer.Name = "SpriteFlagsBuffer";
            cl.UpdateBuffer(_ib, 0, Indices);

            var shaderSet = new ShaderSetDescription(new[] { Vertex2DTextured.VertexLayout },
                factory.CreateFromSpirv(ShaderHelper.Vertex(Shader.VertexShader), ShaderHelper.Fragment(Shader.FragmentShader)));

            _perSpriteResourceLayout = factory.CreateResourceLayout(Shader.PerSpriteLayoutDescription);

            var depthStencilMode = gd.IsDepthRangeZeroToOne
                    ? DepthStencilStateDescription.DepthOnlyGreaterEqual
                    : DepthStencilStateDescription.DepthOnlyLessEqual;

            var rasterizerMode = new RasterizerStateDescription(
                FaceCullMode.None, PolygonFillMode.Solid, FrontFace.Clockwise,
                true, true);

            var pd = new GraphicsPipelineDescription(
                BlendStateDescription.SingleAlphaBlend,
                depthStencilMode,
                rasterizerMode,
                PrimitiveTopology.TriangleList,
                new ShaderSetDescription(Shader.VertexLayouts, shaderSet.Shaders, ShaderHelper.GetSpecializations(gd)),
                new[] { _perSpriteResourceLayout },
                sc.MainSceneFramebuffer.OutputDescription);

            _pipeline = factory.CreateGraphicsPipeline(ref pd);

            _disposeCollector.Add(_vb, _ib, _perSpriteResourceLayout, _pipeline, _flagsBuffer);
        }

        public void UpdatePerFrameResources(GraphicsDevice gd, CommandList cl, SceneContext sc, IRenderable renderable)
        {
            var sprite = (ISprite)renderable;
            _textureManager.PrepareTexture(sprite.Texture, gd);
        }

        public void Render(GraphicsDevice gd, CommandList cl, SceneContext sc, RenderPasses renderPass, IRenderable renderable)
        {
            var sprite = (ISprite)renderable;
            var flags = sprite.Flags;

            //if (sprite.Texture.Format == PixelFormat.R8_UNorm)
                flags |= SpriteFlags.UsePalette;

            var projection = (sprite.Flags & SpriteFlags.NoTransform) != 0 ? sc.IdentityMatrixBuffer : sc.ProjectionMatrixBuffer;
            var view = (sprite.Flags & SpriteFlags.NoTransform) != 0 ? sc.IdentityMatrixBuffer : sc.ViewMatrixBuffer;
            TextureView textureView = _textureManager.GetTexture(sprite.Texture);
            gd.UpdateBuffer(_flagsBuffer, 0, new float[] { (float)flags, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 });

            var resourceSet = gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(_perSpriteResourceLayout,
                projection, view, _flagsBuffer, gd.PointSampler, textureView, sc.PaletteView));

            cl.UpdateBuffer(_vb, 0, BuildVertices(sprite));
            cl.SetVertexBuffer(0, _vb);
            cl.SetIndexBuffer(_ib, IndexFormat.UInt16);
            cl.SetPipeline(_pipeline);
            cl.SetGraphicsResourceSet(0, resourceSet);
            float depth = gd.IsDepthRangeZeroToOne ? 0 : 1;
            cl.SetViewport(0, new Viewport(0, 0, sc.MainSceneColorTexture.Width, sc.MainSceneColorTexture.Height, depth, depth));
            cl.DrawIndexed((uint)Indices.Length, 1, 0, 0, 0);
            cl.SetViewport(0, new Viewport(0, 0, sc.MainSceneColorTexture.Width, sc.MainSceneColorTexture.Height, 0, 1));
        }

        public void DestroyDeviceObjects() { _disposeCollector.DisposeAll(); }
        public void Dispose() { DestroyDeviceObjects(); }
    }
}

