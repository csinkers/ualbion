using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UAlbion.Api;
using UAlbion.Core.Textures;
using UAlbion.Core.Visual;
using Veldrid;
using Veldrid.Utilities;

namespace UAlbion.Core.Veldrid.Visual
{
    public class SpriteRenderer : Component, IRenderer
    {
        // Vertex Layout
        static readonly VertexLayoutDescription VertexLayout = VertexLayoutHelper.Vertex2DTextured;

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
            )
        { InstanceStepRate = 1 };

        // Resource Sets
        static readonly ResourceLayoutDescription PerSpriteLayoutDescription = new ResourceLayoutDescription(
            ResourceLayoutHelper.Sampler("vdspv_0_0"), // SpriteSampler
            ResourceLayoutHelper.Texture("vdspv_0_1"), // SpriteTexture
            ResourceLayoutHelper.Uniform("vdspv_0_2") // Per-draw call uniform buffer
        );

        static readonly ushort[] Indices = { 0, 1, 2, 2, 1, 3 };
        static readonly Vertex2DTextured[] Vertices =
        {
            new Vertex2DTextured(-0.5f, 0.0f, 0.0f, 0.0f), new Vertex2DTextured(0.5f, 0.0f, 1.0f, 0.0f),
            new Vertex2DTextured(-0.5f, 1.0f, 0.0f, 1.0f), new Vertex2DTextured(0.5f, 1.0f, 1.0f, 1.0f),
        };


        readonly DisposeCollector _disposeCollector = new DisposeCollector();
        readonly List<Shader> _shaders = new List<Shader>();
        readonly Dictionary<SpriteShaderKey, Pipeline> _pipelines = new Dictionary<SpriteShaderKey, Pipeline>();

        // Context objects
        DeviceBuffer _vertexBuffer;
        DeviceBuffer _indexBuffer;
        DeviceBuffer _uniformBuffer;
        ResourceLayout _perSpriteResourceLayout;

        public bool CanRender(Type renderable) => renderable == typeof(MultiSprite);
        public RenderPasses RenderPasses => RenderPasses.Standard;

        Pipeline BuildPipeline(GraphicsDevice gd, SceneContext sc, SpriteShaderKey key)
        {
            var shaderCache = Resolve<IShaderCache>();
            var shaderName = key.UseCylindricalShader ? "CylindricalSprite" : "Sprite"; 
            var vertexShaderName = shaderName + "SV.vert";
            var fragmentShaderName = shaderName + "SF.frag";
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

            var depthStencilMode =
                key.PerformDepthTest
                ? gd.IsDepthRangeZeroToOne
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

        public void CreateDeviceObjects(IRendererContext context)
        {
            var c = (VeldridRendererContext)context;
            var cl = c.CommandList;
            var gd = c.GraphicsDevice;

            ResourceFactory factory = gd.ResourceFactory;

            _vertexBuffer = factory.CreateBuffer(new BufferDescription(Vertices.SizeInBytes(), BufferUsage.VertexBuffer));
            _indexBuffer = factory.CreateBuffer(new BufferDescription(Indices.SizeInBytes(), BufferUsage.IndexBuffer));
            _uniformBuffer = factory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<SpriteUniformInfo>(), BufferUsage.UniformBuffer));
            _vertexBuffer.Name = "SpriteVertexBuffer";
            _indexBuffer.Name = "SpriteIndexBuffer";
            _uniformBuffer.Name = "SpriteUniformBuffer";
            cl.UpdateBuffer(_vertexBuffer, 0, Vertices);
            cl.UpdateBuffer(_indexBuffer, 0, Indices);

