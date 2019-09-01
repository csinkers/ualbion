using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Core.Textures;
using Veldrid;
using Veldrid.SPIRV;
using Veldrid.Utilities;

namespace UAlbion.Core.Visual
{
    public class SpriteRenderer : IRenderer
    {
        static class Shader
        {
            // Vertex Layout
            public static readonly VertexLayoutDescription VertexLayout = new VertexLayoutDescription(VertexLayoutHelper.Vector2D("Position"));

            // Instance Layout
            public static readonly VertexLayoutDescription InstanceLayout = new VertexLayoutDescription(
                    VertexLayoutHelper.Vector3D("Offset"), VertexLayoutHelper.Vector2D("Size"),
                    VertexLayoutHelper.Vector2D("TexPosition"), VertexLayoutHelper.Vector2D("TexSize"),
                    VertexLayoutHelper.Int("TexLayer"), VertexLayoutHelper.Int("Flags"))
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

            // Instance Data
            layout(location = 1) in vec3 _Offset;
            layout(location = 2) in vec2 _Size;
            layout(location = 3) in vec2 _TexOffset;
            layout(location = 4) in vec2 _TexSize;
            layout(location = 5) in int _TexLayer;
            layout(location = 6) in int _Flags;

            // Outputs to fragment shader
            layout(location = 0) out vec2 fsin_0;     // Texture Coordinates
            layout(location = 1) out flat int fsin_1; // Texture Layer
            layout(location = 2) out flat int fsin_2; // Flags

            void main()
            {
                fsin_0 = _Position * _TexSize + _TexOffset;
                fsin_1 = _TexLayer;
                fsin_2 = _Flags;

                if((_Flags & 1) != 0) // If NoTransform set
                    gl_Position = vec4((_Position * _Size) + _Offset.xy, _Offset.z, 1);
                else
                    gl_Position = Projection * View * vec4((_Position * _Size) + _Offset.xy, _Offset.z, 1);
            }";

            public const string FragmentShader = @"
            #version 450

            // Resource Sets
            layout(set = 0, binding = 2) uniform sampler SpriteSampler;   // vdspv_0_2
            layout(set = 0, binding = 3) uniform texture2D SpriteTexture; // vdspv_0_3
            layout(set = 0, binding = 4) uniform texture2D PaletteView;   // vdspv_0_4

            // Inputs from vertex shader
            layout(location = 0) in vec2 fsin_0;     // Texture Coordinates
            layout(location = 1) in flat int fsin_1; // Texture Layer
            layout(location = 2) in flat int fsin_2; // Flags

            // Fragment shader outputs
            layout(location = 0) out vec4 OutputColor;

            void main()
            {
                /* NoTransform = 1,  Highlight      = 2,
                   UsePalette  = 4,  OnlyEvenFrames = 8,
                   RedTint     = 16, GreenTint      = 32,
                   BlueTint    = 64, Transparent    = 128 */

                vec4 color;
                if ((fsin_2 & 4) != 0)
                {
                    float redChannel = texture(sampler2D(SpriteTexture, SpriteSampler), fsin_0)[0];
                    float index = 255.0f * redChannel;
                    color = texture(sampler2D(PaletteView, SpriteSampler), vec2(redChannel, 0.0f));
                    if(index == 0)
                        color = vec4(0.0f, 0.0f, 0.0f, 0.0f);
                }
                else
                {
                    color = texture(sampler2D(SpriteTexture, SpriteSampler), fsin_0);
                }

                if((fsin_2 &   2) != 0) color = color * 1.2; // Highlight
                if((fsin_2 &  16) != 0) color = vec4(color.x * 1.5f + 0.3f, color.yzw);         // Red tint
                if((fsin_2 &  32) != 0) color = vec4(color.x, color.y * 1.5f + 0.3f, color.zw); // Green tint
                if((fsin_2 &  64) != 0) color = vec4(color.xy, color.z * 1.5f + 0.f, color.w); // Blue tint
                if((fsin_2 & 128) != 0) color = vec4(color.xyz, color.w * 0.5f); // Transparent

                OutputColor = color;
            }";
        }

