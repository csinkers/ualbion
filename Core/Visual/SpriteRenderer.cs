using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UAlbion.Api;
using UAlbion.Core.Textures;
using Veldrid;
using Veldrid.Utilities;

namespace UAlbion.Core.Visual
{
    struct SpriteShaderKey : IEquatable<SpriteShaderKey>
    {
        public bool UseArrayTexture { get; }
        public bool PerformDepthTest { get; }
        public bool UsePalette { get; }

        public SpriteShaderKey(MultiSprite sprite) : this(
                sprite.Key.Texture.ArrayLayers > 1,
                !sprite.Key.Flags.HasFlag(SpriteKeyFlags.NoDepthTest),
                sprite.Key.Texture is EightBitTexture) { }
        public SpriteShaderKey(bool useArrayTexture, bool performDepthTest, bool usePalette)
        {
            UseArrayTexture = useArrayTexture;
            PerformDepthTest = performDepthTest;
            UsePalette = usePalette;
        }
        public override string ToString() => $"{(UseArrayTexture ? "Array" : "Flat")}_{(PerformDepthTest ? "Depth" : "NoDepth")}{(UsePalette ? "_Pal" : "")}";

        public bool Equals(SpriteShaderKey other) => 
            UseArrayTexture == other.UseArrayTexture && 
            PerformDepthTest == other.PerformDepthTest && 
            UsePalette == other.UsePalette;

        public override bool Equals(object obj) => obj is SpriteShaderKey other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(UseArrayTexture, PerformDepthTest, UsePalette);
    }

    [StructLayout(LayoutKind.Sequential)]
    struct SpriteUniformInfo // Length must be multiple of 16
    {
        public SpriteKeyFlags Flags { get; set; } // 1 byte
        readonly byte _pad1;   // 2
        readonly ushort _pad2; // 4
        readonly uint _pad3;   // 8
        readonly double _pad4; // 16
    }

    public class SpriteRenderer : Component, IRenderer
    {
        // Vertex Layout
        static readonly VertexLayoutDescription VertexLayout = Vertex2DTextured.VertexLayout;

        // Instance Layout
        static readonly VertexLayoutDescription InstanceLayout = new VertexLayoutDescription(
                VertexLayoutHelper.Vector3D("Transform1"),
                VertexLayoutHelper.Vector3D("Transform2"),
                VertexLayoutHelper.Vector3D("Transform3"),
                VertexLayoutHelper.Vector3D("Transform4"),
                //VertexLayoutHelper.Vector3D("Offset"), VertexLayoutHelper.Vector2D("Size"),
                VertexLayoutHelper.Vector2D("TexPosition"), VertexLayoutHelper.Vector2D("TexSize"),
                VertexLayoutHelper.Int("TexLayer"), VertexLayoutHelper.Int("Flags")
                //VertexLayoutHelper.Float("Rotation")
            ) { InstanceStepRate = 1 };

        // Resource Sets
        static readonly ResourceLayoutDescription PerSpriteLayoutDescription = new ResourceLayoutDescription(
            ResourceLayoutHelper.Sampler("vdspv_0_0"), // SpriteSampler
            ResourceLayoutHelper.Texture("vdspv_0_1"), // SpriteTexture
            ResourceLayoutHelper.Uniform("vdspv_0_2") // Per-draw call uniform buffer
        );

        static readonly ushort[] Indices = {0, 1, 2, 2, 1, 3};
        static readonly Vertex2DTextured[] Vertices =
        {
            new Vertex2DTextured(-0.5f, 0.0f, 0.0f, 0.0f), new Vertex2DTextured(0.5f, 0.0f, 1.0f, 0.0f),
            new Vertex2DTextured(-0.5f, 1.0f, 0.0f, 1.0f), new Vertex2DTextured(0.5f, 1.0f, 1.0f, 1.0f),
        };


        readonly DisposeCollector _disposeCollector = new DisposeCollector();
        readonly List<Shader> _shaders = new List<Shader>();

        // Context objects
        DeviceBuffer _vertexBuffer;
        DeviceBuffer _indexBuffer;
        DeviceBuffer _uniformBuffer;
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

            if (key.UsePalette)
            {
                fragmentShaderName += ".pal";
                fragmentShaderContent =
                    @"#define USE_PALETTE
" + fragmentShaderContent;
            }


            var shaders = shaderCache.GetShaderPair(
                gd.ResourceFactory,
                vertexShaderName, fragmentShaderName,
                vertexShaderContent, fragmentShaderContent);

            _shaders.AddRange(shaders);
            var shaderSet = new ShaderSetDescription(new[] { VertexLayout, InstanceLayout }, shaders);

            Console.WriteLine($"-- Build SpriteRenderer-- IsDepth0..1: {gd.IsDepthRangeZeroToOne} YInvert: {gd.IsClipSpaceYInverted} UV TopLeft: {gd.IsUvOriginTopLeft}");
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

