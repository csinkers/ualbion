using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Assets.Save;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Game.Entities.Map2D
{
    public class LogicalMap2D : LogicalMap
    {
        readonly MapData2D _mapData;
        readonly TilesetData _tileData;
        readonly IList<Block> _blockList;

        public LogicalMap2D(
            IAssetManager assetManager,
            MapData2D mapData,
            MapChangeCollection tempChanges,
            MapChangeCollection permChanges) : base(mapData, tempChanges, permChanges)
        {
            if (assetManager == null) throw new ArgumentNullException(nameof(assetManager));
            _mapData = mapData ?? throw new ArgumentNullException(nameof(mapData));
            _tileData = assetManager.LoadTileData(_mapData.TilesetId);
            _blockList = assetManager.LoadBlockList(_mapData.TilesetId.ToBlockList());
            UseSmallSprites = _tileData.UseSmallGraphics;
        }

        public bool UseSmallSprites { get; }
        public TilesetGraphicsId TilesetId => _mapData.TilesetId.ToTilesetGraphics();
        public Vector2 TileSize { get; set; } // TODO: Tidy up how this gets initialised

        public TileData GetUnderlay(int x, int y) => GetUnderlay(Index(x, y));
        public TileData GetUnderlay(int index)
        {
            if (index < 0 || index >= _mapData.Underlay.Length)
                return null;

            int tileIndex = _mapData.Underlay[index];
            return tileIndex > 1 ? _tileData.Tiles[tileIndex] : null;
        }

        public TileData GetOverlay(int x, int y) => GetOverlay(Index(x, y));
        public TileData GetOverlay(int index)
        {
            if (index < 0 || index >= _mapData.Overlay.Length)
                return null;

            int tileIndex = _mapData.Overlay[index];
            return tileIndex > 1 ? _tileData.Tiles[tileIndex] : null;
        }
        
        public Passability GetPassability(int index) => 
            GetUnderlay(index)?.Collision 
            ?? GetOverlay(index)?.Collision 
            ?? Passability.Passable;

        protected override void ChangeUnderlay(byte x, byte y, ushort value)
        {
            var index = Index(x, y);
            if (index < 0 || index >= _mapData.Underlay.Length)
            {
                Error($"Tried to update invalid underlay index {index} (max {_mapData.Underlay.Length}");
            }
            else
            {
                _mapData.Underlay[index] = value;
                OnDirty(x, y, IconChangeType.Underlay);
            }
        }

        protected override void ChangeOverlay(byte x, byte y, ushort value)
        {
            var index = Index(x, y);
            if (index < 0 || index >= _mapData.Overlay.Length)
            {
                Error($"Tried to update invalid overlay index {index} (max {_mapData.Overlay.Length}");
            }
            else
            {
                _mapData.Overlay[index] = value;
                OnDirty(x, y, IconChangeType.Overlay);
            }
        }

        protected override void PlaceBlock(byte x, byte y, ushort blockId, bool overwrite)
        {
            if (blockId >= _blockList.Count)
            {
                Error($"Tried to set out-of-range block {blockId} (max id {_blockList.Count-1})");
                return;
            }

            var block = _blockList[blockId];
            for (int j = 0; j < block.Height; j++)
            {
                for (int i = 0; i < block.Width; i++)
                {
                    var targetIndex = Index(x + i, y + j);
                    var targetBlockIndex = j * block.Width + i;

                    if(targetIndex < 0 || targetIndex > _mapData.Underlay.Length)
                    {
                        Error($"Tried to set out-of-range index {targetIndex}, @ ({x},{y}) + ({i},{j}) for block {blockId}");
                        return;
                    }

                    int underlay = _mapData.Underlay[targetIndex];
                    int newUnderlay = block.Underlay[targetBlockIndex];
                    if (newUnderlay > 1 && (overwrite || underlay <= 1))
                    {
                        _mapData.Underlay[targetIndex] = newUnderlay;
                        OnDirty(x + i, y + j, IconChangeType.Underlay);
                    }

                    int overlay = _mapData.Overlay[targetIndex];
                    int newOverlay = block.Overlay[targetBlockIndex];
                    if (overwrite || overlay > 1)
                    {
                        _mapData.Overlay[targetIndex] = newOverlay;
                        OnDirty(x + i, y + j, IconChangeType.Overlay);
                    }
                }
            }
        }
    }
}
