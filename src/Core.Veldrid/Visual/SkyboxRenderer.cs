using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UAlbion.Api;
using UAlbion.Core.Textures;
using UAlbion.Core.Visual;
using Veldrid;
using Veldrid.Utilities;

namespace UAlbion.Core.Veldrid.Visual
{
    [StructLayout(LayoutKind.Sequential)]
    struct SkyboxUniformInfo // Length must be multiple of 16
    {
        public float uYaw { get; set; } // 4
        public float uPitch { get; set; } // 8
        public float uVisibleProportion { get; set; } // 12
        readonly uint _pad1;   // 16
    }

    public sealed class SkyboxRenderer : VeldridComponent, IRenderer
    {
        // Vertex Layout
        static readonly VertexLayoutDescription VertexLayout = VertexLayoutHelper.Vertex2DTextured;

        // Resource Sets
        static readonly ResourceLayoutDescription ResourceLayoutDescription = new ResourceLayoutDescription(
            ResourceLayoutHelper.Sampler("uSampler"),
            ResourceLayoutHelper.Texture("uTexture"),
            ResourceLayoutHelper.Uniform("_Uniform")
        );

        static readonly ushort[] Indices = { 0, 1, 2, 2, 1, 3 };
        static readonly Vertex2DTextured[] Vertices =
        {
            new Vertex2DTextured(-1.0f, -1.0f, 0.0f, 0.0f), new Vertex2DTextured(1.0f, -1.0f, 1.0f, 0.0f),
            new Vertex2DTextured(-1.0f, 1.0f, 0.0f, 1.0f), new Vertex2DTextured(1.0f, 1.0f, 1.0f, 1.0f),
        };

        readonly DisposeCollector _disposeCollector = new DisposeCollector();
        readonly List<Shader> _shaders = new List<Shader>();

#pragma warning disable CA2213 // Disposable fields should be disposed
        // Context objects - disposed by the dispose collector.
        Pipeline _pipeline;
        DeviceBuffer _vertexBuffer;
        DeviceBuffer _indexBuffer;
        DeviceBuffer _uniformBuffer;
        ResourceLayout _resourceLayout;
#pragma warning restore CA2213 // Disposable fields should be disposed

        public Type[] RenderableTypes => new[] { typeof(SkyboxRenderable) };
        public RenderPasses RenderPasses => RenderPasses.Standard;

        Pipeline BuildPipeline(VeldridRendererContext context)
        {
            var shaderCache = Resolve<IVeldridShaderCache>();

            var shaders = shaderCache.GetShaderPair(context.GraphicsDevice.ResourceFactory, "SkyBoxSV.vert", "SkyBoxSF.frag");
            _shaders.AddRange(shaders);
            var shaderSet = new ShaderSetDescription(new[] { VertexLayout }, shaders);
            var depthStencilMode = DepthStencilStateDescription.Disabled;
            var rasterizerMode = new RasterizerStateDescription(
                FaceCullMode.None,
                PolygonFillMode.Solid,
                FrontFace.Clockwise,
                false, // depth test
                true); // scissor test

            var pipelineDescription = new GraphicsPipelineDescription(
                BlendStateDescription.SingleDisabled,
                depthStencilMode,
                rasterizerMode,
                PrimitiveTopology.TriangleList,
                new ShaderSetDescription(new[] { VertexLayout },
                    shaderSet.Shaders,
                    ShaderHelper.GetSpecializations(context.GraphicsDevice)),
                new[] { _resourceLayout, context.SceneContext.CommonResourceLayout },
                context.GraphicsDevice.SwapchainFramebuffer.OutputDescription);

            var pipeline = context.GraphicsDevice.ResourceFactory.CreateGraphicsPipeline(ref pipelineDescription);
            pipeline.Name = "P_Skybox";
            return pipeline;
        }

