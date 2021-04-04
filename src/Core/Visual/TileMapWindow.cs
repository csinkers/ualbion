using UAlbion.Api.Visual;

namespace UAlbion.Core.Visual
{
    public class TileMapWindow : IRenderable
    {
        public string Name => Tilemap.Name;
        public DrawLayer RenderOrder => Tilemap.RenderOrder;
        public int PipelineId => 1;
        public int Offset { get; }
        public int Length { get; }
        public DungeonTilemap Tilemap { get; }
        public int InstanceBufferId { get; set; }

        public TileMapWindow(DungeonTilemap tilemap, int offset, int length)
        {
            Tilemap = tilemap;
            Offset = offset;
            Length = length;
        }
    }
}
