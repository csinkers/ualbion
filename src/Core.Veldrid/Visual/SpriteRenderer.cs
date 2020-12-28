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
    public sealed class SpriteRenderer : Component, IRenderer
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
                VertexLayoutHelper.IntElement("TexLayer"), VertexLayoutHelper.UIntElement("Flags")
            //VertexLayoutHelper.FloatElement("Rotation")
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

#pragma warning disable CA2213 // Analysis doesn't know about DisposeCollector
        // Context objects
        DeviceBuffer _vertexBuffer;
        DeviceBuffer _indexBuffer;
        ResourceLayout _perSpriteResourceLayout;
#pragma warning restore CA2213 // Analysis doesn't know about DisposeCollector

        public Type[] RenderableTypes => new [] { typeof(MultiSprite) };
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
                    shaders,
                    ShaderHelper.GetSpecializations(gd)),
                new[] { _perSpriteResourceLayout, sc.CommonResourceLayout },
                gd.SwapchainFramebuffer.OutputDescription);

            var pipeline = gd.ResourceFactory.CreateGraphicsPipeline(ref pipelineDescription);
            pipeline.Name = $"P_Sprite_{key}";
            return pipeline;
        }

        public void CreateDeviceObjects(IRendererContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            var c = (VeldridRendererContext)context;
            var cl = c.CommandList;
            var gd = c.GraphicsDevice;
            var factory = gd.ResourceFactory;

            _vertexBuffer = factory.CreateBuffer(new BufferDescription(Vertices.SizeInBytes(), BufferUsage.VertexBuffer));
            _indexBuffer = factory.CreateBuffer(new BufferDescription(Indices.SizeInBytes(), BufferUsage.IndexBuffer));
            _vertexBuffer.Name = "SpriteVertexBuffer";
            _indexBuffer.Name = "SpriteIndexBuffer";
            cl.UpdateBuffer(_vertexBuffer, 0, Vertices);
            cl.UpdateBuffer(_indexBuffer, 0, Indices);

            _perSpriteResourceLayout = factory.CreateResourceLayout(PerSpriteLayoutDescription);
            _disposeCollector.Add(_vertexBuffer, _indexBuffer, _perSpriteResourceLayout);
        }

        public void UpdatePerFrameResources(IRendererContext context, IEnumerable<IRenderable> renderables, IList<IRenderable> results)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (renderables == null) throw new ArgumentNullException(nameof(renderables));
            if (results == null) throw new ArgumentNullException(nameof(results));

            var c = (VeldridRendererContext)context;
            var cl = c.CommandList;
            var gd = c.GraphicsDevice;
            var textureManager = Resolve<ITextureManager>();
            var objectManager = Resolve<IDeviceObjectManager>();
            var engineFlags = Resolve<IEngineSettings>().Flags;

            foreach (var renderable in renderables)
            {
                var sprite = (MultiSprite)renderable;
                if (sprite.ActiveInstances == 0)
                    continue;

                cl.PushDebugGroup(sprite.Name);
                var shaderKey = new SpriteShaderKey(sprite, engineFlags);
                if (!_pipelines.ContainsKey(shaderKey))
                    _pipelines.Add(shaderKey, BuildPipeline(gd, c.SceneContext, shaderKey));

                uint bufferSize = (uint)sprite.Instances.Length * SpriteInstanceData.StructSize;
                var buffer = objectManager.GetDeviceObject<DeviceBuffer>((sprite, sprite, "InstanceBuffer"));
                if (buffer?.SizeInBytes != bufferSize)
                {
                    buffer = gd.ResourceFactory.CreateBuffer(new BufferDescription(bufferSize, BufferUsage.VertexBuffer));
                    buffer.Name = $"B_SpriteInst:{sprite.Name}";
                    PerfTracker.IncrementFrameCounter("Create InstanceBuffer");
                    objectManager.SetDeviceObject((sprite, sprite, "InstanceBuffer"), buffer);
                }

                if (sprite.InstancesDirty)
                {
                    var instances = sprite.Instances;
                    VeldridUtil.UpdateBufferSpan(cl, buffer, instances);
                    PerfTracker.IncrementFrameCounter("Update InstanceBuffers");
                }

                textureManager?.PrepareTexture(sprite.Key.Texture, context);
                TextureView textureView = (TextureView)textureManager?.GetTexture(sprite.Key.Texture);

                var uniformBuffer = objectManager.GetDeviceObject<DeviceBuffer>((sprite, textureView, "UniformBuffer"));
                if (uniformBuffer == null)
                {
                    uniformBuffer = gd.ResourceFactory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<SpriteUniformInfo>(), BufferUsage.UniformBuffer));
                    uniformBuffer.Name = "SpriteUniformBuffer";
                    PerfTracker.IncrementFrameCounter("Create sprite uniform buffer");
                    objectManager.SetDeviceObject((sprite, textureView, "UniformBuffer"), uniformBuffer);
                }

                var uniformInfo = new SpriteUniformInfo
                {
                    Flags = sprite.Key.Flags,
                    TextureWidth = textureView?.Target.Width ?? 1,
                    TextureHeight = textureView?.Target.Height ?? 1
                };
                cl.UpdateBuffer(uniformBuffer, 0, uniformInfo);

                var resourceSet = objectManager.GetDeviceObject<ResourceSet>((sprite, textureView, "ResourceSet"));
                if (resourceSet == null)
                {
                    resourceSet = gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                        _perSpriteResourceLayout,
                        gd.PointSampler,
                        textureView,
                        uniformBuffer));
                    resourceSet.Name = $"RS_Sprite:{sprite.Key.Texture.Name}";
                    PerfTracker.IncrementFrameCounter("Create ResourceSet");
                    objectManager.SetDeviceObject((sprite, textureView, "ResourceSet"), resourceSet);
                }

                sprite.InstancesDirty = false;
                results.Add(sprite);
                cl.PopDebugGroup();
            }

            Resolve<ISpriteManager>().Cleanup();
        }

        public void Render(IRendererContext context, RenderPasses renderPass, IRenderable renderable)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (renderable == null) throw new ArgumentNullException(nameof(renderable));

            var c = (VeldridRendererContext)context;
            var cl = c.CommandList;
            var dom = Resolve<IDeviceObjectManager>();
            var engineFlags = Resolve<IEngineSettings>().Flags;
            var textureManager = Resolve<ITextureManager>();

            var sprite = (MultiSprite)renderable;
            var shaderKey = new SpriteShaderKey(sprite, engineFlags);
            sprite.PipelineId = shaderKey.GetHashCode();

            //if (!shaderKey.UseArrayTexture)
            //    return;

            cl.PushDebugGroup(sprite.Name);

            if (c.SceneContext.PaletteView == null)
                return;

            TextureView textureView = (TextureView)textureManager?.GetTexture(sprite.Key.Texture);
            var resourceSet = dom.GetDeviceObject<ResourceSet>((sprite, textureView, "ResourceSet"));
            var instanceBuffer = dom.GetDeviceObject<DeviceBuffer>((sprite, sprite, "InstanceBuffer"));

            if (sprite.Key.ScissorRegion.HasValue)
            {
                IWindowManager wm = Resolve<IWindowManager>();
                var screenCoordinates = wm.UiToPixel(sprite.Key.ScissorRegion.Value);
                cl.SetScissorRect(0, (uint)screenCoordinates.X, (uint)screenCoordinates.Y, (uint)screenCoordinates.Width, (uint)screenCoordinates.Height);
            }

            cl.SetPipeline(_pipelines[shaderKey]);
            cl.SetGraphicsResourceSet(0, resourceSet);
            cl.SetGraphicsResourceSet(1, c.SceneContext.CommonResourceSet);
            cl.SetVertexBuffer(0, _vertexBuffer);
            cl.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
            cl.SetVertexBuffer(1, instanceBuffer);

            //cl.SetViewport(0, new Viewport(0, 0, sc.MainSceneColorTexture.Width, sc.MainSceneColorTexture.Height, depth, depth));
            cl.DrawIndexed((uint)Indices.Length, (uint)sprite.ActiveInstances, 0, 0, 0);
            //cl.SetViewport(0, new Viewport(0, 0, sc.MainSceneColorTexture.Width, sc.MainSceneColorTexture.Height, 0, 1));
            if (sprite.Key.ScissorRegion.HasValue)
                cl.SetFullScissorRect(0);
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

