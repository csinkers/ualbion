using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Core.Textures;
using Veldrid;
using Veldrid.Utilities;

namespace UAlbion.Core.Visual
{
    struct SpriteShaderKey : IEquatable<SpriteShaderKey>
    {
        public bool UseArrayTexture { get; }
        public bool PerformDepthTest { get; }
        public SpriteShaderKey(bool useArrayTexture, bool performDepthTest) { UseArrayTexture = useArrayTexture; PerformDepthTest = performDepthTest; }
        public override string ToString() => $"{(UseArrayTexture ? "Array" : "Flat")}_{(PerformDepthTest ? "Depth" :"NoDepth")}";
        public bool Equals(SpriteShaderKey other) => UseArrayTexture == other.UseArrayTexture && PerformDepthTest == other.PerformDepthTest;
        public override bool Equals(object obj) => obj is SpriteShaderKey other && Equals(other);
        public override int GetHashCode() { unchecked { return (UseArrayTexture.GetHashCode() * 397) ^ PerformDepthTest.GetHashCode(); } }
    }

    public class SpriteRenderer : Component, IRenderer
    {
        // Vertex Layout
        static readonly VertexLayoutDescription VertexLayout = Vertex2DTextured.VertexLayout;

        // Instance Layout
        static readonly VertexLayoutDescription InstanceLayout = new VertexLayoutDescription(
                VertexLayoutHelper.Vector3D("Offset"), VertexLayoutHelper.Vector2D("Size"),
                VertexLayoutHelper.Vector2D("TexPosition"), VertexLayoutHelper.Vector2D("TexSize"),
                VertexLayoutHelper.Int("TexLayer"), VertexLayoutHelper.Int("Flags"),
                VertexLayoutHelper.Float("Rotation")
            )
            {InstanceStepRate = 1};

        // Resource Sets
        static readonly ResourceLayoutDescription PerSpriteLayoutDescription = new ResourceLayoutDescription(
            ResourceLayoutHelper.UniformV("vdspv_0_0"), // Perspective Matrix
            ResourceLayoutHelper.UniformV("vdspv_0_1"), // View-Model Matrix
            ResourceLayoutHelper.Sampler("vdspv_0_2"), // SpriteSampler
            ResourceLayoutHelper.Texture("vdspv_0_3"), // SpriteTexture
            ResourceLayoutHelper.Texture("vdspv_0_4")  // PaletteTexture
        );

        static readonly Vertex2DTextured[] CenteredVertices =
        {
            new Vertex2DTextured(-0.5f, 0.0f, 0.0f, 0.0f), new Vertex2DTextured(0.5f, 0.0f, 1.0f, 0.0f),
            new Vertex2DTextured(-0.5f, 1.0f, 0.0f, 1.0f), new Vertex2DTextured(0.5f, 1.0f, 1.0f, 1.0f),
        };

        static readonly Vertex2DTextured[] LeftAlignedVertices =
        {
            new Vertex2DTextured(0.0f, 0.0f, 0.0f, 0.0f), new Vertex2DTextured(1.0f, 0.0f, 1.0f, 0.0f),
            new Vertex2DTextured(0.0f, 1.0f, 0.0f, 1.0f), new Vertex2DTextured(1.0f, 1.0f, 1.0f, 1.0f),
        };

        static readonly ushort[] Indices = {0, 1, 2, 2, 1, 3};
        readonly DisposeCollector _disposeCollector = new DisposeCollector();
        readonly IList<DeviceBuffer> _instanceBuffers = new List<DeviceBuffer>();
        readonly IList<ResourceSet> _resourceSets = new List<ResourceSet>();
        readonly List<Shader> _shaders = new List<Shader>();

        // Context objects
        DeviceBuffer _centeredVb;
        DeviceBuffer _leftVb;
        DeviceBuffer _ib;
        Dictionary<SpriteShaderKey, Pipeline> _pipelines = new Dictionary<SpriteShaderKey, Pipeline>();
        ResourceLayout _perSpriteResourceLayout;

        public RenderPasses RenderPasses => RenderPasses.Standard;

        Pipeline BuildPipeline(GraphicsDevice gd, SceneContext sc, SpriteShaderKey key)
        {
            var shaderCache = Resolve<IShaderCache>();
            var vertexShaderName = "SpriteSV.vert";
            var fragmentShaderName = "SpriteSF.frag";
            var vertexShaderContent = shaderCache.GetGlsl(vertexShaderName);
            var fragmentShaderContent = shaderCache.GetGlsl(fragmentShaderName);

            if (key.UseArrayTexture)
            {
                fragmentShaderName += ".array";
                fragmentShaderContent =
                    @"#define USE_ARRAY_TEXTURE
" + fragmentShaderContent;
            }

            var shaders = shaderCache.GetShaderPair(
                gd.ResourceFactory,
                vertexShaderName, fragmentShaderName,
                vertexShaderContent, fragmentShaderContent);

            _shaders.AddRange(shaders);
            var shaderSet = new ShaderSetDescription(new[] { VertexLayout, InstanceLayout }, shaders);

            var depthStencilMode = 
                key.PerformDepthTest
                ?  gd.IsDepthRangeZeroToOne
                    ? DepthStencilStateDescription.DepthOnlyLessEqual
                    : DepthStencilStateDescription.DepthOnlyGreaterEqual
                : DepthStencilStateDescription.Disabled;

            var rasterizerMode = new RasterizerStateDescription(
                FaceCullMode.None, 
                PolygonFillMode.Solid, 
                FrontFace.Clockwise,
                key.PerformDepthTest, // depth test
                true); // scissor test

            var pipelineDescription = new GraphicsPipelineDescription(
                BlendStateDescription.SingleAlphaBlend,
                depthStencilMode,
                rasterizerMode,
                PrimitiveTopology.TriangleList,
                new ShaderSetDescription(new[] { VertexLayout, InstanceLayout },
                    shaderSet.Shaders,
                    ShaderHelper.GetSpecializations(gd)),
                new[] { _perSpriteResourceLayout, sc.CommonResourceLayout },
                sc.MainSceneFramebuffer.OutputDescription);

            var pipeline = gd.ResourceFactory.CreateGraphicsPipeline(ref pipelineDescription);
            pipeline.Name = $"P_Sprite_{key}";
            return pipeline;
        }

