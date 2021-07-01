using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Api.Visual;
using UAlbion.Core.Textures;
using UAlbion.Core.Veldrid.Sprites;
using UAlbion.Core.Veldrid.Visual;
using UAlbion.Core.Visual;
using Veldrid;

namespace UAlbion.Core.Veldrid
{
    public sealed class DungeonTilemap : Component, IDungeonTilemap
    {
        readonly EtmManager _manager;
        readonly MultiBuffer<DungeonTile> _tiles;
        readonly SingleBuffer<DungeonTileMapProperties> _properties;
        readonly CompositedTexture _dayFloors;
        readonly CompositedTexture _dayWalls;
        readonly CompositedTexture _nightFloors;
        readonly CompositedTexture _nightWalls;

        public DungeonTilemap(EtmManager manager, IAssetId id, string name, int tileCount, DungeonTileMapProperties properties, IPalette dayPalette, IPalette nightPalette)
        {
            if (dayPalette == null) throw new ArgumentNullException(nameof(dayPalette));
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));

            _tiles = new MultiBuffer<DungeonTile>(tileCount, BufferUsage.VertexBuffer) { Name = $"B_Inst{name}"};
            _properties = new SingleBuffer<DungeonTileMapProperties>(properties, BufferUsage.UniformBuffer) { Name = $"B_TileProps:{name}" };
            AttachChild(_tiles);
            AttachChild(_properties);

