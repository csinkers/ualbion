using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Api.Visual;
using UAlbion.Core.Textures;

namespace UAlbion.Core.Visual
{
    public class DungeonTileMap : IRenderable
    {
        public DungeonTileMap(IAssetId id, string name, DrawLayer renderOrder, Vector3 tileSize, uint width, uint height, ICoreFactory factory, IPaletteManager paletteManager)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            if (paletteManager == null) throw new ArgumentNullException(nameof(paletteManager));
            RenderOrder = renderOrder;
            TileSize = tileSize;
            Width = width;
            Height = height;
            _tiles = new DungeonTile[width * height];
            Floors = factory.CreateMultiTexture(id, "FloorTiles:" + name, paletteManager);
            Walls = factory.CreateMultiTexture(id, "WallTiles:" + name, paletteManager);
        }

        readonly DungeonTile[] _tiles;
        public string Name { get; set; }
        public DrawLayer RenderOrder { get; }
        public int PipelineId => 1;
        public Vector3 TileSize { get; }
        public ReadOnlySpan<DungeonTile> Tiles => _tiles;
        public ISet<int> AnimatedTiles { get; } = new HashSet<int>();
        public uint Width { get; }
        public uint Height { get; }
        public MultiTexture Floors { get; }
        public MultiTexture Walls { get; }

        public void DefineFloor(int id, ITexture texture) => Floors.AddTexture(id, texture, 0, 0, null, false);
        public void DefineWall(int id, ITexture texture, int x, int y, byte transparentColour, bool isAlphaTested) 
            => Walls.AddTexture(id, texture, x, y, transparentColour, isAlphaTested);

        public void Set(int index, int x, int y, byte floorSubImage, byte ceilingSubImage, byte wallSubImage, int frame)
        {
            bool isAnimated = Floors.IsAnimated(floorSubImage) || Floors.IsAnimated(ceilingSubImage) || Walls.IsAnimated(wallSubImage);
            if (isAnimated) AnimatedTiles.Add(index);
            else AnimatedTiles.Remove(index);

            unsafe
            {
                fixed (DungeonTile* tile = &Tiles[index])
                {
                    tile->TilePosition = new Vector2(x, y);
                    tile->Floor = (byte)Floors.GetSubImageAtTime(floorSubImage, frame);
                    tile->Ceiling = (byte)Floors.GetSubImageAtTime(ceilingSubImage, frame);
                    tile->Wall = (byte)Walls.GetSubImageAtTime(wallSubImage, frame);
                    tile->Flags = 0; // DungeonTileFlags.UsePalette;
                    var subImage = (SubImage)Walls.GetSubImage(tile->Wall);
                    tile->WallSize = subImage.TexSize;
                }
            }
        }
    }
}
