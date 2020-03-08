using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using UAlbion.Api;
using UAlbion.Core.Textures;
using UAlbion.Core.Visual;
using Veldrid;
using Veldrid.Utilities;

namespace UAlbion.Core.Veldrid.Visual
{
    public class ExtrudedTileMapRenderer : Component, IRenderer
    {
        public ExtrudedTileMapRenderer() : base(null) { }

        // Vertex Layout
        static readonly VertexLayoutDescription VertexLayout = VertexLayoutHelper.Vertex3DTextured;

        // Instance Layout
        static readonly VertexLayoutDescription InstanceLayout = new VertexLayoutDescription(
            VertexLayoutHelper.Vector2D("TilePosition"), // 2
            VertexLayoutHelper.Int("Textures"), // 3
            VertexLayoutHelper.Int("Flags"), // 4
            VertexLayoutHelper.Vector2D("WallSize") // 5
        ) { InstanceStepRate = 1 };

        static readonly ResourceLayoutDescription PerSpriteLayoutDescription = new ResourceLayoutDescription(
            ResourceLayoutHelper.UniformV("vdspv_0_0"),  // Misc Uniform Data
            ResourceLayoutHelper.Sampler("vdspv_0_1"),  // Point Sampler
            ResourceLayoutHelper.Sampler("vdspv_0_2"),  // Texture Sampler
            ResourceLayoutHelper.Texture("vdspv_0_3"),  // Floors
            ResourceLayoutHelper.Texture("vdspv_0_4")); // Walls

        const string VertexShaderName = "ExtrudedTileMapSV.vert";
        const string FragmentShaderName = "ExtrudedTileMapSF.frag";