            Name = name;
            _dayFloors = new CompositedTexture(id, "FloorTiles:" + name, dayPalette);
            _dayWalls = new CompositedTexture(id, "WallTiles:" + name, dayPalette);
            if (nightPalette != null)
            {
                _nightFloors = new CompositedTexture(id, "FloorTiles:" + name, nightPalette);
                _nightWalls = new CompositedTexture(id, "WallTiles:" + name, nightPalette);
            }
        }

        protected override void Subscribed()
        {
            base.Subscribed();
            var textureSource = Resolve<ITextureSource>();
            var samplerSource = Resolve<ISpriteSamplerSource>();
            ResourceSet = new EtmSet
            {
                Name = $"RS_TileMap:{Name}",
                Properties = _properties,
                DayFloors = textureSource.GetArrayTexture(_dayFloors),
                DayWalls = textureSource.GetArrayTexture(_dayWalls),
                NightFloors = textureSource.GetArrayTexture(_nightFloors ?? _dayFloors),
                NightWalls = textureSource.GetArrayTexture(_nightWalls ?? _dayWalls),
                TextureSampler = samplerSource.GetSampler(SpriteSampler.Point)
            };
            AttachChild(ResourceSet);
        }
        protected override void Unsubscribed() => CleanupSet();

        public string Name { get; }
        public ReadOnlySpan<DungeonTile> Tiles => _tiles.Data;
        public float ObjectYScaling { get; set; }
        public int TileCount { get => _tiles.Count; set => _tiles.Resize(value); }

        public CompositedTexture DayWalls => _dayWalls;
        public CompositedTexture DayFloors => _dayFloors;
        internal EtmSet ResourceSet { get; private set; }
        public DungeonTileMapProperties Properties
        {
            get => _properties.Data;
            set => _properties.Data = value;
        }

        public ISet<int> AnimatedTiles { get; } = new HashSet<int>();
        public DungeonTilemapPipeline RendererId { get; set; }
        public DeviceBuffer TileBuffer => _tiles.DeviceBuffer;

        public void DefineFloor(int id, ITexture texture)
        {
            _dayFloors.AddTexture(id, texture, 0, 0, null, false);
            _nightFloors?.AddTexture(id, texture, 0, 0, null, false);
        }

        public void DefineWall(int id, ITexture texture, int x, int y, byte transparentColour, bool isAlphaTested)
        {
            _dayWalls.AddTexture(id, texture, x, y, transparentColour, isAlphaTested);
            _nightWalls?.AddTexture(id, texture, x, y, transparentColour, isAlphaTested);
        }

        public void SetTile(int index, byte floorSubImage, byte ceilingSubImage, byte wallSubImage, int frame, Tile3DFlags flags)
        {
            int totalFrameCount = 
                _dayFloors.GetFrameCountForLogicalId(floorSubImage) 
                + _dayFloors.GetFrameCountForLogicalId(ceilingSubImage) 
                + _dayWalls.GetFrameCountForLogicalId(wallSubImage);

            if (totalFrameCount > 3) AnimatedTiles.Add(index);
            else AnimatedTiles.Remove(index);

            var tiles = _tiles.Borrow();
            var wall = (byte)_dayWalls.GetSubImageAtTime(wallSubImage, frame, (flags & Tile3DFlags.WallBackAndForth) != 0);
            var subImage = _dayWalls.Regions[wall];
            tiles[index] =
                new DungeonTile
                {
                    Floor = (byte)_dayFloors.GetSubImageAtTime(floorSubImage, frame, (flags & Tile3DFlags.FloorBackAndForth) != 0),
                    Ceiling = (byte)_dayFloors.GetSubImageAtTime(ceilingSubImage, frame, (flags & Tile3DFlags.CeilingBackAndForth) != 0),
                    Wall = wall,
                    Flags = 0, // DungeonTileFlags.UsePalette;
                    WallSize = subImage.TexSize
                };
        }

        void CleanupSet()
        {
            ResourceSet.Dispose();
            RemoveChild(ResourceSet);
            ResourceSet = null;
        }

        public void Dispose()
        {
            CleanupSet();
            _manager.DisposeTilemap(this);
            _tiles.Dispose();
            _properties.Dispose();
        }

        public Vector3 Scale 
        { 
            get => new(_properties.Data.Scale.X, _properties.Data.Scale.Y, _properties.Data.Scale.Z);
            set { _properties.Modify((ref DungeonTileMapProperties x) => x.Scale = new Vector4(value, 0)); }
        }

        public Vector3 Rotation 
        { 
            get => new(_properties.Data.Rotation.X, _properties.Data.Rotation.Y, _properties.Data.Rotation.Z);
            set { _properties.Modify((ref DungeonTileMapProperties x) => x.Rotation = new Vector4(value, 0)); }
        }
        public Vector3 Origin 
        { 
            get => new(_properties.Data.Origin.X, _properties.Data.Origin.Y, _properties.Data.Origin.Z);
            set { _properties.Modify((ref DungeonTileMapProperties x) => x.Origin = new Vector4(value, 1.0f)); }
        }
        public Vector3 HorizontalSpacing 
        { 
            get => new(_properties.Data.HorizontalSpacing.X, _properties.Data.HorizontalSpacing.Y, _properties.Data.HorizontalSpacing.Z);
            set { _properties.Modify((ref DungeonTileMapProperties x) => x.HorizontalSpacing = new Vector4(value, 0)); }
        }
        public Vector3 VerticalSpacing 
        { 
            get => new(_properties.Data.VerticalSpacing.X, _properties.Data.VerticalSpacing.Y, _properties.Data.VerticalSpacing.Z);
            set { _properties.Modify((ref DungeonTileMapProperties x) => x.VerticalSpacing = new Vector4(value, 0)); }
        }
        public uint Width 
        { 
            get => _properties.Data.Width;
            set { _properties.Modify((ref DungeonTileMapProperties x) => x.Width = value); }
        }
        public uint AmbientLightLevel 
        { 
            get => _properties.Data.AmbientLightLevel;
            set { _properties.Modify((ref DungeonTileMapProperties x) => x.AmbientLightLevel = value); }
        }
        public uint FogColor 
        { 
            get => _properties.Data.FogColor;
            set { _properties.Modify((ref DungeonTileMapProperties x) => x.FogColor = value); }
        }
    }
}
