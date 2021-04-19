using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UAlbion.Api;
using UAlbion.Core.Veldrid;
using UAlbion.Core.Veldrid.Visual;
using UAlbion.Core.Visual;
using UAlbion.Game.Entities.Map2D;
using Veldrid;
using Veldrid.Utilities;

namespace UAlbion.Game.Veldrid.Visual
{
    public sealed class InfoOverlayRenderer : VeldridComponent, IRenderer
    {
        static readonly VertexLayoutDescription VertexLayout = VertexLayoutHelper.Vertex2DTextured;

        // Resource Sets
        static readonly ResourceLayoutDescription PerSpriteLayoutDescription = new ResourceLayoutDescription(
            ResourceLayoutHelper.BufferReadOnlyFrag("_Buffer"), // Tile data
            ResourceLayoutHelper.Uniform("_Uniform") // Per-draw call uniform buffer
        );

        static readonly ushort[] Indices = { 0, 1, 2, 2, 1, 3 };
        static readonly Vertex2DTextured[] Vertices =
        {
            new Vertex2DTextured(0, 0.0f, 0.0f, 0.0f), new Vertex2DTextured(1.0f, 0.0f, 1.0f, 0.0f),
            new Vertex2DTextured(0, 1.0f, 0.0f, 1.0f), new Vertex2DTextured(1.0f, 1.0f, 1.0f, 1.0f),
        };

        readonly DisposeCollector _disposeCollector = new DisposeCollector();
        readonly List<Shader> _shaders = new List<Shader>();

#pragma warning disable CA2213 // Analysis doesn't know about DisposeCollector
        // Context objects
        Pipeline _pipeline;
        DeviceBuffer _vertexBuffer;
        DeviceBuffer _indexBuffer;
        ResourceLayout _perSpriteResourceLayout;
#pragma warning restore CA2213 // Analysis doesn't know about DisposeCollector

        public Type[] RenderableTypes => new[] { typeof(InfoOverlay) };
        public RenderPasses RenderPasses => RenderPasses.Standard;
        Pipeline BuildPipeline(VeldridRendererContext context)
        {
            var shaderCache = Resolve<IVeldridShaderCache>();
            var shaderName = "InfoOverlay"; 
            var vertexShaderName = shaderName + "SV.vert";
            var fragmentShaderName = shaderName + "SF.frag";
            var vertexShaderContent = shaderCache.GetGlsl(vertexShaderName);
            var fragmentShaderContent = shaderCache.GetGlsl(fragmentShaderName);

            var shaders = shaderCache.GetShaderPair(
                context.GraphicsDevice.ResourceFactory,
                vertexShaderName, fragmentShaderName,
                vertexShaderContent, fragmentShaderContent);

            _shaders.AddRange(shaders);

            var depthStencilMode =
                context.GraphicsDevice.IsDepthRangeZeroToOne
                    ? DepthStencilStateDescription.DepthOnlyLessEqual
                    : DepthStencilStateDescription.DepthOnlyGreaterEqual;

            var rasterizerMode = new RasterizerStateDescription(
                FaceCullMode.None,
                PolygonFillMode.Solid,
                FrontFace.Clockwise,
                true, // depth test
                true); // scissor test

            var pipelineDescription = new GraphicsPipelineDescription(
                BlendStateDescription.SingleAlphaBlend,
                depthStencilMode,
                rasterizerMode,
                PrimitiveTopology.TriangleList,
                new ShaderSetDescription(new[] { VertexLayout },
                    shaders,
                    ShaderHelper.GetSpecializations(context.GraphicsDevice)),
                new[] { _perSpriteResourceLayout, context.SceneContext.CommonResourceLayout },
                context.GraphicsDevice.SwapchainFramebuffer.OutputDescription);

            var pipeline = context.GraphicsDevice.ResourceFactory.CreateGraphicsPipeline(ref pipelineDescription);
            pipeline.Name = "P_InfoOverlay";
            return pipeline;
        }

        public override void CreateDeviceObjects(VeldridRendererContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            _perSpriteResourceLayout = context.GraphicsDevice.ResourceFactory.CreateResourceLayout(PerSpriteLayoutDescription);
            _pipeline = BuildPipeline(context);
            _vertexBuffer = context.GraphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(Vertices.SizeInBytes(), BufferUsage.VertexBuffer));
            _indexBuffer = context.GraphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(Indices.SizeInBytes(), BufferUsage.IndexBuffer));
            _vertexBuffer.Name = "InfoOverlayVertexBuffer";
            _indexBuffer.Name = "InfoOverlayIndexBuffer";
            context.CommandList.UpdateBuffer(_vertexBuffer, 0, Vertices);
            context.CommandList.UpdateBuffer(_indexBuffer, 0, Indices);

