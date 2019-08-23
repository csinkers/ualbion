using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Core.Textures;
using Veldrid;

namespace UAlbion.Core.Visual
{
    public class MultiTexture : ITexture
    {
        IList<ITexture> _subTextures;
        IDictionary<ITexture, byte> _subTextureIndices;
        Texture _cachedTexture;

        public string Name { get; }
        public PixelFormat Format { get; }
        public TextureType Type { get; }
        public uint Width { get; }
        public uint Height { get; }
        public uint Depth { get; }
        public uint MipLevels { get; }
        public uint ArrayLayers { get; }
        public bool IsDirty { get; private set; }
        public void GetSubImageDetails(int subImage, out Vector2 size, out Vector2 texOffset, out Vector2 texSize, out int layer)
        {
            throw new NotImplementedException();
        }

        public Texture CreateDeviceTexture(GraphicsDevice gd, ResourceFactory rf, TextureUsage usage)
        {
            throw new NotImplementedException();
        }

        public byte AddTexture(ITexture texture, Func<int> getCurrentSubImage)
        {
            if (texture == null)
                return 0;

            // Max textures = 255. 0 = no texture.
            return 1; // TODO
        }
    }

    public class TileMap : IRenderable
    {
        const int MaxChildren = 16;
        public enum TextureSetId
        {
            Floor   = 0,
            Ceiling = 1,
            Wall    = 2,
            Overlay = 3,
        }

        static readonly Vertex3DTextured[] Vertices =
        {
            // Floor
            new Vertex3DTextured(-0.5f, 0.0f,  0.5f, 0.0f, 0.0f), //  0 Bottom Front Left
            new Vertex3DTextured( 0.5f, 0.0f,  0.5f, 1.0f, 0.0f), //  1 Bottom Front Right
            new Vertex3DTextured(-0.5f, 0.0f, -0.5f, 0.0f, 1.0f), //  2 Bottom Back Left
            new Vertex3DTextured(-0.5f, 0.0f, -0.5f, 1.0f, 1.0f), //  3 Bottom Back Right

            // Ceiling
            new Vertex3DTextured(-0.5f, 1.0f,  0.5f, 0.0f, 0.0f), //  4 Top Front Left
            new Vertex3DTextured( 0.5f, 1.0f,  0.5f, 1.0f, 0.0f), //  5 Top Front Right
            new Vertex3DTextured(-0.5f, 1.0f, -0.5f, 0.0f, 1.0f), //  6 Top Back Left
            new Vertex3DTextured(-0.5f, 1.0f, -0.5f, 1.0f, 1.0f), //  7 Top Back Right

            // Back
            new Vertex3DTextured( 0.5f, 1.0f, -0.5f, 0.0f, 0.0f), //  8 Back Top Right
            new Vertex3DTextured(-0.5f, 1.0f, -0.5f, 1.0f, 0.0f), //  9 Back Top Left
            new Vertex3DTextured( 0.5f, 0.0f, -0.5f, 0.0f, 1.0f), // 10 Back Bottom Right
            new Vertex3DTextured(-0.5f, 0.0f, -0.5f, 1.0f, 1.0f), // 11 Back Bottom Left

            // Front
            new Vertex3DTextured(-0.5f, 1.0f,  0.5f, 0.0f, 0.0f), // 12 Front Top Left
            new Vertex3DTextured( 0.5f, 1.0f,  0.5f, 1.0f, 0.0f), // 13 Front Top Right
            new Vertex3DTextured(-0.5f, 0.0f,  0.5f, 0.0f, 1.0f), // 14 Front Bottom Left
            new Vertex3DTextured( 0.5f, 0.0f,  0.5f, 1.0f, 1.0f), // 15 Front Bottom Right

            // Left
            new Vertex3DTextured(-0.5f, 1.0f, -0.5f, 0.0f, 0.0f), // 16 Back Top Left
            new Vertex3DTextured(-0.5f, 1.0f,  0.5f, 0.0f, 0.0f), // 17 Front Top Left
            new Vertex3DTextured(-0.5f, 0.0f, -0.5f, 0.0f, 0.0f), // 18 Back Bottom Left
            new Vertex3DTextured(-0.5f, 0.0f,  0.5f, 0.0f, 0.0f), // 19 Back Front Left

            // Right
            new Vertex3DTextured( 0.5f, 1.0f,  0.5f, 0.0f, 0.0f), // 20 Front Top Right
            new Vertex3DTextured( 0.5f, 1.0f, -0.5f, 0.0f, 0.0f), // 21 Back Top Right
            new Vertex3DTextured( 0.5f, 0.0f,  0.5f, 0.0f, 0.0f), // 22 Front Bottom Right
            new Vertex3DTextured( 0.5f, 0.0f, -0.5f, 0.0f, 0.0f), // 23 Back Bottom Right
        };

        static readonly ushort[] Indices =
        {
             0,  1,  2,  2,  0,  3, // Floor
             4,  5,  6,  6,  5,  7, // Ceiling
             8,  9, 10, 10,  9, 11, // Back
            12, 13, 14, 14, 13, 15, // Front
            16, 17, 18, 18, 17, 19, // Left
            20, 21, 22, 22, 21, 23, // Right
        };

        public class Tile
        {
            public byte Floor { get; set; } // 0 = No floor
            public byte Ceiling { get; set; } // 0 = No Ceiling
            public byte Wall { get; set; } // 0 = No Wall
            public byte Overlay { get; set; } // 0 = No Overlay
        }

        public TileMap(int renderOrder, Vector3 tileSize, int width, int height)
        {
            RenderOrder = renderOrder;
            TileSize = tileSize;
            Width = width;
            Height = height;
            Tiles = new Tile[width * height];
            Floors = new MultiTexture();
            Ceilings = new MultiTexture();
            Walls = new MultiTexture();
            Overlays = new MultiTexture();
        }

        public int RenderOrder { get; }
        public Vector3 TileSize { get; }
        public Type Renderer => typeof(OctreeTileRenderer);
        public Tile[] Tiles { get; }
        public int Width { get; }
        public int Height { get; }
        public MultiTexture Floors { get; }
        public MultiTexture Ceilings { get; }
        public MultiTexture Walls { get; }
        public MultiTexture Overlays { get; }

        public void Set(int x, int y, 
            ITexture floor, ITexture ceiling, ITexture wall, ITexture overlay,
            Func<int> getFloorSubImage, 
            Func<int> getCeilingSubImage, 
            Func<int> getWallSubImage, 
            Func<int> getOverlaySubImage)
        {
            var tile = Tiles[y * Width + x];
            tile.Floor = Floors.AddTexture(floor, getFloorSubImage);
            tile.Ceiling = Ceilings.AddTexture(ceiling, getCeilingSubImage);
            tile.Wall = Walls.AddTexture(wall, getWallSubImage);
            tile.Overlay = Overlays.AddTexture(overlay, getOverlaySubImage);
        }
    }

    public class OctreeTileRenderer : IRenderer
    {
        public RenderPasses RenderPasses => RenderPasses.Standard;
        public void CreateDeviceObjects(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
        }

        public IEnumerable<IRenderable> UpdatePerFrameResources(GraphicsDevice gd, CommandList cl, SceneContext sc, IEnumerable<IRenderable> renderables)
        {
            return Enumerable.Empty<IRenderable>();
        }

        public void Render(GraphicsDevice gd, CommandList cl, SceneContext sc, RenderPasses renderPass, IRenderable renderable)
        {
        }

        public void DestroyDeviceObjects()
        {
        }

        public void Dispose()
        {
        }
    }
}
