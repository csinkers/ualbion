using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;
using Veldrid.SPIRV;
using Veldrid.Utilities;

namespace UAlbion.Core.Objects
{
    public class SpriteRenderer : IRenderer
    {
        public interface ISprite: IRenderable
        {
            SpriteFlags Flags { get; }
            ITexture Texture { get; }
            Vector2 Position { get; }
            Vector2 Size { get; }
            Vector2 TexPosition { get; }
            Vector2 TexSize { get; }
        }

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
            layout(set = 0, binding = 3) uniform Flags { int _Flags; int _u2; int _u3; int _u4; };

            layout(set = 1, binding = 0) uniform sampler SpriteSampler;
            layout(set = 1, binding = 1) uniform texture2D Palette;

            layout(location = 0) in vec2 fsin_0;
            layout(location = 0) out vec4 OutputColor;

            void main()
            {
                // If palette texture
                if((_Flags & 4) != 0) // TODO: Const
                {
                    int index = int(255.0 * texture(sampler2D(SpriteTexture, SpriteSampler), fsin_0)[0]);
                    vec4 color = texture(sampler2D(Palette, SpriteSampler), vec2(index, 0));
                    OutputColor = color;
                }
                else
                {
                    OutputColor = texture(sampler2D(SpriteTexture, SpriteSampler), fsin_0);
                }
            }";

        readonly ushort[] _indices;
        readonly DisposeCollector _disposeCollector = new DisposeCollector();
        readonly ITextureManager _textureManager;

        // Context objects
        DeviceBuffer _vb;
        DeviceBuffer _ib;
        DeviceBuffer _flagsBuffer;
        Pipeline _pipeline;
        ResourceLayout _perSpriteResourceLayout;
        ResourceLayout _perFrameResourceLayout;

        public SpriteRenderer(ITextureManager textureManager)
        {
            _textureManager = textureManager ?? throw new ArgumentNullException(nameof(textureManager));
            _indices = new ushort[] { 0, 1, 2, 2, 1, 3 };
        }

        public RenderPasses RenderPasses => RenderPasses.Standard;
        public Sprite CreateSprite() { return new Sprite(); }

        static Vertex2DTextured[] BuildVertices(ISprite sprite)
        {
            return new[]
            {
                Vertex(sprite.Position.X, sprite.Position.Y, sprite.TexPosition.X, sprite.TexPosition.Y),
                Vertex(sprite.Position.X + sprite.Size.X, sprite.Position.Y, sprite.TexPosition.X + sprite.TexSize.X, sprite.TexPosition.Y),
                Vertex(sprite.Position.X, sprite.Position.Y + sprite.Size.Y, sprite.TexPosition.X, sprite.TexPosition.Y + sprite.TexSize.Y),
                Vertex(sprite.Position.X + sprite.Size.X, sprite.Position.Y + sprite.Size.Y, sprite.TexPosition.X + sprite.TexSize.X, sprite.TexPosition.Y + sprite.TexSize.Y)
            };
        }

        public void Render(GraphicsDevice gd, CommandList cl, SceneContext sc, RenderPasses renderPass, IRenderable renderable)
        {
            var sprite = (ISprite) renderable;
            var texture = _textureManager.GetTexture(sprite.Texture);
            var flags = sprite.Flags;

            if (sprite.Texture.Format == PixelFormat.R8_UNorm)
                flags |= SpriteFlags.UsePalette;

            var flagsArray = new int[] {(int)flags, 0, 0, 0};
            gd.UpdateBuffer(_flagsBuffer, 0, flagsArray);

            var resourceSet = gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(_perSpriteResourceLayout,
                (sprite.Flags & SpriteFlags.NoTransform) != 0 ? sc.IdentityMatrixBuffer : sc.ProjectionMatrixBuffer,
                (sprite.Flags & SpriteFlags.NoTransform) != 0 ? sc.IdentityMatrixBuffer : sc.ViewMatrixBuffer,
                texture, _flagsBuffer));

            // TODO: Only generate once per frame
            var perFrameResourceSet = gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(_perFrameResourceLayout,
                gd.PointSampler, sc.Palette));

            cl.UpdateBuffer(_vb, 0, BuildVertices(sprite));

            cl.SetVertexBuffer(0, _vb);
            cl.SetIndexBuffer(_ib, IndexFormat.UInt16);
            cl.SetPipeline(_pipeline);
            cl.SetGraphicsResourceSet(0, resourceSet);
            cl.SetGraphicsResourceSet(1, perFrameResourceSet);
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
            _flagsBuffer = factory.CreateBuffer(new BufferDescription(4 * (uint)Marshal.SizeOf<int>(), BufferUsage.UniformBuffer));
            cl.UpdateBuffer(_ib, 0, _indices);

            var shaderSet = new ShaderSetDescription(new[] { Vertex2DTextured.VertexLayout },
                factory.CreateFromSpirv(ShaderHelper.Vertex(VertexShader), ShaderHelper.Fragment(FragmentShader)));

            var PerSpriteLayoutDescription = new ResourceLayoutDescription(
                ResourceLayoutH.Uniform("Projection"),
                ResourceLayoutH.Uniform("View"),
                ResourceLayoutH.Texture("SpriteTexture"),
                ResourceLayoutH.Uniform("Flags"));

            var PerFrameLayoutDescription = new ResourceLayoutDescription(
                ResourceLayoutH.Sampler("SpriteSampler"),
                ResourceLayoutH.Texture("Palette"));

            _perSpriteResourceLayout = factory.CreateResourceLayout(PerSpriteLayoutDescription);
            _perFrameResourceLayout = factory.CreateResourceLayout(PerFrameLayoutDescription);

            var pd = new GraphicsPipelineDescription(
                BlendStateDescription.SingleAlphaBlend,
                gd.IsDepthRangeZeroToOne ? DepthStencilStateDescription.DepthOnlyGreaterEqual : DepthStencilStateDescription.DepthOnlyLessEqual,
                new RasterizerStateDescription(FaceCullMode.None, PolygonFillMode.Solid, FrontFace.Clockwise, true, true),
                PrimitiveTopology.TriangleList,
                new ShaderSetDescription(VertexLayouts, shaderSet.Shaders, ShaderHelper.GetSpecializations(gd)),
                new[] { _perSpriteResourceLayout, _perFrameResourceLayout },
                sc.MainSceneFramebuffer.OutputDescription);

            _pipeline = factory.CreateGraphicsPipeline(ref pd);

            _disposeCollector.Add(_vb, _ib, _perSpriteResourceLayout, _perFrameResourceLayout, _pipeline, _flagsBuffer);
        }

        public void UpdatePerFrameResources(GraphicsDevice gd, CommandList cl, SceneContext sc, IRenderable renderable)
        {
            var sprite = (ISprite) renderable;
            _textureManager.PrepareTexture(sprite.Texture, gd);
            // TODO: Setup device textures
            //ImageSharpTexture imageSharpTexture = new ImageSharpTexture(_texture, false);
            //Texture deviceTexture = imageSharpTexture.CreateDeviceTexture(gd, factory);
            //TextureView textureView = factory.CreateTextureView(new TextureViewDescription(deviceTexture));
        }

        public void DestroyDeviceObjects() { _disposeCollector.DisposeAll(); }
        public void Dispose() { DestroyDeviceObjects(); }
    }
}

