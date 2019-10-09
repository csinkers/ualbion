using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using UAlbion.Core.Textures;
using Veldrid.Utilities;

namespace UAlbion.Core.Visual
{
    public class TileMap : IRenderable
    {
        readonly IList<uint[]> _palette;

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
            public TileFlags Flags { get; set; }
            public Vector2 WallSize { get; set; }

            public override string ToString() => $"({TilePosition.X}, {TilePosition.Y}): {Floor}.{Ceiling}.{Wall} ({Flags})";
        }

        public TileMap(int renderOrder, Vector3 tileSize, uint width, uint height, IList<uint[]> palette)
        {
            _palette = palette;
            RenderOrder = renderOrder;
            TileSize = tileSize;
            Width = width;
            Height = height;
            Tiles = new Tile[width * height];
            Floors = new MultiTexture("FloorTiles", palette);
            Walls = new MultiTexture("WallTiles", palette);
        }

        public string Name { get; set; }
        public int RenderOrder { get; }
        public Vector3 Position { get; set; }
        public Vector3 TileSize { get; }
        public Type Renderer => typeof(ExtrudedTileMapRenderer);

        public BoundingBox? Extents => new BoundingBox(Position, Position + TileSize * new Vector3(Width, 1, Height));
        public Matrix4x4 Transform => Matrix4x4.Identity;
        public event EventHandler ExtentsChanged; // Never fires, tilemaps remain static.
        public Tile[] Tiles { get; }
        public uint Width { get; }
        public uint Height { get; }
        public MultiTexture Floors { get; }
        public MultiTexture Walls { get; }
        public int InstanceBufferId { get; set; }

        public void DefineFloor(int id, ITexture texture) { Floors.AddTexture(id, texture, 0, 0, null, false); }
        public void DefineWall(int id, ITexture texture, uint x, uint y, byte transparentColour, bool isAlphaTested) { Walls.AddTexture(id, texture, x, y, transparentColour, isAlphaTested); }

        public void Set(int index, int x, int y, byte floorSubImage, byte ceilingSubImage, byte wallSubImage, int tick)
        {
            int paletteFrame = tick % _palette.Count;
            Floors.PaletteFrame = paletteFrame;
            Walls.PaletteFrame = paletteFrame;
            unsafe
            {
                fixed (Tile* tile = &Tiles[index])
                {
                    tile->TilePosition = new Vector2(x, y);
                    tile->Floor = (byte)Floors.GetSubImageAtTime(floorSubImage, tick);
                    tile->Ceiling = (byte)Floors.GetSubImageAtTime(ceilingSubImage, tick);
                    tile->Wall = (byte) Walls.GetSubImageAtTime(wallSubImage, tick);
                    tile->Flags = 0; // TileFlags.UsePalette;
                    Walls.GetSubImageDetails(tile->Wall, out _, out _, out var wallSize, out _);
                    tile->WallSize = wallSize;
                }
            }
        }
    }
}
