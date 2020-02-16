using System.Collections.Generic;
using System.Numerics;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Entities.Map2D
{
    public class LogicalMap
    {
        readonly MapData2D _mapData;
        readonly TilesetData _tileData;

        public LogicalMap(IAssetManager assetManager, MapDataId mapId)
        {
            _mapData = assetManager.LoadMap2D(mapId);
            _tileData = assetManager.LoadTileData((IconDataId)_mapData.TilesetId);
            UseSmallSprites = _tileData.UseSmallGraphics;
        }

        public int Width => _mapData.Width;
        public int Height => _mapData.Height;
        public bool UseSmallSprites { get; }
        public PaletteId PaletteId => (PaletteId)_mapData.PaletteId;
        public IconGraphicsId TilesetId => (IconGraphicsId)_mapData.TilesetId;
        public IEnumerable<MapNpc> Npcs => _mapData.Npcs;
        public Vector2 TileSize { get; set; } // TODO: Tidy up

        public int Index(int x, int y) => y * _mapData.Width + x;

        public TilesetData.TileData GetUnderlay(int x, int y) => GetUnderlay(Index(x, y));
        public TilesetData.TileData GetUnderlay(int index)
        {
            if (index < 0 || index >= _mapData.Underlay.Length)
                return null;

            int tileIndex = _mapData.Underlay[index];
            return tileIndex != -1 ? _tileData.Tiles[tileIndex] : null;
        }

        public TilesetData.TileData GetOverlay(int x, int y) => GetOverlay(Index(x, y));
        public TilesetData.TileData GetOverlay(int index)
        {
            if (index < 0 || index >= _mapData.Overlay.Length)
                return null;

            int tileIndex = _mapData.Overlay[index];
            return tileIndex != -1 ? _tileData.Tiles[tileIndex] : null;
        }

        public MapEventZone GetZone(int x, int y) => GetZone(Index(x, y));
        public MapEventZone GetZone(int index)
        {
            if (index < 0 || index >= _mapData.ZoneLookup.Length)
                return null;

            int zoneIndex = _mapData.ZoneLookup[index];
            return zoneIndex != -1 ? _mapData.Zones[zoneIndex] : null;
        }
    }
}