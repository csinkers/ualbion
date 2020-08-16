using UAlbion.Api;

namespace UAlbion.Core.Visual
{
    public class TileMapWindow : IRenderable
    {
        public string Name => TileMap.Name;
        public DrawLayer RenderOrder => TileMap.RenderOrder;
        public int PipelineId => 1;
        public int Offset { get; }
        public int Length { get; }
        public DungeonTileMap TileMap { get; }
        public int InstanceBufferId { get; set; }

        public TileMapWindow(DungeonTileMap tileMap, int offset, int length)
        {
            TileMap = tileMap;
            Offset = offset;
            Length = length;
        }
    }
}