            _disposeCollector.Add(_vertexBuffer, _indexBuffer, _pipeline, _perSpriteResourceLayout);
        }

        public void UpdatePerFrameResources(IRendererContext context, IEnumerable<IRenderable> renderables, IList<IRenderable> results)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (renderables == null) throw new ArgumentNullException(nameof(renderables));
            if (results == null) throw new ArgumentNullException(nameof(results));

            var c = (VeldridRendererContext)context;
            var cl = c.CommandList;
            var gd = c.GraphicsDevice;
            var objectManager = Resolve<IDeviceObjectManager>();

            foreach (var renderable in renderables)
            {
                var overlay = (InfoOverlay)renderable;
                // Only render if an overlay is being shown
                if (overlay.ExamineOpacity + overlay.ManipulateOpacity + overlay.TalkOpacity + overlay.TakeOpacity <= 0)
                    continue;

                cl.PushDebugGroup("Info Overlay");
                var tiles = overlay.Tiles;
                var buffer = objectManager.GetDeviceObject<DeviceBuffer>((overlay, overlay, "Tiles"));
                if (buffer?.SizeInBytes != tiles.Length)
                {
                    var size = 16 * (((uint)tiles.Length + 15) / 16);
                    buffer = gd.ResourceFactory.CreateBuffer(new BufferDescription(size, BufferUsage.StructuredBufferReadOnly, 4));
                    buffer.Name = "B_InfoOverlayTiles";
                    objectManager.SetDeviceObject((overlay, overlay, "Tiles"), buffer);
                    overlay.BufferDirty = true;
                }

                if (overlay.BufferDirty)
                {
                    VeldridUtil.UpdateBufferSpan(cl, buffer, tiles);
                    PerfTracker.IncrementFrameCounter("Update InstanceBuffers");
                }

                var uniformBuffer = objectManager.GetDeviceObject<DeviceBuffer>((overlay, overlay, "UniformBuffer"));
                if (uniformBuffer == null)
                {
                    uniformBuffer = gd.ResourceFactory.CreateBuffer(new BufferDescription(
                        (uint)Unsafe.SizeOf<InfoOverlayUniforms>(), BufferUsage.UniformBuffer));
                    uniformBuffer.Name = "InfoOverlayUniformBuffer";
                    PerfTracker.IncrementFrameCounter("Create info overlay uniform buffer");
                    objectManager.SetDeviceObject((overlay, overlay, "UniformBuffer"), uniformBuffer);
                }

                var uniformInfo = new InfoOverlayUniforms
                {
                    Examine = overlay.ExamineOpacity,
                    Manipulate = overlay.ManipulateOpacity,
                    Talk = overlay.TalkOpacity,
                    Take = overlay.TakeOpacity,
                    Width = overlay.Width,
                    Height = overlay.Height,
                    TileWidth = overlay.TileWidth,
                    TileHeight = overlay.TileHeight,
                };
                cl.UpdateBuffer(uniformBuffer, 0, uniformInfo);

                var resourceSet = objectManager.GetDeviceObject<ResourceSet>((overlay, overlay, "ResourceSet"));
                if (resourceSet == null)
                {
                    resourceSet = gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                        _perSpriteResourceLayout,
                        buffer,
                        uniformBuffer));
                    resourceSet.Name = "RS_InfoOverlay";
                    PerfTracker.IncrementFrameCounter("Create ResourceSet");
                    objectManager.SetDeviceObject((overlay, overlay, "ResourceSet"), resourceSet);
                }

                results.Add(overlay);
                overlay.BufferDirty = false;
                cl.PopDebugGroup();
            }
        }

        public void Render(IRendererContext context, RenderPasses renderPass, IRenderable renderable)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (renderable == null) throw new ArgumentNullException(nameof(renderable));
            var c = (VeldridRendererContext)context;
            var cl = c.CommandList;
            var dom = Resolve<IDeviceObjectManager>();

            var overlay = (InfoOverlay)renderable;
            cl.PushDebugGroup(overlay.Name);

            var resourceSet = dom.GetDeviceObject<ResourceSet>((overlay, overlay, "ResourceSet"));

            cl.SetPipeline(_pipeline);
            cl.SetGraphicsResourceSet(0, resourceSet);
            cl.SetGraphicsResourceSet(1, c.SceneContext.CommonResourceSet);
            cl.SetVertexBuffer(0, _vertexBuffer);
            cl.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
            cl.DrawIndexed((uint)Indices.Length);
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

