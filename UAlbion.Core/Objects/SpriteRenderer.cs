using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using Veldrid;
using Veldrid.SPIRV;
using Veldrid.Utilities;

namespace UAlbion.Core.Objects
{
    public interface ISpriteResolver
    {
        Tuple<SpriteRenderer.SpriteKey, SpriteRenderer.InstanceData> Resolve(SpriteDefinition spriteDefinition);
    }

    public class SpriteDefinition<T> : SpriteDefinition where T : Enum
    {
        public SpriteDefinition(T id, int subObject, Vector2 position, int renderOrder, SpriteFlags flags)
            : base(subObject, position, renderOrder, flags) { Id = id; }

        public T Id { get; }
        public override Type IdType => typeof(T);
        public override int NumericId => (int)(object)Id;
    }

    public abstract class SpriteDefinition : IRenderable
    {
        protected SpriteDefinition(int subObject, Vector2 position, int renderOrder, SpriteFlags flags)
        {
            SubObject = subObject;
            Position = position;
            RenderOrder = renderOrder;
            Flags = flags;
        }

        public Type Renderer => typeof(SpriteRenderer);
        public int RenderOrder { get; }
        public SpriteFlags Flags { get; }
        public int SubObject { get; }
        public Vector2 Position { get; }
        public abstract Type IdType { get; }
        public abstract int NumericId { get; }
    }

    public class SpriteRenderer : IRenderer
    {
        public struct SpriteKey : IEquatable<SpriteKey>
        {
            public SpriteKey(ITexture texture, int renderOrder) { Texture = texture; RenderOrder = renderOrder; }
            public ITexture Texture { get; }
            public int RenderOrder { get; }
            public bool Equals(SpriteKey other) => Equals(Texture, other.Texture) && RenderOrder == other.RenderOrder;
            public override bool Equals(object obj) => obj is SpriteKey other && Equals(other);
            public override int GetHashCode() { unchecked { return ((Texture != null ? Texture.GetHashCode() : 0) * 397) ^ RenderOrder; } }
        }

        public struct InstanceData
        {
            public static readonly uint StructSize = (uint)Unsafe.SizeOf<InstanceData>();
            public static readonly VertexLayoutDescription VertexLayout = new VertexLayoutDescription(
                VertexLayoutH.Vector2D("Offset"), VertexLayoutH.Vector2D("Size"),
                VertexLayoutH.Vector2D("TexPosition"), VertexLayoutH.Vector2D("TexSize"),
                VertexLayoutH.Int("TexLayer"), VertexLayoutH.Int("Flags")
                )
            { InstanceStepRate = 1 };
            public InstanceData(Vector2 position, Vector2 size, Vector2 texPosition, Vector2 texSize, int texLayer, SpriteFlags flags)
            {
                Offset = position; Size = size;
                TexPosition = texPosition; TexSize = texSize;
                TexLayer = texLayer;
                Flags = flags;
            }

            public Vector2 Offset { get; }
            public Vector2 Size { get; }
            public Vector2 TexPosition { get; }
            public Vector2 TexSize { get; }
            public int TexLayer { get; }
            public SpriteFlags Flags { get; }
        }

        class MultiSprite : IRenderable
        {
            public MultiSprite(SpriteKey key, int bufferId, IEnumerable<InstanceData> sprites)
            {
                Key = key;
                BufferId = bufferId;
                Instances = sprites.ToArray();
            }

            public int RenderOrder => Key.RenderOrder;
            public Type Renderer => typeof(SpriteRenderer);
            public SpriteKey Key { get; }
            public int BufferId { get; }
            public InstanceData[] Instances { get; }
        }

        static class Shader
        {
            public static readonly VertexLayoutDescription VertexLayout = new VertexLayoutDescription(VertexLayoutH.Vector2D("Position"));
            public static readonly ResourceLayoutDescription PerSpriteLayoutDescription = new ResourceLayoutDescription(
                ResourceLayoutH.Uniform("Projection"),
                ResourceLayoutH.Uniform("View"),
                ResourceLayoutH.Sampler("SpriteSampler"),
                ResourceLayoutH.Texture("SpriteTexture"),
                ResourceLayoutH.Texture("PaletteView"));

            public const string VertexShader = @"
            #version 450

            layout(set = 0, binding = 0) uniform Projection { mat4 _Proj; };
            layout(set = 0, binding = 1) uniform View { mat4 _View; };

            layout(location = 0) in vec2 Position;
            layout(location = 1) in vec2 Offset;
            layout(location = 2) in vec2 Size;
            layout(location = 3) in vec2 TexOffset;
            layout(location = 4) in vec2 TexSize;
            layout(location = 5) in int TexLayer;
            layout(location = 6) in int Flags;

            layout(location = 0) out vec2 fsin_0;
            layout(location = 1) out flat int fsin_1;
            layout(location = 2) out flat int fsin_2;

            void main()
            {
                fsin_0 = Position * TexSize + TexOffset;
                fsin_1 = TexLayer;
                fsin_2 = Flags;
                gl_Position = _Proj * _View * vec4((Position * Size) + Offset, 0, 1);
            }";

            public const string FragmentShader = @"
            #version 450

            layout(set = 0, binding = 2) uniform sampler SpriteSampler;
            layout(set = 0, binding = 3) uniform texture2D SpriteTexture;
            layout(set = 0, binding = 4) uniform texture2D PaletteView;

            layout(location = 0) in vec2 fsin_0;
            layout(location = 1) in flat int fsin_1;
            layout(location = 2) in flat int fsin_2;

            layout(location = 0) out vec4 OutputColor;

            void main()
            {
                if ((fsin_2 & 4) != 0)
                {
                    float redChannel = texture(sampler2D(SpriteTexture, SpriteSampler), fsin_0)[0];
                    float index = 255.0f * redChannel;
                    vec4 color = texture(sampler2D(PaletteView, SpriteSampler), vec2(redChannel, 0.0f));
                    OutputColor = color;
                }
                else
                {
                    OutputColor = texture(sampler2D(SpriteTexture, SpriteSampler), fsin_0);
                }
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

            _vb = factory.CreateBuffer(new BufferDescription(new Vertex2DTextured[4].SizeInBytes(), BufferUsage.VertexBuffer));
            _ib = factory.CreateBuffer(new BufferDescription(Indices.SizeInBytes(), BufferUsage.IndexBuffer));
            _vb.Name = "SpriteVertexBuffer";
            _ib.Name = "SpriteIndexBuffer";
            cl.UpdateBuffer(_vb, 0, Vertices);
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
                new ShaderSetDescription(new[] { Shader.VertexLayout, InstanceData.VertexLayout }, shaderSet.Shaders, ShaderHelper.GetSpecializations(gd)),
                new[] { _perSpriteResourceLayout },
                sc.MainSceneFramebuffer.OutputDescription);

            _pipeline = factory.CreateGraphicsPipeline(ref pd);
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
                var buffer = gd.ResourceFactory.CreateBuffer(new BufferDescription((uint)multiSprite.Instances.Length * InstanceData.StructSize, BufferUsage.VertexBuffer));
                buffer.Name = $"B_SpriteInst{_instanceBuffers.Count}";
                gd.UpdateBuffer(buffer, 0, multiSprite.Instances);
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

