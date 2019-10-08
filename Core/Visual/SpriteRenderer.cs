using System.Collections.Generic;
using System.Linq;
using UAlbion.Core.Textures;
using Veldrid;
using Veldrid.SPIRV;
using Veldrid.Utilities;

namespace UAlbion.Core.Visual
{
    public class SpriteRenderer : Component, IRenderer
    {
        public SpriteRenderer() : base(null) { }

        static class Shader
        {
            // Vertex Layout
            public static readonly VertexLayoutDescription VertexLayout = Vertex2DTextured.VertexLayout;

            // Instance Layout
            public static readonly VertexLayoutDescription InstanceLayout = new VertexLayoutDescription(
                    VertexLayoutHelper.Vector3D("Offset"), VertexLayoutHelper.Vector2D("Size"),
                    VertexLayoutHelper.Vector2D("TexPosition"), VertexLayoutHelper.Vector2D("TexSize"),
                    VertexLayoutHelper.Int("TexLayer"), VertexLayoutHelper.Int("Flags"),
                    VertexLayoutHelper.Float("Rotation")
                    )
            { InstanceStepRate = 1 };

            // Resource Sets
            public static readonly ResourceLayoutDescription PerSpriteLayoutDescription = new ResourceLayoutDescription(
                ResourceLayoutHelper.Uniform("vdspv_0_0"), // Perspective Matrix
                ResourceLayoutHelper.Uniform("vdspv_0_1"),  // View Matrix
                ResourceLayoutHelper.Sampler("vdspv_0_2"),  // SpriteSampler
                ResourceLayoutHelper.Texture("vdspv_0_3"),  // SpriteTexture
                ResourceLayoutHelper.Texture("vdspv_0_4")); // Palette

            public const string VertexShader = @"
            #version 450

            // Resource Sets
            layout(set = 0, binding = 0) uniform _Projection { mat4 Projection; }; // vdspv_0_0
            layout(set = 0, binding = 1) uniform _View { mat4 View; }; // vdspv_0_1

            // Vertex Data
            layout(location = 0) in vec2 _Position;
            layout(location = 1) in vec2 _TexCoords;

            // Instance Data
            layout(location = 2) in vec3 _Offset;
            layout(location = 3) in vec2 _Size;
            layout(location = 4) in vec2 _TexOffset;
            layout(location = 5) in vec2 _TexSize;
            layout(location = 6) in int _TexLayer;
            layout(location = 7) in int _Flags;
            layout(location = 8) in float _Rotation;

            // Outputs to fragment shader
            layout(location = 0) out vec2 fsin_0;     // Texture Coordinates
            layout(location = 1) out flat float fsin_1; // Texture Layer
            layout(location = 2) out flat int fsin_2; /* Flags:
               NoTransform  = 0x1,  Highlight      = 0x2,
               UsePalette   = 0x4,  OnlyEvenFrames = 0x8,
               RedTint      = 0x10,  GreenTint     = 0x20,
               BlueTint     = 0x40,  Transparent   = 0x80 
               FlipVertical = 0x100, FloorTile     = 0x200,
               Billboard    = 0x400, DropShadow    = 0x800 */

            void main()
            {
                mat4 transform = mat4(1.0);
                if((_Flags & 0x200) != 0) {
                    transform = mat4(1, 0,         0, 0,
                                     0, 0,        -1, 0,
                                     0, 1,         0, 0,
                                     0, 0, _Size.y/2, 1) * transform;
                }
                else {
                    float c = cos(_Rotation);
                    float s = sin(_Rotation);
                    transform = mat4(
                        c, 0, s, 0,
                        0, 1, 0, 0,
                       -s, 0, c, 0,
                        0, 0, 0, 1) * transform;
                }

                vec4 worldSpace = transform * vec4((_Position * _Size), 0, 1) + vec4(_Offset, 0);

                if ((_Flags & 1) == 0)
                    gl_Position = Projection * View * worldSpace;
                else
                    gl_Position = worldSpace;

                fsin_0 = _TexCoords * _TexSize + _TexOffset;
                fsin_1 = float(_TexLayer);
                fsin_2 = _Flags;
            }";

            public const string FragmentShader = @"
            #version 450

            // Resource Sets
            layout(set = 0, binding = 2) uniform sampler SpriteSampler;   // vdspv_0_2
            layout(set = 0, binding = 3) uniform texture2DArray SpriteTexture; // vdspv_0_3
            layout(set = 0, binding = 4) uniform texture2D PaletteView;   // vdspv_0_4

            // Inputs from vertex shader
            layout(location = 0) in vec2 fsin_0;       // Texture Coordinates
            layout(location = 1) in flat float fsin_1; // Texture Layer
            layout(location = 2) in flat int fsin_2;   // Flags

            // Fragment shader outputs
            layout(location = 0) out vec4 OutputColor;

