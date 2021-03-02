namespace UAlbion.Formats.Exporters.Tiled
{
    public class TilemapProperties
    {
        public string GraphicsTemplate { get; set; }
        public int TileWidth { get; set; } = 16;
        public int TileHeight { get; set; } = 16;
        public int FrameDurationMs { get; set; } = 180;
        public int TilesetId { get; set; }
        public string BlankTilePath { get; set; }
    }
}