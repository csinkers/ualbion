using System;
using System.Collections.Generic;
using UAlbion.Api;
using UAlbion.Core.Textures;
using UAlbion.Core.Visual;
using Veldrid;
using Veldrid.Utilities;

#pragma warning disable CA2213 // Analysis doesn't know about dispose collector
namespace UAlbion.Core.Veldrid.Visual
{
    public sealed class ExtrudedTileMapRenderer : VeldridComponent, IRenderer
    {
        static (object, object, object) ResourceSetKey(DungeonTilemap tilemap, TextureView texture) => (tilemap, texture, "ResourceSet");
        static (object, object, object) PropertyBufferKey(DungeonTilemap tilemap) => (tilemap, tilemap, "PropertyBuffer");
        static (object, object, object) InstanceBufferKey(DungeonTilemap tilemap) => (tilemap, tilemap, "InstanceBuffer");

        // Instance Layout
        static readonly VertexLayoutDescription InstanceLayout = new VertexLayoutDescription(
            VertexLayoutHelper.UIntElement("iTextures"),
            VertexLayoutHelper.UIntElement("iFlags"),
            VertexLayoutHelper.Vector2D("iWallSize")
        )
        { InstanceStepRate = 1 };

        static readonly ResourceLayoutDescription UniformLayout = new ResourceLayoutDescription(
            ResourceLayoutHelper.UniformV("Properties"), // Property Data
            ResourceLayoutHelper.Texture("DayFloors"),
            ResourceLayoutHelper.Texture("DayWalls"),
            ResourceLayoutHelper.Texture("NightFloors"),
            ResourceLayoutHelper.Texture("NightWalls"),
            ResourceLayoutHelper.Sampler("TextureSampler"));

        const string VertexShaderName = "ExtrudedTileMapSV.vert";
        const string FragmentShaderName = "ExtrudedTileMapSF.frag";

        public Type[] RenderableTypes => new[] { typeof(DungeonTilemap), typeof(TileMapWindow) };
        public RenderPasses RenderPasses => RenderPasses.Standard;
        readonly DisposeCollector _disposeCollector = new DisposeCollector();
        DeviceBuffer _vertexBuffer;
        DeviceBuffer _indexBuffer;
        Pipeline _normalPipeline;
        Pipeline _nonCullingPipeline;
        ResourceLayout _layout;
        Sampler _textureSampler;

        public override void CreateDeviceObjects(VeldridRendererContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            _vertexBuffer = context.GraphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(Cube.Vertices.SizeInBytes(), BufferUsage.VertexBuffer));
            _indexBuffer = context.GraphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(Cube.Indices.SizeInBytes(), BufferUsage.IndexBuffer));
            _vertexBuffer.Name = "TileMapVertexBuffer";
            _indexBuffer.Name = "TileMapIndexBuffer";
            context.CommandList.UpdateBuffer(_vertexBuffer, 0, Cube.Vertices);
            context.CommandList.UpdateBuffer(_indexBuffer, 0, Cube.Indices);

            var shaderCache = Resolve<IVeldridShaderCache>();
            var shaders = shaderCache.GetShaderPair(context.GraphicsDevice.ResourceFactory, VertexShaderName, FragmentShaderName);
            foreach (var shader in shaders)
                _disposeCollector.Add(shader);

            var shaderSet = new ShaderSetDescription(new[] { Cube.VertexLayout, InstanceLayout }, shaders);

            _layout = context.GraphicsDevice.ResourceFactory.CreateResourceLayout(UniformLayout);
            _layout.Name = "RL_Tilemap";

            _textureSampler = context.GraphicsDevice.ResourceFactory.CreateSampler(new SamplerDescription(
                SamplerAddressMode.Clamp,
                SamplerAddressMode.Clamp,
                SamplerAddressMode.Clamp,
                SamplerFilter.MinPoint_MagPoint_MipPoint,
                null, 1, 0, 0, 0, SamplerBorderColor.TransparentBlack
            ));