            void main()
            {
                vec2 uv = ((fsin_2 & 0x100) != 0) ? vec2(fsin_0.x, 1-fsin_0.y) : fsin_0;

                vec4 color;
                if ((fsin_2 & 4) != 0)
                {
                    float redChannel = texture(sampler2DArray(SpriteTexture, SpriteSampler), vec3(uv, fsin_1))[0];
                    float index = 255.0f * redChannel;
                    color = texture(sampler2D(PaletteView, SpriteSampler), vec2(redChannel, 0.0f));
                    if(index == 0)
                        color = vec4(0.0f, 0.0f, 0.0f, 0.0f);
                }
                else
                {
                    color = texture(sampler2DArray(SpriteTexture, SpriteSampler), vec3(uv, fsin_1));
                }

                if(color.w == 0.0f)
                    discard;

                if((fsin_2 & 0x800) != 0)
                    color = vec4(0.0f, 0.0f, 0.0f, 1.0f);

                if((fsin_2 & 0x02) != 0) color = color * 1.2; // Highlight
                if((fsin_2 & 0x10) != 0) color = vec4(color.x * 1.5f + 0.3f, color.yzw);         // Red tint
                if((fsin_2 & 0x20) != 0) color = vec4(color.x, color.y * 1.5f + 0.3f, color.zw); // Green tint
                if((fsin_2 & 0x40) != 0) color = vec4(color.xy, color.z * 1.5f + 0.f, color.w); // Blue tint
                if((fsin_2 & 0x80) != 0) color = vec4(color.xyz, color.w * 0.5f); // Transparent

                OutputColor = color;
            }";
        }

        static readonly Vertex2DTextured[] Vertices =
        {
            new Vertex2DTextured(-0.5f, 0.0f, 0.0f, 0.0f), new Vertex2DTextured(0.5f, 0.0f, 1.0f, 0.0f),
            new Vertex2DTextured(-0.5f, 1.0f, 0.0f, 1.0f), new Vertex2DTextured(0.5f, 1.0f, 1.0f, 1.0f),
        };
        static readonly ushort[] Indices = { 0, 1, 2, 2, 1, 3 };
        readonly DisposeCollector _disposeCollector = new DisposeCollector();
        readonly IList<DeviceBuffer> _instanceBuffers = new List<DeviceBuffer>();
        readonly IList<ResourceSet> _resourceSets = new List<ResourceSet>();

        // Context objects
        DeviceBuffer _vb;
        DeviceBuffer _ib;
        Pipeline _depthTestPipeline;
        Pipeline _noDepthPipeline;
        ResourceLayout _perSpriteResourceLayout;

        public RenderPasses RenderPasses => RenderPasses.Standard;
        public void CreateDeviceObjects(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
            ResourceFactory factory = gd.ResourceFactory;

            _vb = factory.CreateBuffer(new BufferDescription(Vertices.SizeInBytes(), BufferUsage.VertexBuffer));
            _ib = factory.CreateBuffer(new BufferDescription(Indices.SizeInBytes(), BufferUsage.IndexBuffer));
            _vb.Name = "SpriteVertexBuffer";
            _ib.Name = "SpriteIndexBuffer";
            cl.UpdateBuffer(_vb, 0, Vertices);
            cl.UpdateBuffer(_ib, 0, Indices);

            var shaderSet = new ShaderSetDescription(new[] { Shader.VertexLayout, Shader.InstanceLayout },
                factory.CreateFromSpirv(ShaderHelper.Vertex(Shader.VertexShader), ShaderHelper.Fragment(Shader.FragmentShader)));

            _perSpriteResourceLayout = factory.CreateResourceLayout(Shader.PerSpriteLayoutDescription);

            var depthStencilMode = gd.IsDepthRangeZeroToOne
                    ? DepthStencilStateDescription.DepthOnlyLessEqual
                    : DepthStencilStateDescription.DepthOnlyGreaterEqual;

            var rasterizerMode = new RasterizerStateDescription(
                FaceCullMode.None, PolygonFillMode.Solid, FrontFace.Clockwise,
                true, true);

            var depthPd = new GraphicsPipelineDescription(
                BlendStateDescription.SingleAlphaBlend,
                depthStencilMode,
                rasterizerMode,
                PrimitiveTopology.TriangleList,
                new ShaderSetDescription(new[] { Shader.VertexLayout, Shader.InstanceLayout },
                    shaderSet.Shaders,
                    ShaderHelper.GetSpecializations(gd)),
                new[] { _perSpriteResourceLayout },
                sc.MainSceneFramebuffer.OutputDescription);

            _depthTestPipeline = factory.CreateGraphicsPipeline(ref depthPd);
            _depthTestPipeline.Name = "P_SpriteRendererDT";

            var nonDepthPd = new GraphicsPipelineDescription(
                BlendStateDescription.SingleAlphaBlend,
                DepthStencilStateDescription.Disabled,
                new RasterizerStateDescription(FaceCullMode.None, PolygonFillMode.Solid, 
                    FrontFace.Clockwise, false, false), 
                PrimitiveTopology.TriangleList,
                new ShaderSetDescription(new[] { Shader.VertexLayout, Shader.InstanceLayout },
                    shaderSet.Shaders,
                    ShaderHelper.GetSpecializations(gd)),
                new[] { _perSpriteResourceLayout },
                sc.MainSceneFramebuffer.OutputDescription);

            _noDepthPipeline = factory.CreateGraphicsPipeline(ref nonDepthPd);
            _noDepthPipeline.Name = "P_SpriteRendererNoDepth";

            _disposeCollector.Add(_vb, _ib, _perSpriteResourceLayout, _depthTestPipeline);
        }

