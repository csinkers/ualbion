using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using UAlbion.Core.Textures;

namespace UAlbion.Core.Visual
{
    public class TileMap : IRenderable
    {
        [Flags]
        public enum TileFlags : int
        {
            TextureType1 = 1 << 0,
            TextureType2 = 1 << 1,
            UsePalette   = 1 << 2,
            Highlight    = 1 << 3,
            RedTint      = 1 << 4,
            GreenTint    = 1 << 5,
            BlueTint     = 1 << 6,
            Transparent  = 1 << 7,
        }

        public struct Tile
        {
            public static readonly uint StructSize = (uint)Unsafe.SizeOf<Tile>();
            public Vector2 TilePosition { get; set; }
            public byte Floor { get; set; } // 0 = No floor
            public byte Ceiling { get; set; } // 0 = No Ceiling
            public byte Wall { get; set; } // 0 = No Wall
            public byte Overlay { get; set; } // 0 = No Overlay
            public TileFlags Flags { get; set; }
            public Vector2 WallSize { get; set; }

            public override string ToString() => $"({TilePosition.X}, {TilePosition.Y}): {Floor}.{Ceiling}.{Wall}.{Overlay} ({Flags})";
        }

        public TileMap(int renderOrder, Vector3 tileSize, uint width, uint height)
        {
            RenderOrder = renderOrder;
            TileSize = tileSize;
            Width = width;
            Height = height;
            Tiles = new Tile[width * height];
            Floors = new MultiTexture("FloorTiles", 64, 64);
            Walls = new MultiTexture("WallTiles", 256, 256);
            Overlays = new MultiTexture("OverlayTiles", 256, 256);
        }

        public string Name { get; set; }
        public int RenderOrder { get; }
        public Vector3 TileSize { get; }
        public Type Renderer => typeof(ExtrudedTileMapRenderer);
        public Tile[] Tiles { get; }
        public uint Width { get; }
        public uint Height { get; }
        public MultiTexture Floors { get; }
        public MultiTexture Walls { get; }
        public MultiTexture Overlays { get; }
        public int InstanceBufferId { get; set; }

        public void Set(int x, int y, 
            ITexture floor, ITexture ceiling, ITexture wall, ITexture overlay,
            int floorSubImage, 
            int ceilingSubImage, 
            int wallSubImage, 
            int overlaySubImage)
        {
            unsafe
            {
                fixed (Tile* tile = &Tiles[y * Width + x])
                {
                    tile->TilePosition = new Vector2(x, y);
                    tile->Floor = Floors.AddTexture(floor, floorSubImage);
                    tile->Ceiling = Floors.AddTexture(ceiling, ceilingSubImage);
                    tile->Wall = Walls.AddTexture(wall, wallSubImage);
                    tile->Overlay = Overlays.AddTexture(overlay, overlaySubImage);
                    tile->Flags = TileFlags.UsePalette;
                    Walls.GetSubImageDetails(tile->Wall, out _, out _, out var wallSize, out _);
                    tile->WallSize = wallSize;
                }
            }
        }
    }
}