            _textureSampler.Name = "TS_Tilemap";
            _normalPipeline = BuildPipeline("P_TileMapRenderer", FaceCullMode.Back, context, shaderSet);
            _nonCullingPipeline = BuildPipeline("P_TileMapRendererNoCull", FaceCullMode.None, context, shaderSet);
            _disposeCollector.Add(_vertexBuffer, _indexBuffer, _layout, _textureSampler, _normalPipeline, _nonCullingPipeline);
        }

        Pipeline BuildPipeline(string name, FaceCullMode cullMode, VeldridRendererContext c, ShaderSetDescription shaderSet)
        {
            var rasterizerMode = new RasterizerStateDescription(cullMode, PolygonFillMode.Solid, FrontFace.CounterClockwise, true, false);
            var pipelineDescription = new GraphicsPipelineDescription(
                BlendStateDescription.SingleAlphaBlend,
                DepthStencilStateDescription.DepthOnlyLessEqual,
                rasterizerMode,
                PrimitiveTopology.TriangleList,
                new ShaderSetDescription(new[] { Cube.VertexLayout, InstanceLayout },
                    shaderSet.Shaders,
                    ShaderHelper.GetSpecializations(c.GraphicsDevice)),
                new[] { _layout, c.SceneContext.CommonResourceLayout },
                ((FramebufferSource)c.Framebuffer)?.Framebuffer.OutputDescription
                ?? c.GraphicsDevice.SwapchainFramebuffer.OutputDescription);

            var pipeline = c.GraphicsDevice.ResourceFactory.CreateGraphicsPipeline(ref pipelineDescription);
            pipeline.Name = name;
            return pipeline;
        }

        public void UpdatePerFrameResources(IRendererContext context, IEnumerable<IRenderable> renderables, IList<IRenderable> results)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (renderables == null) throw new ArgumentNullException(nameof(renderables));
            if (results == null) throw new ArgumentNullException(nameof(results));

            var c = (VeldridRendererContext)context;
            var textureManager = Resolve<ITextureManager>();
            var objectManager = Resolve<IDeviceObjectManager>();

            foreach (var renderable in renderables)
            {
                if (!(renderable is DungeonTilemap tilemap)) 
                    continue;

                c.CommandList.PushDebugGroup($"Tiles3D:{tilemap.Name}:{tilemap.RenderOrder}");
                textureManager.PrepareTexture(tilemap.DayFloors, c);
                textureManager.PrepareTexture(tilemap.DayWalls, c);
                if (tilemap.NightFloors != null)
                {
                    textureManager.PrepareTexture(tilemap.NightFloors, c);
                    textureManager.PrepareTexture(tilemap.NightWalls, c);
                }

                UpdateInstanceBuffer(tilemap, objectManager, c);
                UpdateResourceSet(tilemap, objectManager, c);
                c.CommandList.PopDebugGroup();
                results.Add(tilemap);
            }
        }

        public void Render(IRendererContext context, RenderPasses renderPass, IRenderable renderable)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (renderable == null) throw new ArgumentNullException(nameof(renderable));
            if (!(renderable is DungeonTilemap tilemap))
                return;

            var c = (VeldridRendererContext)context;
            var cl = c.CommandList;
            var objectManager = Resolve<IDeviceObjectManager>();

            cl.PushDebugGroup($"Tiles3D:{tilemap.Name}:{tilemap.RenderOrder}");

            var textureManager = Resolve<ITextureManager>();
            var floors = (TextureView)textureManager.GetTexture(tilemap.DayFloors); 
            var instanceBuffer = objectManager.GetDeviceObject<DeviceBuffer>(InstanceBufferKey(tilemap));
            var resourceSet = objectManager.GetDeviceObject<ResourceSet>(ResourceSetKey(tilemap, floors));

            cl.SetPipeline(tilemap.PipelineId == (int)DungeonTilemapPipeline.NoCulling ? _nonCullingPipeline : _normalPipeline);
            cl.SetGraphicsResourceSet(0, resourceSet);
            cl.SetGraphicsResourceSet(1, c.SceneContext.CommonResourceSet);
            cl.SetVertexBuffer(0, _vertexBuffer);
            cl.SetVertexBuffer(1, instanceBuffer);
            cl.SetIndexBuffer(_indexBuffer, Cube.IndexFormat);

            cl.DrawIndexed((uint)Cube.Indices.Length, (uint)tilemap.Tiles.Length, 0, 0, 0);
            cl.PopDebugGroup();
        }

        public override void DestroyDeviceObjects() => _disposeCollector.DisposeAll();
        public void Dispose() => DestroyDeviceObjects();

        static unsafe void UpdateInstanceBuffer(DungeonTilemap tilemap, IDeviceObjectManager objectManager, VeldridRendererContext c)
        {
            var bufferSize = (uint)tilemap.Tiles.Length * DungeonTile.StructSize;
            var buffer = objectManager.GetDeviceObject<DeviceBuffer>(InstanceBufferKey(tilemap));
            if (buffer?.SizeInBytes != bufferSize)
            {
                buffer = c.GraphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(bufferSize, BufferUsage.VertexBuffer));
                buffer.Name = $"B_Inst{tilemap.Name}";
                objectManager.SetDeviceObject(InstanceBufferKey(tilemap), buffer);
                tilemap.TilesDirty = true;
            }

            if (!tilemap.TilesDirty) 
                return;

            fixed (DungeonTile* tile = &tilemap.Tiles[0])
            {
                c.CommandList.UpdateBuffer(buffer, 0, (IntPtr)(&tile[0]), bufferSize);
            }

            tilemap.TilesDirty = false;
        }

        void UpdateResourceSet(DungeonTilemap tilemap, IDeviceObjectManager objectManager, VeldridRendererContext c)
        {
            var (propertiesBuffer, isNew) = GetOrCreatePropertiesBuffer(tilemap, objectManager, c);

            // Need to retrieve textures every time to prevent them being flushed from the cache
            var textureManager = Resolve<ITextureManager>();
            TextureView dayFloors = (TextureView)textureManager.GetTexture(tilemap.DayFloors); 
            TextureView dayWalls = (TextureView)textureManager.GetTexture(tilemap.DayWalls);
            TextureView nightFloors = (TextureView)textureManager.GetTexture(tilemap.NightFloors ?? tilemap.DayFloors); 
            TextureView nightWalls = (TextureView)textureManager.GetTexture(tilemap.NightWalls ?? tilemap.DayWalls);

            if (dayFloors == null || dayWalls == null || nightFloors == null || nightWalls == null)
                throw new InvalidOperationException("Could not locate floor/wall multi-texture");

            var resourceSet = objectManager.GetDeviceObject<ResourceSet>(ResourceSetKey(tilemap, dayFloors));
            if (resourceSet == null || isNew) // Only recreate the resource set if we need to
            {
                resourceSet = c.GraphicsDevice.ResourceFactory.CreateResourceSet(new ResourceSetDescription(_layout,
                    propertiesBuffer,
                    dayFloors,
                    dayWalls,
                    nightFloors,
                    nightWalls,
                    _textureSampler));

                resourceSet.Name = $"RS_TileMap:{tilemap.Name}";
                objectManager.SetDeviceObject(ResourceSetKey(tilemap, dayFloors), resourceSet);
            }
        }

        static (DeviceBuffer, bool) GetOrCreatePropertiesBuffer(DungeonTilemap tilemap, IDeviceObjectManager objectManager, VeldridRendererContext c)
        {
            bool isNew = false;
            var propertiesBuffer = objectManager.GetDeviceObject<DeviceBuffer>(PropertyBufferKey(tilemap));
            if (propertiesBuffer == null)
            {
                propertiesBuffer = c.GraphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(DungeonTileMapProperties.StructSize, BufferUsage.UniformBuffer));
                propertiesBuffer.Name = "B_TileProps:" + tilemap.Name;
                objectManager.SetDeviceObject(PropertyBufferKey(tilemap), propertiesBuffer);
                tilemap.PropertiesDirty = true;
                isNew = true;
            }

            if (tilemap.PropertiesDirty)
            {
                c.CommandList.UpdateBuffer(propertiesBuffer, 0, tilemap.Properties);
                tilemap.PropertiesDirty = false;
            }

            return (propertiesBuffer, isNew);
        }
    }
}
#pragma warning restore CA2213 // Analysis doesn't know about dispose collector