        public IEnumerable<IRenderable> UpdatePerFrameResources(GraphicsDevice gd, CommandList cl, SceneContext sc, IEnumerable<IRenderable> renderables)
        {
            ITextureManager textureManager = Exchange.Resolve<ITextureManager>();
            ISpriteResolver spriteResolver = Exchange.Resolve<ISpriteResolver>();

            foreach (var buffer in _instanceBuffers)
                buffer.Dispose();
            _instanceBuffers.Clear();

            foreach (var resourceSet in _resourceSets)
                resourceSet.Dispose();
            _resourceSets.Clear();

            void SetupMultiSpriteResources(MultiSprite multiSprite)
            {
                textureManager?.PrepareTexture(multiSprite.Key.Texture, gd);
                multiSprite.BufferId = _instanceBuffers.Count;
                multiSprite.RotateSprites(sc.Camera.Position);
                var buffer = gd.ResourceFactory.CreateBuffer(new BufferDescription((uint)multiSprite.Instances.Length * SpriteInstanceData.StructSize, BufferUsage.VertexBuffer));
                buffer.Name = $"B_SpriteInst{_instanceBuffers.Count}";
                cl.UpdateBuffer(buffer, 0, multiSprite.Instances);
                _instanceBuffers.Add(buffer);
            }

            var resolved = renderables.OfType<SpriteDefinition>().Select(spriteResolver.Resolve);
            var grouped = resolved.GroupBy(x => x.Item1, x => x.Item2);
            foreach (var group in grouped)
            {
                var multiSprite = new MultiSprite(group.Key, _instanceBuffers.Count, group);
                SetupMultiSpriteResources(multiSprite);
                yield return multiSprite;
            }

            foreach(var multiSprite in renderables.OfType<MultiSprite>())
            {
                SetupMultiSpriteResources(multiSprite);
                yield return multiSprite;
            }
        }

        public void Render(GraphicsDevice gd, CommandList cl, SceneContext sc, RenderPasses renderPass, IRenderable renderable)
        {
            ITextureManager textureManager = Exchange.Resolve<ITextureManager>();
            // float depth = gd.IsDepthRangeZeroToOne ? 0 : 1;
            var sprite = (MultiSprite)renderable;
            cl.PushDebugGroup($"Sprite:{sprite.Key.Texture.Name}:{sprite.Key.RenderOrder}");
            TextureView textureView = textureManager?.GetTexture(sprite.Key.Texture);

            var resourceSet = gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                _perSpriteResourceLayout,
                sc.ProjectionMatrixBuffer,
                sc.ViewMatrixBuffer,
                gd.PointSampler,
                textureView,
                sc.PaletteView));

            _resourceSets.Add(resourceSet);

            cl.SetPipeline(sprite.DepthTested ? _depthTestPipeline : _noDepthPipeline);
            cl.SetGraphicsResourceSet(0, resourceSet);
            cl.SetVertexBuffer(0, _vb);
            cl.SetIndexBuffer(_ib, IndexFormat.UInt16);
            cl.SetVertexBuffer(1, _instanceBuffers[sprite.BufferId]);

            //cl.SetViewport(0, new Viewport(0, 0, sc.MainSceneColorTexture.Width, sc.MainSceneColorTexture.Height, depth, depth));
            cl.DrawIndexed((uint)Indices.Length, (uint)sprite.Instances.Length, 0, 0, 0);
            //cl.SetViewport(0, new Viewport(0, 0, sc.MainSceneColorTexture.Width, sc.MainSceneColorTexture.Height, 0, 1));
            cl.PopDebugGroup();
        }

        public void DestroyDeviceObjects()
        {
            _disposeCollector.DisposeAll();
            foreach (var buffer in _instanceBuffers)
                buffer.Dispose();
            _instanceBuffers.Clear();
            foreach (var resourceSet in _resourceSets)
                resourceSet.Dispose();
            _resourceSets.Clear();
        }
        public void Dispose() { DestroyDeviceObjects(); }
    }
}

