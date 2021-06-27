using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Api.Visual;
using UAlbion.Core.Textures;

namespace UAlbion.Core.Visual
{
    public interface IDungeonTilemap : IDisposable
    {
        DungeonTilemapPipeline RendererId { get; set; }
        public Vector3 Scale { get; set; }
        public Vector3 Rotation { get; set; }
        public Vector3 Origin { get; set; }
        public Vector3 HorizontalSpacing { get; set; }
        public Vector3 VerticalSpacing { get; set; }
        public uint Width { get; set; }
        public uint AmbientLightLevel { get; set; }
        public uint FogColor { get; set; }
        public float ObjectYScaling { get; set; }
        int TileCount { get; set; }
        CompositedTexture DayWalls { get; }
        CompositedTexture DayFloors { get; }
        ISet<int> AnimatedTiles { get; }
        void DefineFloor(int id, ITexture texture);
        void DefineWall(int id, ITexture texture, int x, int y, byte transparentColour, bool isAlphaTested);
        void SetTile(int index, byte floorSubImage, byte ceilingSubImage, byte wallSubImage, int frame, Tile3DFlags flags);
    }
}