using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using UAlbion.Core.Textures;
using Veldrid;
using Veldrid.SPIRV;
using Veldrid.Utilities;

namespace UAlbion.Core.Visual
{
    public class ExtrudedTileMapRenderer : Component, IRenderer
    {
        public ExtrudedTileMapRenderer() : base(null) { }

        // Vertex Layout
        static readonly VertexLayoutDescription VertexLayout = Vertex3DTextured.VertexLayout;

        // Instance Layout
        static readonly VertexLayoutDescription InstanceLayout = new VertexLayoutDescription(
            VertexLayoutHelper.Vector2D("TilePosition"), // 2
            VertexLayoutHelper.Int("Textures"), // 3
            VertexLayoutHelper.Int("Flags"), // 4
            VertexLayoutHelper.Vector2D("WallSize") // 5
        ) { InstanceStepRate = 1 };

        static readonly ResourceLayoutDescription PerSpriteLayoutDescription = new ResourceLayoutDescription(
            ResourceLayoutHelper.Uniform("vdspv_0_0"),  // Projection Matrix
            ResourceLayoutHelper.Uniform("vdspv_0_1"),  // View Matrix
            ResourceLayoutHelper.Uniform("vdspv_0_2"),  // Misc Uniform Data
            ResourceLayoutHelper.Sampler("vdspv_0_3"),  // Point Sampler
            ResourceLayoutHelper.Texture("vdspv_0_4"),  // Palette
            ResourceLayoutHelper.Sampler("vdspv_0_5"),  // Texture Sampler
            ResourceLayoutHelper.Texture("vdspv_0_6"),  // Floors
            ResourceLayoutHelper.Texture("vdspv_0_7")); // Walls

        const string VertexShader = @"
            #version 450

            // Resource Sets / Uniforms
            layout(set = 0, binding = 0) uniform _Projection { mat4 Projection; }; // vdspv_0_0
            layout(set = 0, binding = 1) uniform _View       { mat4 View; };       // vdspv_0_1
            layout(set = 0, binding = 2) uniform _Misc       { vec3 Position; int Unused1; vec3 TileSize; int Unused2; }; // vdspv_0_2
            // TODO: Lighting info

            // Vertex Data
            layout(location = 0) in vec3 _VertexPosition; // N.B. Tile origins are in the middle of the floor.
            layout(location = 1) in vec2 _TexCoords;

            // Instance Data
            layout(location = 2) in vec2 _TilePosition; // X & Z, in tiles
            layout(location = 3) in uint _Textures; // Floor, Ceiling, Walls, Overlay - 1 byte each, 0 = transparent / off
            layout(location = 4) in uint _Flags; // Bits 2 - 31 are instance flags, 0 & 1 denote texture type.
            layout(location = 5) in vec2 _WallSize; // U & W, normalised

            // Outputs
            layout(location = 0) out vec2 fsin_0;     // Texture Coordinates
            layout(location = 1) out flat uint fsin_1; // Textures
            layout(location = 2) out flat uint fsin_2; // Flags, bits 0-1 = tex type

            void main()
            {
                uint textureId = 2;
                if (gl_VertexIndex < 4) textureId = 0;
                else if (gl_VertexIndex < 8) textureId = 1;

                if(textureId == 2)
                    fsin_0 = _TexCoords * _WallSize;
                else
                    fsin_0 = _TexCoords;
                fsin_1 = _Textures;
                fsin_2 = (_Flags & 0xfffffffc) | textureId; // | ((gl_InstanceIndex & 4) == 0 ? 256 : 0);

                if(    (textureId == 0 && ((fsin_1 & 0x000000ff) == 0))
                    || (textureId == 1 && ((fsin_1 & 0x0000ff00) == 0))
                    || (textureId == 2 && ((fsin_1 & 0x00ff0000) == 0))
                )
                    gl_Position = vec4(0, 1e12, 0, 1); // Inactive faces/vertices get relegated to waaaay above the origin
                else
                    gl_Position = Projection * View * vec4(Position + ( _VertexPosition + vec3(_TilePosition.x, 0.0f, _TilePosition.y)) * TileSize, 1);
            }";

        const string FragmentShader = @"
            #version 450

            // Resource Sets / Uniforms
            layout(set = 0, binding = 3) uniform sampler PaletteSampler;  // vdspv_0_3
            layout(set = 0, binding = 4) uniform texture2D PaletteView;   // vdspv_0_4
            layout(set = 0, binding = 5) uniform sampler TextureSampler;  // vdspv_0_5
            layout(set = 0, binding = 6) uniform texture2DArray Floors;   // vdspv_0_6
            layout(set = 0, binding = 7) uniform texture2DArray Walls;    // vdspv_0_7
            // TODO: Lighting info

            // Vertex & Instance data piped through from vertex shader
            layout(location = 0) in vec2 fsin_0;     // Texture Coordinates
            layout(location = 1) in flat uint fsin_1; // Textures
            layout(location = 2) in flat uint fsin_2; // Flags

            layout(location = 0) out vec4 OutputColor;

