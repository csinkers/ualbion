using System;
using System.Collections.Generic;
using UAlbion.Api.Visual;
using UAlbion.Core.Textures;

namespace UAlbion.Core.Visual
{
    public class DungeonTilemap : IRenderable
    {
        readonly DungeonTile[] _tiles;
        DungeonTileMapProperties _properties;

        public DungeonTilemap(IAssetId id, string name, int tileCount, DungeonTileMapProperties properties, ICoreFactory factory, IPaletteManager paletteManager)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            if (paletteManager == null) throw new ArgumentNullException(nameof(paletteManager));

            Properties = properties;
            _tiles = new DungeonTile[tileCount];

            Name = name;
            Floors = factory.CreateMultiTexture(id, "FloorTiles:" + name, paletteManager);
            Walls = factory.CreateMultiTexture(id, "WallTiles:" + name, paletteManager);
        }

        public string Name { get; }
        public DrawLayer RenderOrder => DrawLayer.Background;
        public int PipelineId { get; set; } = (int)DungeonTilemapPipeline.Normal;
        public ReadOnlySpan<DungeonTile> Tiles => _tiles;
        public DungeonTileMapProperties Properties
        {
            get => _properties;
            set { _properties = value; PropertiesDirty = true; }
        }

        public ISet<int> AnimatedTiles { get; } = new HashSet<int>();
        public MultiTexture Floors { get; }
        public MultiTexture Walls { get; }
        public bool TilesDirty { get; set; } = true;
        public bool PropertiesDirty { get; set; } = true;

        public void DefineFloor(int id, ITexture texture) => Floors.AddTexture(id, texture, 0, 0, null, false);
        public void DefineWall(int id, ITexture texture, int x, int y, byte transparentColour, bool isAlphaTested) 
            => Walls.AddTexture(id, texture, x, y, transparentColour, isAlphaTested);

        public void Set(int index, byte floorSubImage, byte ceilingSubImage, byte wallSubImage, int frame)
        {
            bool isAnimated = Floors.IsAnimated(floorSubImage) || Floors.IsAnimated(ceilingSubImage) || Walls.IsAnimated(wallSubImage);
            if (isAnimated) AnimatedTiles.Add(index);
            else AnimatedTiles.Remove(index);

            unsafe
            {
                fixed (DungeonTile* tile = &Tiles[index])
                {
                    tile->Floor = (byte)Floors.GetSubImageAtTime(floorSubImage, frame);
                    tile->Ceiling = (byte)Floors.GetSubImageAtTime(ceilingSubImage, frame);
                    tile->Wall = (byte)Walls.GetSubImageAtTime(wallSubImage, frame);
                    tile->Flags = 0; // DungeonTileFlags.UsePalette;
                    var subImage = (SubImage)Walls.GetSubImage(tile->Wall);
                    tile->WallSize = subImage.TexSize;
                }
            }

            TilesDirty = true;
        }
    }
}