            _perSpriteResourceLayout = factory.CreateResourceLayout(PerSpriteLayoutDescription);
            _disposeCollector.Add(_vertexBuffer, _indexBuffer, _uniformBuffer, _perSpriteResourceLayout);
        }

        public IEnumerable<IRenderable> UpdatePerFrameResources(IRendererContext context, IEnumerable<IRenderable> renderables)
        {
            var c = (VeldridRendererContext)context;
            var cl = c.CommandList;
            var gd = c.GraphicsDevice;
            var sc = c.SceneContext;

            ITextureManager textureManager = Resolve<ITextureManager>();
            IDeviceObjectManager objectManager = Resolve<IDeviceObjectManager>();
            EngineFlags engineFlags = Resolve<IEngineSettings>().Flags;

            foreach (var renderable in renderables)
            {
                var sprite = (MultiSprite)renderable;
                if (sprite.ActiveInstances == 0)
                    continue;

                var shaderKey = new SpriteShaderKey(sprite, engineFlags);
                if (!_pipelines.ContainsKey(shaderKey))
                    _pipelines.Add(shaderKey, BuildPipeline(gd, sc, shaderKey));

                uint bufferSize = (uint)sprite.Instances.Length * SpriteInstanceData.StructSize;
                var buffer = objectManager.Get<DeviceBuffer>((sprite, sprite));
                if (buffer?.SizeInBytes != bufferSize)
                {
                    buffer = gd.ResourceFactory.CreateBuffer(new BufferDescription(bufferSize, BufferUsage.VertexBuffer));
                    buffer.Name = $"B_SpriteInst:{sprite.Name}";
                    PerfTracker.IncrementFrameCounter("Create InstanceBuffer");
                    objectManager.Set((sprite, sprite), buffer);
                }

                if (sprite.InstancesDirty)
                {
                    cl.UpdateBuffer(buffer, 0, sprite.Instances);
                    PerfTracker.IncrementFrameCounter("Update InstanceBuffers");
                }

                textureManager?.PrepareTexture(sprite.Key.Texture, context);
                TextureView textureView = (TextureView)textureManager?.GetTexture(sprite.Key.Texture);

                var resourceSet = objectManager.Get<ResourceSet>((sprite, textureView));
                if (resourceSet == null)
                {
                    resourceSet = gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                        _perSpriteResourceLayout,
                        gd.PointSampler,
                        textureView,
                        _uniformBuffer));
                    resourceSet.Name = $"RS_Sprite:{sprite.Key.Texture.Name}";
                    PerfTracker.IncrementFrameCounter("Create ResourceSet");
                    objectManager.Set((sprite, textureView), resourceSet);
                }

                sprite.InstancesDirty = false;
                yield return sprite;
            }

            Resolve<ISpriteManager>().Cleanup();
        }

        public void Render(IRendererContext context, RenderPasses renderPass, IRenderable renderable)
        {
            var c = (VeldridRendererContext)context;
            var cl = c.CommandList;
            var sc = c.SceneContext;

            ITextureManager textureManager = Resolve<ITextureManager>();
            IDeviceObjectManager dom = Resolve<IDeviceObjectManager>();
            EngineFlags engineFlags = Resolve<IEngineSettings>().Flags;
            // float depth = gd.IsDepthRangeZeroToOne ? 0 : 1;
            var sprite = (MultiSprite)renderable;
            var shaderKey = new SpriteShaderKey(sprite, engineFlags);
            sprite.PipelineId = shaderKey.GetHashCode();

            //if (!shaderKey.UseArrayTexture)
            //    return;

            cl.PushDebugGroup(sprite.Name);

            if (sc.PaletteView == null)
                return;

            TextureView textureView = (TextureView)textureManager?.GetTexture(sprite.Key.Texture);
            var resourceSet = dom.Get<ResourceSet>((sprite, textureView));
            var instanceBuffer = dom.Get<DeviceBuffer>((sprite, sprite));
            var uniformInfo = new SpriteUniformInfo
            {
                Flags = sprite.Key.Flags,
                TextureWidth = textureView?.Target.Width ?? 1,
                TextureHeight = textureView?.Target.Height ?? 1
            };

            cl.UpdateBuffer(_uniformBuffer, 0, uniformInfo);
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

            foreach (var pipeline in _pipelines)
                pipeline.Value.Dispose();
            _pipelines.Clear();

            foreach (var shader in _shaders)
                shader.Dispose();
            _shaders.Clear();
        }

        public void Dispose() => DestroyDeviceObjects();
    }
}