            void main()
            {
                float floorLayer   = float(fsin_1 & 0x000000ff);
                float ceilingLayer = float((fsin_1 & 0x0000ff00) >> 8);
                float wallLayer    = float((fsin_1 & 0x00ff0000) >> 16);
                float overlayLayer = float((fsin_1 & 0xff000000) >> 24);

                /* 0&1: 0=Floor 1=Ceiling 2=Walls+Overlay 3=Unused
                   UsePalette =  4, Highlight   =   8,
                   RedTint    = 16, GreenTint   =  32,
                   BlueTint   = 64, Transparent = 128,
                   NoTexture  = 256 */

                vec4 color;
                if ((fsin_2 & 3) == 0)
                    color = texture(sampler2DArray(Floors, TextureSampler), vec3(fsin_0, floorLayer));
                else if ((fsin_2 & 3) == 1)
                    color = texture(sampler2DArray(Floors, TextureSampler), vec3(fsin_0, ceilingLayer));
                else
                    color = texture(sampler2DArray(Walls, TextureSampler), vec3(fsin_0, wallLayer));

                if ((fsin_2 & 4) != 0) // 4=UsePalette
                {
                    float redChannel = color[0];
                    float index = 255.0f * redChannel;
                    if(index == 0)
                        color = vec4(0.0f, 0.0f, 0.0f, 0.0f);
                    else
                        color = texture(sampler2D(PaletteView, PaletteSampler), vec2(redChannel, 0.0f));
                }
                // else if(color.x != 0) color = vec4(color.xx, 0.5f, 1.0f);

                if(color.w == 0.0f)
                    discard;

                if((fsin_2 &   8) != 0) color = color * 1.2; // Highlight
                if((fsin_2 &  16) != 0) color = vec4(color.x * 1.5f + 0.3f, color.yzw);         // Red tint
                if((fsin_2 &  32) != 0) color = vec4(color.x, color.y * 1.5f + 0.3f, color.zw); // Green tint
                if((fsin_2 &  64) != 0) color = vec4(color.xy, color.z * 1.5f + 0.f, color.w);  // Blue tint
                if((fsin_2 & 128) != 0) color = vec4(color.xyz, color.w * 0.5f); // Transparent
                if((fsin_2 & 256) != 0) {
                    if ((fsin_2 & 3) == 0)
                        color = vec4(floorLayer / 255.0f, floorLayer / 255.0f, floorLayer / 255.0f, 1.0f);
                    else if ((fsin_2 & 3) == 1)
                        color = vec4(ceilingLayer / 255.0f, ceilingLayer / 255.0f, ceilingLayer / 255.0f, 1.0f);
                    else
                        color = vec4(wallLayer / 255.0f, wallLayer / 255.0f, wallLayer / 255.0f, 1.0f);
                }

                OutputColor = color;
            }";

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

        public struct MiscUniformData
        {
            public static readonly uint SizeInBytes = (uint)Unsafe.SizeOf<MiscUniformData>();
            public Vector3 Position { get; set; } // 12 bytes
            public int Unused1 { get; set; } // Need another 8 bytes to reach a multiple of 16.
            public Vector3 TileSize { get; set; } // +12 bytes = 24
            public int Unused2 { get; set; } // Need another 8 bytes to reach a multiple of 16.
        }

        public void CreateDeviceObjects(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
            ResourceFactory factory = gd.ResourceFactory;

            _vb = factory.CreateBuffer(new BufferDescription(Vertices.SizeInBytes(), BufferUsage.VertexBuffer));
            _ib = factory.CreateBuffer(new BufferDescription(Indices.SizeInBytes(), BufferUsage.IndexBuffer));
            _miscUniformBuffer = factory.CreateBuffer(new BufferDescription(MiscUniformData.SizeInBytes, BufferUsage.UniformBuffer));
            _vb.Name = "TileMapVertexBuffer";
            _ib.Name = "TileMapIndexBuffer";
            _miscUniformBuffer.Name = "TileMapMiscBuffer";
            cl.UpdateBuffer(_vb, 0, Vertices);
            cl.UpdateBuffer(_ib, 0, Indices);

            var shaderSet = new ShaderSetDescription(new[] { VertexLayout, InstanceLayout },
                factory.CreateFromSpirv(ShaderHelper.Vertex(VertexShader), ShaderHelper.Fragment(FragmentShader)));

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
                new[] { _layout },
                sc.MainSceneFramebuffer.OutputDescription);

            _pipeline = factory.CreateGraphicsPipeline(ref pd);
            _pipeline.Name = "P_TileMapRenderer";
            _disposeCollector.Add(_vb, _ib, _layout, _textureSampler, _pipeline);
        }

        public IEnumerable<IRenderable> UpdatePerFrameResources(GraphicsDevice gd, CommandList cl, SceneContext sc, IEnumerable<IRenderable> renderables)
        {
            ITextureManager textureManager = Exchange.Resolve<ITextureManager>();
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

                textureManager.PrepareTexture(tilemap.Floors, gd);
                textureManager.PrepareTexture(tilemap.Walls, gd);
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

        public void Render(GraphicsDevice gd, CommandList cl, SceneContext sc, RenderPasses renderPass, IRenderable renderable)
        {
            ITextureManager textureManager = Exchange.Resolve<ITextureManager>();
            var window = renderable as TileMapWindow;
            if (window == null)
                return;

            var tilemap = window.TileMap;
            cl.PushDebugGroup($"Tiles3D:{tilemap.Name}:{tilemap.RenderOrder}");
            TextureView floors = textureManager.GetTexture(tilemap.Floors);
            TextureView walls = textureManager.GetTexture(tilemap.Walls);

            var miscUniformData = new MiscUniformData { Position = tilemap.Position, TileSize = tilemap.TileSize, Unused1 = 0, Unused2 = 0 };
            cl.UpdateBuffer(_miscUniformBuffer, 0, miscUniformData);

            var resourceSet = gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(_layout,
                sc.ProjectionMatrixBuffer,
                sc.ModelViewMatrixBuffer,
                _miscUniformBuffer,
                gd.PointSampler,
                sc.PaletteView,
                _textureSampler,
                floors,
                walls));
            _resourceSets.Add(resourceSet);

            cl.SetPipeline(_pipeline);
            cl.SetGraphicsResourceSet(0, resourceSet);
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

            _disposeCollector.DisposeAll();
        }

        public void Dispose() { DestroyDeviceObjects(); }
    }
}
