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
    public sealed class ExtrudedTileMapRenderer : Component, IRenderer
    {
        static (object, object, object) ResourceSetKey(DungeonTilemap tilemap, TextureView texture) => (tilemap, texture, "ResourceSet");
        static (object, object, object) PropertyBufferKey(DungeonTilemap tilemap) => (tilemap, tilemap, "PropertyBuffer");
        static (object, object, object) InstanceBufferKey(DungeonTilemap tilemap) => (tilemap, tilemap, "InstanceBuffer");

        // Vertex Layout
        static readonly VertexLayoutDescription VertexLayout = VertexLayoutHelper.Vertex3DTextured;

        // Instance Layout
        static readonly VertexLayoutDescription InstanceLayout = new VertexLayoutDescription(
            VertexLayoutHelper.UIntElement("iTextures"),
            VertexLayoutHelper.UIntElement("iFlags"),
            VertexLayoutHelper.Vector2D("iWallSize")
        )
        { InstanceStepRate = 1 };

        static readonly ResourceLayoutDescription UniformLayout = new ResourceLayoutDescription(
            ResourceLayoutHelper.UniformV("Properties"), // Property Data
            ResourceLayoutHelper.Texture("Floors"),
            ResourceLayoutHelper.Texture("Walls"),
            ResourceLayoutHelper.Sampler("TextureSampler"));

        const string VertexShaderName = "ExtrudedTileMapSV.vert";
        const string FragmentShaderName = "ExtrudedTileMapSF.frag";

        static readonly Vertex3DTextured[] Vertices = 
        { // Unit cube centred on the middle of the bottom face. Up axis = Y.
            // Floor (facing inward)
            new Vertex3DTextured(-0.5f, -0.5f,  0.5f, 0.0f, 1.0f), //  0 Bottom Front Left
            new Vertex3DTextured( 0.5f, -0.5f,  0.5f, 1.0f, 1.0f), //  1 Bottom Front Right
            new Vertex3DTextured(-0.5f, -0.5f, -0.5f, 0.0f, 0.0f), //  2 Bottom Back Left
            new Vertex3DTextured( 0.5f, -0.5f, -0.5f, 1.0f, 0.0f), //  3 Bottom Back Right

            // Ceiling (facing inward)
            new Vertex3DTextured(-0.5f, 0.5f,  0.5f, 0.0f, 0.0f), //  4 Top Front Left
            new Vertex3DTextured( 0.5f, 0.5f,  0.5f, 1.0f, 0.0f), //  5 Top Front Right
            new Vertex3DTextured(-0.5f, 0.5f, -0.5f, 0.0f, 1.0f), //  6 Top Back Left
            new Vertex3DTextured( 0.5f, 0.5f, -0.5f, 1.0f, 1.0f), //  7 Top Back Right

            // Back (facing outward)
            new Vertex3DTextured(-0.5f,  0.5f, -0.5f, 1.0f, 0.0f), //  8 Back Top Right
            new Vertex3DTextured( 0.5f,  0.5f, -0.5f, 0.0f, 0.0f), //  9 Back Top Left
            new Vertex3DTextured(-0.5f, -0.5f, -0.5f, 1.0f, 1.0f), // 10 Back Bottom Right
            new Vertex3DTextured( 0.5f, -0.5f, -0.5f, 0.0f, 1.0f), // 11 Back Bottom Left

            // Front (facing outward)
            new Vertex3DTextured( 0.5f,  0.5f,  0.5f, 1.0f, 0.0f), // 12 Front Top Left
            new Vertex3DTextured(-0.5f,  0.5f,  0.5f, 0.0f, 0.0f), // 13 Front Top Right
            new Vertex3DTextured( 0.5f, -0.5f,  0.5f, 1.0f, 1.0f), // 14 Front Bottom Left
            new Vertex3DTextured(-0.5f, -0.5f,  0.5f, 0.0f, 1.0f), // 15 Front Bottom Right

            // Left (facing outward)
            new Vertex3DTextured(-0.5f,  0.5f,  0.5f, 1.0f, 0.0f), // 16 Back Top Left
            new Vertex3DTextured(-0.5f,  0.5f, -0.5f, 0.0f, 0.0f), // 17 Front Top Left
            new Vertex3DTextured(-0.5f, -0.5f,  0.5f, 1.0f, 1.0f), // 18 Back Bottom Left
            new Vertex3DTextured(-0.5f, -0.5f, -0.5f, 0.0f, 1.0f), // 19 Back Front Left

            // Right (facing outward)
            new Vertex3DTextured( 0.5f,  0.5f, -0.5f, 0.0f, 0.0f), // 20 Front Top Right
            new Vertex3DTextured( 0.5f,  0.5f,  0.5f, 1.0f, 0.0f), // 21 Back Top Right
            new Vertex3DTextured( 0.5f, -0.5f, -0.5f, 0.0f, 1.0f), // 22 Front Bottom Right
            new Vertex3DTextured( 0.5f, -0.5f,  0.5f, 1.0f, 1.0f), // 23 Back Bottom Right
        };

        static readonly ushort[] Indices =
        {
             0,  1,  2,  2,  1,  3, // Floor
             6,  5,  4,  7,  5,  6, // Ceiling
             8,  9, 10, 10,  9, 11, // Back
            12, 13, 14, 14, 13, 15, // Front
            16, 17, 18, 18, 17, 19, // Left
            20, 21, 22, 22, 21, 23, // Right
        };

        public Type[] RenderableTypes => new[] { typeof(DungeonTilemap), typeof(TileMapWindow) };
        public RenderPasses RenderPasses => RenderPasses.Standard;
        readonly DisposeCollector _disposeCollector = new DisposeCollector();
        DeviceBuffer _vb;
        DeviceBuffer _ib;
        Pipeline _normalPipeline;
        Pipeline _nonCullingPipeline;
        ResourceLayout _layout;
        Sampler _textureSampler;

        public void CreateDeviceObjects(IRendererContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            var c = (VeldridRendererContext)context;
            var cl = c.CommandList;
            var gd = c.GraphicsDevice;
            var sc = c.SceneContext;

            ResourceFactory factory = gd.ResourceFactory;

            _vb = factory.CreateBuffer(new BufferDescription(Vertices.SizeInBytes(), BufferUsage.VertexBuffer));
            _ib = factory.CreateBuffer(new BufferDescription(Indices.SizeInBytes(), BufferUsage.IndexBuffer));
            _vb.Name = "TileMapVertexBuffer";
            _ib.Name = "TileMapIndexBuffer";
            cl.UpdateBuffer(_vb, 0, Vertices);
            cl.UpdateBuffer(_ib, 0, Indices);

            var shaderCache = Resolve<IVeldridShaderCache>();
            var shaders = shaderCache.GetShaderPair(gd.ResourceFactory, VertexShaderName, FragmentShaderName);
            foreach (var shader in shaders)
                _disposeCollector.Add(shader);

            var shaderSet = new ShaderSetDescription(new[] { VertexLayout, InstanceLayout }, shaders);

            _layout = factory.CreateResourceLayout(UniformLayout);
            _layout.Name = "RL_Tilemap";

            _textureSampler = gd.ResourceFactory.CreateSampler(new SamplerDescription(
                SamplerAddressMode.Clamp,
                SamplerAddressMode.Clamp,
                SamplerAddressMode.Clamp,
                SamplerFilter.MinPoint_MagPoint_MipPoint,
                null, 1, 0, 0, 0, SamplerBorderColor.TransparentBlack
            ));
            _textureSampler.Name = "TS_Tilemap";

            _normalPipeline = BuildPipeline("P_TileMapRenderer", FaceCullMode.Back, gd, sc, shaderSet);
            _nonCullingPipeline = BuildPipeline("P_TileMapRendererNoCull", FaceCullMode.None, gd, sc, shaderSet);
            _disposeCollector.Add(_vb, _ib, _layout, _textureSampler, _normalPipeline, _nonCullingPipeline);
        }

        Pipeline BuildPipeline(string name, FaceCullMode cullMode, GraphicsDevice gd, SceneContext sc, ShaderSetDescription shaderSet)
        {
            var depthStencilMode = gd.IsDepthRangeZeroToOne
                    ? DepthStencilStateDescription.DepthOnlyLessEqual
                    : DepthStencilStateDescription.DepthOnlyGreaterEqual;

            var rasterizerMode = new RasterizerStateDescription(cullMode, PolygonFillMode.Solid, FrontFace.CounterClockwise, true, false);
            var pipelineDescription = new GraphicsPipelineDescription(
                BlendStateDescription.SingleAlphaBlend,
                depthStencilMode,
                rasterizerMode,
                PrimitiveTopology.TriangleList,
                new ShaderSetDescription(new[] { VertexLayout, InstanceLayout },
                    shaderSet.Shaders,
                    ShaderHelper.GetSpecializations(gd)),
                new[] { _layout, sc.CommonResourceLayout },
                gd.SwapchainFramebuffer.OutputDescription);

            var pipeline = gd.ResourceFactory.CreateGraphicsPipeline(ref pipelineDescription);
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
                textureManager.PrepareTexture(tilemap.Floors, c);
                textureManager.PrepareTexture(tilemap.Walls, c);
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
            TextureView floors = (TextureView)textureManager.GetTexture(tilemap.Floors); 
            var instanceBuffer = objectManager.GetDeviceObject<DeviceBuffer>(InstanceBufferKey(tilemap));
            var resourceSet = objectManager.GetDeviceObject<ResourceSet>(ResourceSetKey(tilemap, floors));

            cl.SetPipeline(tilemap.PipelineId == (int)DungeonTilemapPipeline.NoCulling ? _nonCullingPipeline : _normalPipeline);
            cl.SetGraphicsResourceSet(0, resourceSet);
            cl.SetGraphicsResourceSet(1, c.SceneContext.CommonResourceSet);
            cl.SetVertexBuffer(0, _vb);
            cl.SetVertexBuffer(1, instanceBuffer);
            cl.SetIndexBuffer(_ib, IndexFormat.UInt16);

            cl.DrawIndexed((uint)Indices.Length, (uint)tilemap.Tiles.Length, 0, 0, 0);
            cl.PopDebugGroup();
        }

        public void DestroyDeviceObjects() => _disposeCollector.DisposeAll();
        public void Dispose() => DestroyDeviceObjects();

        static void UpdateInstanceBuffer(DungeonTilemap tilemap, IDeviceObjectManager objectManager, VeldridRendererContext c)
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

            unsafe
            {
                fixed (DungeonTile* tile = &tilemap.Tiles[0])
                {
                    c.CommandList.UpdateBuffer(buffer, 0, (IntPtr)(&tile[0]), bufferSize);
                }
            }

            tilemap.TilesDirty = false;
        }

        void UpdateResourceSet(DungeonTilemap tilemap, IDeviceObjectManager objectManager, VeldridRendererContext c)
        {
            var (propertiesBuffer, isNew) = GetOrCreatePropertiesBuffer(tilemap, objectManager, c);

            // Need to retrieve textures every time to prevent them being flushed from the cache
            var textureManager = Resolve<ITextureManager>();
            TextureView floors = (TextureView)textureManager.GetTexture(tilemap.Floors); 
            TextureView walls = (TextureView)textureManager.GetTexture(tilemap.Walls);
            if (floors == null || walls == null)
                throw new InvalidOperationException("Could not locate floor/wall multi-texture");

            var resourceSet = objectManager.GetDeviceObject<ResourceSet>(ResourceSetKey(tilemap, floors));
            if (resourceSet == null || isNew) // Only recreate the resource set if we need to
            {
                resourceSet = c.GraphicsDevice.ResourceFactory.CreateResourceSet(new ResourceSetDescription(_layout,
                    propertiesBuffer,
                    floors,
                    walls,
                    _textureSampler));

                resourceSet.Name = $"RS_TileMap:{tilemap.Name}";
                objectManager.SetDeviceObject(ResourceSetKey(tilemap, floors), resourceSet);
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
