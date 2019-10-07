using System;
using Veldrid.Utilities;

namespace UAlbion.Core.Visual
{
    public class TileMapWindow : IRenderable
    {
        public string Name => TileMap.Name;
        public int RenderOrder => TileMap.RenderOrder;
        public Type Renderer => TileMap.Renderer;
        public BoundingBox? Extents => TileMap.Extents;
        public event EventHandler ExtentsChanged;

        public int Offset { get; }
        public int Length { get; }
        public TileMap TileMap { get; }
        public int InstanceBufferId { get; set; }

        public TileMapWindow(TileMap tileMap, int offset, int length)
        {
            TileMap = tileMap;
            Offset = offset;
            Length = length;
            TileMap.ExtentsChanged += (e, sender) => ExtentsChanged?.Invoke(sender, EventArgs.Empty);
        }
    }
}