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

    public sealed class SkyboxRenderer : Component, IRenderer
    {
        // Vertex Layout
        static readonly VertexLayoutDescription VertexLayout = VertexLayoutHelper.Vertex2DTextured;

        // Resource Sets
        static readonly ResourceLayoutDescription ResourceLayoutDescription = new ResourceLayoutDescription(
            ResourceLayoutHelper.Sampler("vdspv_0_0"), // Sampler
            ResourceLayoutHelper.Texture("vdspv_0_1"), // Texture
            ResourceLayoutHelper.Uniform("vdspv_0_2") // Per-draw call uniform buffer
        );

        static readonly ushort[] Indices = { 0, 1, 2, 2, 1, 3 };
        static readonly Vertex2DTextured[] Vertices =
        {
            new Vertex2DTextured(-1.0f, -1.0f, 0.0f, 0.0f), new Vertex2DTextured(1.0f, -1.0f, 1.0f, 0.0f),
            new Vertex2DTextured(-1.0f, 1.0f, 0.0f, 1.0f), new Vertex2DTextured(1.0f, 1.0f, 1.0f, 1.0f),
        };

        readonly DisposeCollector _disposeCollector = new DisposeCollector();
        readonly List<Shader> _shaders = new List<Shader>();
        Pipeline _pipeline;

#pragma warning disable CA2213 // Disposable fields should be disposed
        // Context objects - disposed by the dispose collector.
        DeviceBuffer _vertexBuffer;
        DeviceBuffer _indexBuffer;
        DeviceBuffer _uniformBuffer;
        ResourceLayout _resourceLayout;
#pragma warning restore CA2213 // Disposable fields should be disposed

        public bool CanRender(Type renderable) => renderable == typeof(SkyboxRenderable);
        public RenderPasses RenderPasses => RenderPasses.Standard;

        Pipeline BuildPipeline(GraphicsDevice gd, SceneContext sc)
        {
            var shaderCache = Resolve<IShaderCache>();
            var vertexShaderName = "SkyBoxSV.vert";
            var fragmentShaderName = "SkyBoxSF.frag";
            var vertexShaderContent = shaderCache.GetGlsl(vertexShaderName);
            var fragmentShaderContent = shaderCache.GetGlsl(fragmentShaderName);

            var shaders = shaderCache.GetShaderPair(
                gd.ResourceFactory,
                vertexShaderName, fragmentShaderName,
                vertexShaderContent, fragmentShaderContent);

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
                    ShaderHelper.GetSpecializations(gd)),
                new[] { _resourceLayout, sc.CommonResourceLayout },
                sc.MainSceneFramebuffer.OutputDescription);

            var pipeline = gd.ResourceFactory.CreateGraphicsPipeline(ref pipelineDescription);
            pipeline.Name = "P_Skybox";
            return pipeline;
        }

        public void CreateDeviceObjects(IRendererContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            var c = (VeldridRendererContext)context;

            ResourceFactory factory = c.GraphicsDevice.ResourceFactory;

            _vertexBuffer = factory.CreateBuffer(new BufferDescription(Vertices.SizeInBytes(), BufferUsage.VertexBuffer));
            _indexBuffer = factory.CreateBuffer(new BufferDescription(Indices.SizeInBytes(), BufferUsage.IndexBuffer));
            _uniformBuffer = factory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<SkyboxUniformInfo>(), BufferUsage.UniformBuffer));
            _vertexBuffer.Name = "SpriteVertexBuffer";
            _indexBuffer.Name = "SpriteIndexBuffer";
            _uniformBuffer.Name = "SpriteUniformBuffer";
            c.CommandList.UpdateBuffer(_vertexBuffer, 0, Vertices);
            c.CommandList.UpdateBuffer(_indexBuffer, 0, Indices);

            _resourceLayout = factory.CreateResourceLayout(ResourceLayoutDescription);
            _disposeCollector.Add(_vertexBuffer, _indexBuffer, _uniformBuffer, _resourceLayout);
            _pipeline = BuildPipeline(c.GraphicsDevice, c.SceneContext);
        }

        public IEnumerable<IRenderable> UpdatePerFrameResources(IRendererContext context, IEnumerable<IRenderable> renderables)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (renderables == null) throw new ArgumentNullException(nameof(renderables));
            var c = (VeldridRendererContext)context;
            var gd = c.GraphicsDevice;
            if(!(renderables.FirstOrDefault() is SkyboxRenderable skybox))
                yield break;

            ITextureManager textureManager = Resolve<ITextureManager>();
            IDeviceObjectManager objectManager = Resolve<IDeviceObjectManager>();

            textureManager?.PrepareTexture(skybox.Texture, context);
            TextureView textureView = (TextureView)textureManager?.GetTexture(skybox.Texture);

            var resourceSet = objectManager.GetDeviceObject<ResourceSet>((skybox, textureView));
            if (resourceSet == null)
            {
                resourceSet = gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                    _resourceLayout,
                    gd.PointSampler,
                    textureView,
                    _uniformBuffer));
                resourceSet.Name = $"RS_Sky:{skybox.Texture.Name}";
                PerfTracker.IncrementFrameCounter("Create ResourceSet");
                objectManager.SetDeviceObject((skybox, textureView), resourceSet);
            }

            yield return skybox;
        }

        public void Render(IRendererContext context, RenderPasses renderPass, IRenderable renderable)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (renderable == null) throw new ArgumentNullException(nameof(renderable));
            var c = (VeldridRendererContext)context;
            var cl = c.CommandList;
            var sc = c.SceneContext;

            ITextureManager textureManager = Resolve<ITextureManager>();
            IDeviceObjectManager dom = Resolve<IDeviceObjectManager>();
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

            TextureView textureView = (TextureView)textureManager?.GetTexture(skybox.Texture);
            var resourceSet = dom.GetDeviceObject<ResourceSet>((skybox, textureView));

            cl.SetPipeline(_pipeline);
            cl.SetGraphicsResourceSet(0, resourceSet);
            cl.SetGraphicsResourceSet(1, sc.CommonResourceSet);
            cl.SetVertexBuffer(0, _vertexBuffer);
            cl.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);

            cl.DrawIndexed((uint)Indices.Length, 1, 0, 0, 0);
            cl.PopDebugGroup();
        }

        public void DestroyDeviceObjects()
        {
            _disposeCollector.DisposeAll();
            _pipeline.Dispose();

            foreach (var shader in _shaders)
                shader.Dispose();
            _shaders.Clear();
        }

        public void Dispose() => DestroyDeviceObjects();
    }
}

