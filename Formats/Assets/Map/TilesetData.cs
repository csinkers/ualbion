using System.Collections.Generic;

namespace UAlbion.Formats.Assets.Map
{
    public class TilesetData
    {
        public bool UseSmallGraphics { get; set; }
        public IList<TileData> Tiles { get; } = new List<TileData>();
    }
}