        static readonly Vertex3DTextured[] Vertices =
        {
            // Floor (facing inward)
            new Vertex3DTextured(-0.5f, 0.0f,  0.5f, 0.0f, 1.0f), //  0 Bottom Front Left
            new Vertex3DTextured( 0.5f, 0.0f,  0.5f, 1.0f, 1.0f), //  1 Bottom Front Right
            new Vertex3DTextured(-0.5f, 0.0f, -0.5f, 0.0f, 0.0f), //  2 Bottom Back Left
            new Vertex3DTextured( 0.5f, 0.0f, -0.5f, 1.0f, 0.0f), //  3 Bottom Back Right

            // Ceiling (facing inward)
            new Vertex3DTextured(-0.5f, 1.0f,  0.5f, 0.0f, 0.0f), //  4 Top Front Left
            new Vertex3DTextured( 0.5f, 1.0f,  0.5f, 1.0f, 0.0f), //  5 Top Front Right
            new Vertex3DTextured(-0.5f, 1.0f, -0.5f, 0.0f, 1.0f), //  6 Top Back Left
            new Vertex3DTextured( 0.5f, 1.0f, -0.5f, 1.0f, 1.0f), //  7 Top Back Right

            // Back (facing outward)
            new Vertex3DTextured(-0.5f, 1.0f, -0.5f, 0.0f, 0.0f), //  8 Back Top Right
            new Vertex3DTextured( 0.5f, 1.0f, -0.5f, 1.0f, 0.0f), //  9 Back Top Left
            new Vertex3DTextured(-0.5f, 0.0f, -0.5f, 0.0f, 1.0f), // 10 Back Bottom Right
            new Vertex3DTextured( 0.5f, 0.0f, -0.5f, 1.0f, 1.0f), // 11 Back Bottom Left

            // Front (facing outward)
            new Vertex3DTextured( 0.5f, 1.0f,  0.5f, 0.0f, 0.0f), // 12 Front Top Left
            new Vertex3DTextured(-0.5f, 1.0f,  0.5f, 1.0f, 0.0f), // 13 Front Top Right
            new Vertex3DTextured( 0.5f, 0.0f,  0.5f, 0.0f, 1.0f), // 14 Front Bottom Left
            new Vertex3DTextured(-0.5f, 0.0f,  0.5f, 1.0f, 1.0f), // 15 Front Bottom Right

            // Left (facing outward)
            new Vertex3DTextured(-0.5f, 1.0f,  0.5f, 0.0f, 0.0f), // 16 Back Top Left
            new Vertex3DTextured(-0.5f, 1.0f, -0.5f, 1.0f, 0.0f), // 17 Front Top Left
            new Vertex3DTextured(-0.5f, 0.0f,  0.5f, 0.0f, 1.0f), // 18 Back Bottom Left
            new Vertex3DTextured(-0.5f, 0.0f, -0.5f, 1.0f, 1.0f), // 19 Back Front Left

            // Right (facing outward)
            new Vertex3DTextured( 0.5f, 1.0f, -0.5f, 0.0f, 0.0f), // 20 Front Top Right
            new Vertex3DTextured( 0.5f, 1.0f,  0.5f, 1.0f, 0.0f), // 21 Back Top Right
            new Vertex3DTextured( 0.5f, 0.0f, -0.5f, 0.0f, 1.0f), // 22 Front Bottom Right
            new Vertex3DTextured( 0.5f, 0.0f,  0.5f, 1.0f, 1.0f), // 23 Back Bottom Right
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

        public bool CanRender(Type renderable) => renderable == typeof(TileMap);
        public RenderPasses RenderPasses => RenderPasses.Standard;
        readonly DisposeCollector _disposeCollector = new DisposeCollector();
        readonly IList<DeviceBuffer> _instanceBuffers = new List<DeviceBuffer>();
        readonly IList<ResourceSet> _resourceSets = new List<ResourceSet>();
        DeviceBuffer _vb;
        DeviceBuffer _ib;
        DeviceBuffer _miscUniformBuffer;
        Pipeline _pipeline;
        ResourceLayout _layout;
        Sampler _textureSampler;
        Shader[] _shaders;

        public struct MiscUniformData
        {
            public static readonly uint SizeInBytes = (uint)Unsafe.SizeOf<MiscUniformData>();
            public Vector3 Position { get; set; } // 12 bytes
            public int Unused1 { get; set; } // Need another 8 bytes to reach a multiple of 16.
            public Vector3 TileSize { get; set; } // +12 bytes = 24
            public int Unused2 { get; set; } // Need another 8 bytes to reach a multiple of 16.
        }

        public void CreateDeviceObjects(IRendererContext context)
        {
            var c = (VeldridRendererContext) context;
            var cl = c.CommandList;
            var gd = c.GraphicsDevice;
            var sc = c.SceneContext;

            ResourceFactory factory = gd.ResourceFactory;

            _vb = factory.CreateBuffer(new BufferDescription(Vertices.SizeInBytes(), BufferUsage.VertexBuffer));
            _ib = factory.CreateBuffer(new BufferDescription(Indices.SizeInBytes(), BufferUsage.IndexBuffer));
            _miscUniformBuffer = factory.CreateBuffer(new BufferDescription(MiscUniformData.SizeInBytes, BufferUsage.UniformBuffer));
            _vb.Name = "TileMapVertexBuffer";
            _ib.Name = "TileMapIndexBuffer";
            _miscUniformBuffer.Name = "TileMapMiscBuffer";
            cl.UpdateBuffer(_vb, 0, Vertices);
            cl.UpdateBuffer(_ib, 0, Indices);

            var shaderCache = Resolve<IShaderCache>();
            _shaders = shaderCache.GetShaderPair(gd.ResourceFactory,
                VertexShaderName,
                FragmentShaderName,
                shaderCache.GetGlsl(VertexShaderName),
                shaderCache.GetGlsl(FragmentShaderName));

            var shaderSet = new ShaderSetDescription(new[] { VertexLayout, InstanceLayout }, _shaders);

            _layout = factory.CreateResourceLayout(PerSpriteLayoutDescription);
            _textureSampler = gd.ResourceFactory.CreateSampler(new SamplerDescription(
                SamplerAddressMode.Clamp,
                SamplerAddressMode.Clamp,
                SamplerAddressMode.Clamp,
                SamplerFilter.MinPoint_MagPoint_MipPoint,
                null, 1, 0, 0, 0, SamplerBorderColor.TransparentBlack
            ));

            var depthStencilMode = gd.IsDepthRangeZeroToOne
                    ? DepthStencilStateDescription.DepthOnlyLessEqual
                    : DepthStencilStateDescription.DepthOnlyGreaterEqual;

            var rasterizerMode = new RasterizerStateDescription(
                FaceCullMode.Front, PolygonFillMode.Solid, FrontFace.Clockwise,
                true, true);

            var pd = new GraphicsPipelineDescription(
                BlendStateDescription.SingleAlphaBlend,
                depthStencilMode,
                rasterizerMode,
                PrimitiveTopology.TriangleList,
                new ShaderSetDescription(new[] { VertexLayout, InstanceLayout },
                    shaderSet.Shaders,
                    ShaderHelper.GetSpecializations(gd)),
                new[] { _layout, sc.CommonResourceLayout },
                sc.MainSceneFramebuffer.OutputDescription);

            _pipeline = factory.CreateGraphicsPipeline(ref pd);
            _pipeline.Name = "P_TileMapRenderer";
            _disposeCollector.Add(_vb, _ib, _layout, _textureSampler, _pipeline);
        }

        public IEnumerable<IRenderable> UpdatePerFrameResources(IRendererContext context, IEnumerable<IRenderable> renderables)
        {
            var c = (VeldridRendererContext) context;
            var cl = c.CommandList;
            var gd = c.GraphicsDevice;
            var sc = c.SceneContext;

            ITextureManager textureManager = Resolve<ITextureManager>();
            foreach (var buffer in _instanceBuffers)
                buffer.Dispose();
            _instanceBuffers.Clear();

            foreach (var resourceSet in _resourceSets)
                resourceSet.Dispose();
            _resourceSets.Clear();

            void UpdateTilemapWindow(TileMapWindow window)
            {
                var tilemap = window.TileMap;
                window.InstanceBufferId = _instanceBuffers.Count;
                var buffer = gd.ResourceFactory.CreateBuffer(new BufferDescription((uint)window.Length * TileMap.Tile.StructSize, BufferUsage.VertexBuffer));
                buffer.Name = $"B_Tile3DInst{_instanceBuffers.Count}";
                cl.UpdateBuffer(buffer, 0, ref tilemap.Tiles[window.Offset], TileMap.Tile.StructSize * (uint)window.Length);
                _instanceBuffers.Add(buffer);

                textureManager.PrepareTexture(tilemap.Floors, context);
                textureManager.PrepareTexture(tilemap.Walls, context);
            }

            foreach (var tilemap in renderables.OfType<TileMap>())
            {
                var window = new TileMapWindow(tilemap, 0, tilemap.Tiles.Length);
                UpdateTilemapWindow(window);
                yield return window;
            }

            foreach (var window in renderables.OfType<TileMapWindow>())
            {
                UpdateTilemapWindow(window);
                yield return window;
            }
        }

        public void Render(IRendererContext context, RenderPasses renderPass, IRenderable renderable)
        {
            var c = (VeldridRendererContext) context;
            var cl = c.CommandList;
            var gd = c.GraphicsDevice;
            var sc = c.SceneContext;

            ITextureManager textureManager = Resolve<ITextureManager>();
            var window = renderable as TileMapWindow;
            if (window == null)
                return;

            var tilemap = window.TileMap;
            cl.PushDebugGroup($"Tiles3D:{tilemap.Name}:{tilemap.RenderOrder}");
            TextureView floors = (TextureView)textureManager.GetTexture(tilemap.Floors);
            TextureView walls = (TextureView)textureManager.GetTexture(tilemap.Walls);

            var miscUniformData = new MiscUniformData { Position = tilemap.Position, TileSize = tilemap.TileSize, Unused1 = 0, Unused2 = 0 };
            cl.UpdateBuffer(_miscUniformBuffer, 0, miscUniformData);

            var resourceSet = gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(_layout,
                _miscUniformBuffer,
                gd.PointSampler,
                _textureSampler,
                floors,
                walls));
            resourceSet.Name = $"RS_TileMap:{tilemap.Name}";
            _resourceSets.Add(resourceSet);

            cl.SetPipeline(_pipeline);
            cl.SetGraphicsResourceSet(0, resourceSet);
            cl.SetGraphicsResourceSet(1, sc.CommonResourceSet);
            cl.SetVertexBuffer(0, _vb);
            cl.SetVertexBuffer(1, _instanceBuffers[window.InstanceBufferId]);
            cl.SetIndexBuffer(_ib, IndexFormat.UInt16);

            cl.DrawIndexed((uint)Indices.Length, (uint)window.Length, 0, 0, 0);
            cl.PopDebugGroup();
        }

        public void DestroyDeviceObjects()
        {
            foreach (var buffer in _instanceBuffers)
                buffer.Dispose();
            _instanceBuffers.Clear();

            foreach (var resourceSet in _resourceSets)
                resourceSet.Dispose();
            _resourceSets.Clear();

            if (_shaders != null)
                foreach (var shader in _shaders)
                    shader.Dispose();
            _shaders = null;

            _disposeCollector.DisposeAll();
        }

        public void Dispose() => DestroyDeviceObjects();
    }
}
