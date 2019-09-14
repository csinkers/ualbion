using System;

namespace UAlbion.Core.Visual
{
    public class TileMapWindow : IRenderable
    {
        public string Name => TileMap.Name;
        public int RenderOrder => TileMap.RenderOrder;
        public Type Renderer => TileMap.Renderer;

        public int Offset { get; }
        public int Length { get; }
        public TileMap TileMap { get; }
        public int InstanceBufferId { get; set; }

        public TileMapWindow(TileMap tileMap, int offset, int length)
        {
            TileMap = tileMap;
            Offset = offset;
            Length = length;
        }
    }
}