            _vertexBuffer = factory.CreateBuffer(new BufferDescription(Vertices.SizeInBytes(), BufferUsage.VertexBuffer));
            _indexBuffer = factory.CreateBuffer(new BufferDescription(Indices.SizeInBytes(), BufferUsage.IndexBuffer));
            _uniformBuffer = factory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<SpriteUniformInfo>(), BufferUsage.UniformBuffer));
            _vertexBuffer.Name = "SpriteVertexBuffer";
            _indexBuffer.Name = "SpriteIndexBuffer";
            _uniformBuffer.Name = "SpriteUniformBuffer";
            cl.UpdateBuffer(_vertexBuffer, 0, Vertices);
            cl.UpdateBuffer(_indexBuffer, 0, Indices);

            if (_pipelines != null)
            {
                foreach (var pipeline in _pipelines)
                    pipeline.Value.Dispose();
            }

            var keys = new[] // TODO: Use shader customisation constants instead
            {
                new SpriteShaderKey(true, true, false),
                new SpriteShaderKey(true, false, false),
                new SpriteShaderKey(false, true, false),
                new SpriteShaderKey(false, false, false),
                new SpriteShaderKey(true, true, true),
                new SpriteShaderKey(true, false, true),
                new SpriteShaderKey(false, true, true),
                new SpriteShaderKey(false, false, true)
            };

            _perSpriteResourceLayout = factory.CreateResourceLayout(PerSpriteLayoutDescription);
            _pipelines = keys.ToDictionary(x => x, x => BuildPipeline(gd, sc, x));
            _disposeCollector.Add(_vertexBuffer, _indexBuffer, _uniformBuffer, _perSpriteResourceLayout);
        }

        public IEnumerable<IRenderable> UpdatePerFrameResources(GraphicsDevice gd, CommandList cl, SceneContext sc, IEnumerable<IRenderable> renderables)
        {
            ITextureManager textureManager = Resolve<ITextureManager>();
            IDeviceObjectManager dom = Resolve<IDeviceObjectManager>();

            foreach(var sprite in renderables.OfType<MultiSprite>())
            {
                if (sprite.ActiveInstances == 0) continue;

                uint bufferSize = (uint) sprite.Instances.Length * SpriteInstanceData.StructSize;
                var buffer = dom.Prepare((sprite, sprite),
                    () =>
                    {
                        var newBuffer = gd.ResourceFactory.CreateBuffer(new BufferDescription(bufferSize, BufferUsage.VertexBuffer));
                        newBuffer.Name = $"B_SpriteInst:{sprite.Name}";
                        PerfTracker.IncrementFrameCounter("Create InstanceBuffer");
                        return newBuffer;
                    }, existing => existing.SizeInBytes != bufferSize);

                if (sprite.InstancesDirty)
                {
                    cl.UpdateBuffer(buffer, 0, sprite.Instances);
                    PerfTracker.IncrementFrameCounter("Update InstanceBuffers");
                }

                textureManager?.PrepareTexture(sprite.Key.Texture, gd);
                TextureView textureView = textureManager?.GetTexture(sprite.Key.Texture);
                dom.Prepare((sprite, textureView),
                    () =>
                    {
                        var resourceSet = gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                            _perSpriteResourceLayout,
                            gd.PointSampler,
                            textureView,
                            _uniformBuffer));
                        resourceSet.Name = $"RS_Sprite:{sprite.Key.Texture.Name}";
                        PerfTracker.IncrementFrameCounter("Create ResourceSet");
                        return resourceSet;
                    }, _ => false
                );

                sprite.InstancesDirty = false;
                yield return sprite;
            }

            Resolve<ISpriteManager>().Cleanup();
        }

        public void Render(GraphicsDevice gd, CommandList cl, SceneContext sc, RenderPasses renderPass, IRenderable renderable)
        {
            ITextureManager textureManager = Resolve<ITextureManager>();
            IDeviceObjectManager dom = Resolve<IDeviceObjectManager>();
            // float depth = gd.IsDepthRangeZeroToOne ? 0 : 1;
            var sprite = (MultiSprite)renderable;
            var shaderKey = new SpriteShaderKey(sprite);
            sprite.PipelineId = shaderKey.GetHashCode();

            //if (!shaderKey.UseArrayTexture)
            //    return;

            cl.PushDebugGroup($"Sprite:{sprite.Key.Texture.Name}:{sprite.Key.RenderOrder}");

            var uniformInfo = new SpriteUniformInfo { Flags = sprite.Key.Flags };
            cl.UpdateBuffer(_uniformBuffer, 0, uniformInfo);

            if (sc.PaletteView == null)
                return;

            TextureView textureView = textureManager?.GetTexture(sprite.Key.Texture);
            var resourceSet = dom.Get<ResourceSet>((sprite, textureView));
            var instanceBuffer = dom.Get<DeviceBuffer>((sprite, sprite));

            cl.SetPipeline(_pipelines[shaderKey]);
            cl.SetGraphicsResourceSet(0, resourceSet);
            cl.SetGraphicsResourceSet(1, sc.CommonResourceSet);
            cl.SetVertexBuffer(0, _vertexBuffer);
            cl.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
            cl.SetVertexBuffer(1, instanceBuffer);

            //cl.SetViewport(0, new Viewport(0, 0, sc.MainSceneColorTexture.Width, sc.MainSceneColorTexture.Height, depth, depth));
            cl.DrawIndexed((uint)Indices.Length, (uint)sprite.ActiveInstances, 0, 0, 0);
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

            foreach(var shader in _shaders)
                shader.Dispose();
            _shaders.Clear();
        }

        public void Dispose() => DestroyDeviceObjects();
    }
}

