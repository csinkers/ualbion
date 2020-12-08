namespace UAlbion.Formats.Exporters.Tiled
{
    public class TilemapProperties
    {
        public int TileWidth { get; set; }
        public int TileHeight { get; set; }
        public int Margin { get; set; }
        public int Spacing { get; set; }
        public string SheetPath { get; set; }
        public int SheetWidth { get; set; }
        public int SheetHeight { get; set; }
        public int FrameDurationMs { get; set; }
    }
}