        public void CreateDeviceObjects(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
            ResourceFactory factory = gd.ResourceFactory;

            _centeredVb = factory.CreateBuffer(new BufferDescription(CenteredVertices.SizeInBytes(), BufferUsage.VertexBuffer));
            _leftVb = factory.CreateBuffer(new BufferDescription(CenteredVertices.SizeInBytes(), BufferUsage.VertexBuffer));
            _ib = factory.CreateBuffer(new BufferDescription(Indices.SizeInBytes(), BufferUsage.IndexBuffer));
            _centeredVb.Name = "SpriteVertexBufferC";
            _leftVb.Name = "SpriteVertexBufferL";
            _ib.Name = "SpriteIndexBuffer";
            cl.UpdateBuffer(_centeredVb, 0, CenteredVertices);
            cl.UpdateBuffer(_leftVb, 0, LeftAlignedVertices);
            cl.UpdateBuffer(_ib, 0, Indices);

            if (_pipelines != null)
            {
                foreach (var pipeline in _pipelines)
                    pipeline.Value.Dispose();
            }

            var keys = new[]
            {
                new SpriteShaderKey(true, true),
                new SpriteShaderKey(true, false),
                new SpriteShaderKey(false, true),
                new SpriteShaderKey(false, false)
            };
            _perSpriteResourceLayout = factory.CreateResourceLayout(PerSpriteLayoutDescription);
            _pipelines = keys.ToDictionary(x => x, x => BuildPipeline(gd, sc, x));
            _disposeCollector.Add(_centeredVb, _leftVb, _ib, _perSpriteResourceLayout);
        }

        public IEnumerable<IRenderable> UpdatePerFrameResources(GraphicsDevice gd, CommandList cl, SceneContext sc, IEnumerable<IRenderable> renderables)
        {
            ITextureManager textureManager = Resolve<ITextureManager>();
            ISpriteResolver spriteResolver = Resolve<ISpriteResolver>();

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

            var resolved = renderables.OfType<Sprite>().Select(spriteResolver.Resolve);
            var grouped = resolved.GroupBy(x => x.Item1, x => x.Item2);
            foreach (var group in grouped)
            {
                var multiSprite = group.Key.Flags.HasFlag(SpriteFlags.NoTransform)
                    ? new UiMultiSprite(group.Key, _instanceBuffers.Count, group) 
                    : new MultiSprite(group.Key, _instanceBuffers.Count, group);

                SetupMultiSpriteResources(multiSprite);
                yield return multiSprite;
            }

            foreach(var multiSprite in renderables.OfType<MultiSprite>())
            {
                if (multiSprite.Instances.Length == 0) continue;
                SetupMultiSpriteResources(multiSprite);
                yield return multiSprite;
            }
        }

        public void Render(GraphicsDevice gd, CommandList cl, SceneContext sc, RenderPasses renderPass, IRenderable renderable)
        {
            ITextureManager textureManager = Resolve<ITextureManager>();
            // float depth = gd.IsDepthRangeZeroToOne ? 0 : 1;
            var sprite = (MultiSprite)renderable;
            var shaderKey = new SpriteShaderKey(
                sprite.Key.Texture.ArrayLayers > 1,
                !sprite.Flags.HasFlag(SpriteFlags.NoDepthTest));

            //if (!shaderKey.UseArrayTexture)
            //    return;

            cl.PushDebugGroup($"Sprite:{sprite.Key.Texture.Name}:{sprite.Key.RenderOrder}");
            TextureView textureView = textureManager?.GetTexture(sprite.Key.Texture);

            if (sc.PaletteView == null)
                return;

            var resourceSet = gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                _perSpriteResourceLayout,
                sc.ProjectionMatrixBuffer,
                sc.ModelViewMatrixBuffer,
                gd.PointSampler,
                textureView,
                sc.PaletteView
                ));

            resourceSet.Name = $"RS_Sprite:{sprite.Key.Texture.Name}";
            _resourceSets.Add(resourceSet);

            cl.SetPipeline(_pipelines[shaderKey]);
            cl.SetGraphicsResourceSet(0, resourceSet);
            cl.SetGraphicsResourceSet(1, sc.CommonResourceSet);
            cl.SetVertexBuffer(0, sprite.Flags.HasFlag(SpriteFlags.LeftAligned) ? _leftVb : _centeredVb);
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

            if (_pipelines != null)
            {
                foreach (var pipeline in _pipelines)
                    pipeline.Value.Dispose();
                _pipelines = null;
            }

            foreach (var buffer in _instanceBuffers)
                buffer.Dispose();
            _instanceBuffers.Clear();

            foreach (var resourceSet in _resourceSets)
                resourceSet.Dispose();
            _resourceSets.Clear();

            foreach(var shader in _shaders)
                shader.Dispose();
            _shaders.Clear();
        }

        public void Dispose() => DestroyDeviceObjects();
    }
}

