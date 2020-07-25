using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using UAlbion.Api;
using UAlbion.Core.Textures;

namespace UAlbion.Core.Visual
{
    public class TileMap : IRenderable
    {
        [Flags]
        public enum TileFlags
        {
            TextureType1 = 1,
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

        public TileMap(string name, DrawLayer renderOrder, Vector3 tileSize, uint width, uint height, ICoreFactory factory, IPaletteManager paletteManager)
        {
            RenderOrder = renderOrder;
            TileSize = tileSize;
            Width = width;
            Height = height;
            Tiles = new Tile[width * height];
            Floors = factory.CreateMultiTexture("FloorTiles:" + name, paletteManager);
            Walls = factory.CreateMultiTexture("WallTiles:" + name, paletteManager);
        }

        public string Name { get; set; }
        public DrawLayer RenderOrder { get; }
        public int PipelineId => 1;
        public Vector3 TileSize { get; }

        public Tile[] Tiles { get; }
        public ISet<int> AnimatedTiles { get; } = new HashSet<int>();
        public uint Width { get; }
        public uint Height { get; }
        public MultiTexture Floors { get; }
        public MultiTexture Walls { get; }

        public void DefineFloor(int id, ITexture texture) => Floors.AddTexture(id, texture, 0, 0, null, false);
        public void DefineWall(int id, ITexture texture, uint x, uint y, byte transparentColour, bool isAlphaTested) => Walls.AddTexture(id, texture, x, y, transparentColour, isAlphaTested);

        public void Set(int index, int x, int y, byte floorSubImage, byte ceilingSubImage, byte wallSubImage, int frame)
        {
            bool isAnimated = Floors.IsAnimated(floorSubImage) || Floors.IsAnimated(ceilingSubImage) || Walls.IsAnimated(wallSubImage);
            if (isAnimated) AnimatedTiles.Add(index);
            else AnimatedTiles.Remove(index);

            unsafe
            {
                fixed (Tile* tile = &Tiles[index])
                {
                    tile->TilePosition = new Vector2(x, y);
                    tile->Floor = (byte)Floors.GetSubImageAtTime(floorSubImage, frame);
                    tile->Ceiling = (byte)Floors.GetSubImageAtTime(ceilingSubImage, frame);
                    tile->Wall = (byte)Walls.GetSubImageAtTime(wallSubImage, frame);
                    tile->Flags = 0; // TileFlags.UsePalette;
                    var subImage = Walls.GetSubImageDetails(tile->Wall);
                    tile->WallSize = subImage.TexSize;
                }
            }
        }
    }
}
