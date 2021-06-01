using System;
using System.Collections.Generic;
using UAlbion.Api.Visual;
using UAlbion.Core.Textures;

namespace UAlbion.Core.Visual
{
    [Flags]
    public enum Tile3DFlags
    {
        FloorBackAndForth = 1,
        CeilingBackAndForth = 1 << 1,
        WallBackAndForth = 1 << 2,
    }

    public class DungeonTilemap : IRenderable
    {
        DungeonTile[] _tiles;
        DungeonTileMapProperties _properties;

        public DungeonTilemap(IAssetId id, string name, int tileCount, DungeonTileMapProperties properties, ICoreFactory factory, IPalette dayPalette, IPalette nightPalette)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            if (dayPalette == null) throw new ArgumentNullException(nameof(dayPalette));

            Properties = properties;
            _tiles = new DungeonTile[tileCount];

            Name = name;
            DayFloors = new MultiTexture(id, "FloorTiles:" + name, dayPalette);
            DayWalls = new MultiTexture(id, "WallTiles:" + name, dayPalette);
            if (nightPalette != null)
            {
                NightFloors = new MultiTexture(id, "FloorTiles:" + name, nightPalette);
                NightWalls = new MultiTexture(id, "WallTiles:" + name, nightPalette);
            }
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
        public MultiTexture DayFloors { get; }
        public MultiTexture DayWalls { get; }
        public MultiTexture NightFloors { get; }
        public MultiTexture NightWalls { get; }
        public bool TilesDirty { get; set; } = true;
        public bool PropertiesDirty { get; set; } = true;

        public void DefineFloor(int id, ITexture texture)
        {
            DayFloors.AddTexture(id, texture, 0, 0, null, false);
            NightFloors?.AddTexture(id, texture, 0, 0, null, false);
        }

        public void DefineWall(int id, ITexture texture, int x, int y, byte transparentColour, bool isAlphaTested)
        {
            DayWalls.AddTexture(id, texture, x, y, transparentColour, isAlphaTested);
            NightWalls?.AddTexture(id, texture, x, y, transparentColour, isAlphaTested);
        }

        public void Set(int index, byte floorSubImage, byte ceilingSubImage, byte wallSubImage, int frame, Tile3DFlags flags)
        {
            int totalFrameCount = 
                DayFloors.GetFrameCountForLogicalId(floorSubImage) 
                + DayFloors.GetFrameCountForLogicalId(ceilingSubImage) 
                + DayWalls.GetFrameCountForLogicalId(wallSubImage);

            if (totalFrameCount > 3) AnimatedTiles.Add(index);
            else AnimatedTiles.Remove(index);

            unsafe
            {
                fixed (DungeonTile* tile = &Tiles[index])
                {
                    tile->Floor = (byte)DayFloors.GetSubImageAtTime(floorSubImage, frame, (flags & Tile3DFlags.FloorBackAndForth) != 0);
                    tile->Ceiling = (byte)DayFloors.GetSubImageAtTime(ceilingSubImage, frame, (flags & Tile3DFlags.CeilingBackAndForth) != 0);
                    tile->Wall = (byte)DayWalls.GetSubImageAtTime(wallSubImage, frame, (flags & Tile3DFlags.WallBackAndForth) != 0);
                    tile->Flags = 0; // DungeonTileFlags.UsePalette;
                    var subImage = DayWalls.Regions[tile->Wall];
                    tile->WallSize = subImage.TexSize;
                }
            }

            TilesDirty = true;
        }

        public void Resize(int tileCount)
        {
            var old = _tiles;
            _tiles = new DungeonTile[tileCount];
            if (old != null)
                for (int i = 0; i < _tiles.Length && i < old.Length; i++)
                    _tiles[i] = old[i];
            TilesDirty = true;
        }
    }
}
