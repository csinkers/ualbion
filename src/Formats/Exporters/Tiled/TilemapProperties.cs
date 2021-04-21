using UAlbion.Formats.Assets.Labyrinth;

namespace UAlbion.Formats.Exporters.Tiled
{
    public class TilemapProperties
    {
        public string GraphicsTemplate { get; set; }
        public int TileWidth { get; set; } = 16;
        public int TileHeight { get; set; } = 16;
        public int FrameDurationMs { get; set; } = 180;
        public int TilesetId { get; set; }
        public IsometricMode IsoMode { get; set; } = IsometricMode.All;
        public string BlankTilePath { get; set; }
        public string TilesetPath { get; set; }
        public string ImagePath { get; set; }
        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }
    }
}