        static readonly Vector2[] Vertices =
        {
            new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f),
            new Vector2(0.0f, 1.0f), new Vector2(1.0f, 1.0f),
        };
        static readonly ushort[] Indices = { 0, 1, 2, 2, 1, 3 };
        readonly DisposeCollector _disposeCollector = new DisposeCollector();
        readonly IList<DeviceBuffer> _instanceBuffers = new List<DeviceBuffer>();
        readonly ITextureManager _textureManager;
        readonly ISpriteResolver _spriteResolver;

        // Context objects
        DeviceBuffer _vb;
        DeviceBuffer _ib;
        Pipeline _pipeline;
        ResourceLayout _perSpriteResourceLayout;

        public SpriteRenderer(ITextureManager textureManager, ISpriteResolver spriteResolver)
        {
            _textureManager = textureManager ?? throw new ArgumentNullException(nameof(textureManager));
            _spriteResolver = spriteResolver ?? throw new ArgumentNullException(nameof(spriteResolver));
        }

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

            var pd = new GraphicsPipelineDescription(
                BlendStateDescription.SingleAlphaBlend,
                depthStencilMode,
                rasterizerMode,
                PrimitiveTopology.TriangleList,
                new ShaderSetDescription(new[] { Shader.VertexLayout, Shader.InstanceLayout },
                    shaderSet.Shaders,
                    ShaderHelper.GetSpecializations(gd)),
                new[] { _perSpriteResourceLayout },
                sc.MainSceneFramebuffer.OutputDescription);

            _pipeline = factory.CreateGraphicsPipeline(ref pd);
            _pipeline.Name = "P_SpriteRenderer";
            _disposeCollector.Add(_vb, _ib, _perSpriteResourceLayout, _pipeline);
        }

        public IEnumerable<IRenderable> UpdatePerFrameResources(GraphicsDevice gd, CommandList cl, SceneContext sc, IEnumerable<IRenderable> renderables)
        {
            foreach (var buffer in _instanceBuffers)
                buffer.Dispose();
            _instanceBuffers.Clear();

            var resolved = renderables.OfType<SpriteDefinition>().Select(_spriteResolver.Resolve);
            var grouped = resolved.GroupBy(x => x.Item1, x => x.Item2);
            foreach (var group in grouped)
            {
                _textureManager.PrepareTexture(group.Key.Texture, gd);
                var multiSprite = new MultiSprite(group.Key, _instanceBuffers.Count, group);
                var buffer = gd.ResourceFactory.CreateBuffer(new BufferDescription((uint)multiSprite.Instances.Length * SpriteInstanceData.StructSize, BufferUsage.VertexBuffer));
                buffer.Name = $"B_SpriteInst{_instanceBuffers.Count}";
                cl.UpdateBuffer(buffer, 0, multiSprite.Instances);
                _instanceBuffers.Add(buffer);
                yield return multiSprite;
            }

            foreach(var multiSprite in renderables.OfType<MultiSprite>())
            {
                _textureManager.PrepareTexture(multiSprite.Key.Texture, gd);
                multiSprite.BufferId = _instanceBuffers.Count;
                var buffer = gd.ResourceFactory.CreateBuffer(new BufferDescription((uint)multiSprite.Instances.Length * SpriteInstanceData.StructSize, BufferUsage.VertexBuffer));
                buffer.Name = $"B_SpriteInst{_instanceBuffers.Count}";
                cl.UpdateBuffer(buffer, 0, multiSprite.Instances);
                _instanceBuffers.Add(buffer);
                yield return multiSprite;
            }
        }

        public void Render(GraphicsDevice gd, CommandList cl, SceneContext sc, RenderPasses renderPass, IRenderable renderable)
        {
            float depth = gd.IsDepthRangeZeroToOne ? 0 : 1;
            var sprite = (MultiSprite)renderable;
            cl.PushDebugGroup($"Sprite:{sprite.Key.Texture.Name}:{sprite.Key.RenderOrder}");
            TextureView textureView = _textureManager.GetTexture(sprite.Key.Texture);
            var resourceSet = gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(_perSpriteResourceLayout,
                sc.ProjectionMatrixBuffer,
                sc.ViewMatrixBuffer,
                gd.PointSampler,
                textureView,
                sc.PaletteView));

            cl.SetPipeline(_pipeline);
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
        }
        public void Dispose() { DestroyDeviceObjects(); }
    }
}

