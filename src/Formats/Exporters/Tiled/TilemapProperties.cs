using UAlbion.Formats.Assets.Labyrinth;

namespace UAlbion.Formats.Exporters.Tiled
{
    public class TilemapProperties
    {
        public int TilesetId { get; set; }
        public int TileWidth { get; set; } = 16;
        public int TileHeight { get; set; } = 16;
        public int FrameDurationMs { get; set; } = 180;
        public string BlankTilePath { get; set; }
    }

    public class Tilemap2DProperties : TilemapProperties
    {
        public string GraphicsTemplate { get; set; }
    }

    public class Tilemap3DProperties : TilemapProperties
    {
        public string ImagePath { get; set; } // For tilesets
        public string TilesetPath { get; set; } // For tilesets
        public int ImageWidth { get; set; } // For tilesets
        public int ImageHeight { get; set; } // For tilesets
        public IsometricMode IsoMode { get; set; } = IsometricMode.All; // For tilesets
        public int OffsetX { get; set; }
        public int OffsetY { get; set; }

        public string FloorPath { get; set; } // For maps
        public string CeilingPath { get; set; } // For maps
        public string WallPath { get; set; } // For maps
        public string ContentsPath { get; set; } // For maps
    }
}