        public override void CreateDeviceObjects(VeldridRendererContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            _vertexBuffer = context.GraphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(Vertices.SizeInBytes(), BufferUsage.VertexBuffer));
            _indexBuffer = context.GraphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(Indices.SizeInBytes(), BufferUsage.IndexBuffer));
            _uniformBuffer = context.GraphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<SkyboxUniformInfo>(), BufferUsage.UniformBuffer));
            _vertexBuffer.Name = "SpriteVertexBuffer";
            _indexBuffer.Name = "SpriteIndexBuffer";
            _uniformBuffer.Name = "SpriteUniformBuffer";
            context.CommandList.UpdateBuffer(_vertexBuffer, 0, Vertices);
            context.CommandList.UpdateBuffer(_indexBuffer, 0, Indices);

            _resourceLayout = context.GraphicsDevice.ResourceFactory.CreateResourceLayout(ResourceLayoutDescription);
            _pipeline = BuildPipeline(context);
            _disposeCollector.Add(_vertexBuffer, _indexBuffer, _uniformBuffer, _resourceLayout, _pipeline);
        }

        public void UpdatePerFrameResources(IRendererContext context, IEnumerable<IRenderable> renderables, IList<IRenderable> results)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (renderables == null) throw new ArgumentNullException(nameof(renderables));
            if (results == null) throw new ArgumentNullException(nameof(results));
            var c = (VeldridRendererContext)context;
            if(!(renderables.FirstOrDefault() is SkyboxRenderable skybox))
                return;

            var textureManager = Resolve<ITextureManager>();
            var objectManager = Resolve<IDeviceObjectManager>();

            textureManager.PrepareTexture(skybox.Texture, context);
            TextureView textureView = (TextureView)textureManager.GetTexture(skybox.Texture);

            var resourceSet = objectManager.GetDeviceObject<ResourceSet>((skybox, textureView, null));
            if (resourceSet == null)
            {
                resourceSet = c.GraphicsDevice.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                    _resourceLayout,
                    c.GraphicsDevice.PointSampler,
                    textureView,
                    _uniformBuffer));
                resourceSet.Name = $"RS_Sky:{skybox.Texture.Name}";
                PerfTracker.IncrementFrameCounter("Create ResourceSet");
                objectManager.SetDeviceObject((skybox, textureView, null), resourceSet);
            }

            results.Add(skybox);
        }

        public void Render(IRendererContext context, RenderPasses renderPass, IRenderable renderable)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (renderable == null) throw new ArgumentNullException(nameof(renderable));
            var c = (VeldridRendererContext)context;
            var cl = c.CommandList;
            var sc = c.SceneContext;

            var textureManager = Resolve<ITextureManager>();
            var dom = Resolve<IDeviceObjectManager>();
            var config = Resolve<CoreConfig>().Visual.Skybox;
            if (!(Resolve<ICamera>() is PerspectiveCamera camera))
                return;

            var skybox = (SkyboxRenderable)renderable;

            cl.PushDebugGroup(skybox.Name);

            var uniformInfo = new SkyboxUniformInfo
            {
                uYaw = camera.Yaw,
                uPitch = camera.Pitch,
                uVisibleProportion = config.VisibleProportion
            };

            cl.UpdateBuffer(_uniformBuffer, 0, uniformInfo);

            if (sc.PaletteView == null)
                return;

            var textureView = (TextureView)textureManager.GetTexture(skybox.Texture);
            var resourceSet = dom.GetDeviceObject<ResourceSet>((skybox, textureView, null));

            cl.SetPipeline(_pipeline);
            cl.SetGraphicsResourceSet(0, resourceSet);
            cl.SetGraphicsResourceSet(1, sc.CommonResourceSet);
            cl.SetVertexBuffer(0, _vertexBuffer);
            cl.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);

            cl.DrawIndexed((uint)Indices.Length, 1, 0, 0, 0);
            cl.PopDebugGroup();
        }

        public override void DestroyDeviceObjects()
        {
            _disposeCollector.DisposeAll();

            foreach (var shader in _shaders)
                shader.Dispose();
            _shaders.Clear();
        }

        public void Dispose() => DestroyDeviceObjects();
    }
}

