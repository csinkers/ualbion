using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Assets.Save;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Game.Entities.Map2D;

public class LogicalMap2D : LogicalMap
{
    readonly MapData2D _mapData;
    readonly IList<Block> _blockList;

    public LogicalMap2D(
        IAssetManager assetManager,
        MapData2D mapData,
        MapChangeCollection tempChanges,
        MapChangeCollection permChanges) : base(mapData, tempChanges, permChanges)
    {
        if (assetManager == null) throw new ArgumentNullException(nameof(assetManager));
        _mapData = mapData ?? throw new ArgumentNullException(nameof(mapData));
        TileData = assetManager.LoadTileData(_mapData.TilesetId);
        _blockList = assetManager.LoadBlockList(_mapData.TilesetId.ToBlockList());
        UseSmallSprites = TileData.UseSmallGraphics;
    }

    public bool UseSmallSprites { get; }
    public TilesetData TileData { get; }
    public Vector2 TileSize { get; set; } // TODO: Tidy up how this gets initialised

    public MapTile[] RawTiles => _mapData.Tiles;
    public IEventSet EventSet => _mapData;

    public TileData GetUnderlay(int x, int y) => GetUnderlay(Index(x, y));
    public TileData GetUnderlay(int index)
    {
        if (index < 0 || index >= _mapData.Tiles.Length)
            return null;

        int tileIndex = _mapData.Tiles[index].Underlay;
        return tileIndex >= 1 ? TileData.Tiles[tileIndex] : null;
    }

    public TileData GetOverlay(int x, int y) => GetOverlay(Index(x, y));
    public TileData GetOverlay(int index)
    {
        if (index < 0 || index >= _mapData.Tiles.Length)
            return null;

        int tileIndex = _mapData.Tiles[index].Overlay;
        return tileIndex > 1 ? TileData.Tiles[tileIndex] : null;
    }

    public Passability GetPassability(int index)
    {
        if (index < 0 || index >= _mapData.Tiles.Length)
            return Passability.Solid;

        var underlay = GetUnderlay(index);
        var overlay = GetOverlay(index);

        return overlay == null || overlay.UseUnderlayFlags 
            ? underlay?.Collision ?? 0 
            : overlay.Collision;
    }

    protected override void ChangeUnderlay(byte x, byte y, ushort value)
    {
        var index = Index(x, y);
        if (index < 0 || index >= _mapData.Tiles.Length)
        {
            Error($"Tried to update invalid underlay index {index} (max {_mapData.Tiles.Length}");
        }
        else
        {
            _mapData.Tiles[index].Underlay = (ushort)(value - 1);
            OnDirty(x, y, IconChangeType.Underlay);
        }
    }

    protected override void ChangeOverlay(byte x, byte y, ushort value)
    {
        var index = Index(x, y);
        if (index < 0 || index >= _mapData.Tiles.Length)
        {
            Error($"Tried to update invalid overlay index {index} (max {_mapData.Tiles.Length}");
        }
        else
        {
            _mapData.Tiles[index].Overlay = (ushort)(value - 1);
            OnDirty(x, y, IconChangeType.Overlay);
        }
    }

    protected override void PlaceBlock(byte x, byte y, bool overwrite, ChangeIconLayers layers, ushort blockId)
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

                if(targetIndex < 0 || targetIndex > _mapData.Tiles.Length)
                {
                    Error($"Tried to set out-of-range index {targetIndex}, @ ({x},{y}) + ({i},{j}) for block {blockId}");
                    return;
                }

                ushort newUnderlay = block.Tiles[targetBlockIndex].Underlay;
                if ((layers & ChangeIconLayers.Underlay) != 0 && (overwrite || newUnderlay > 1))
                {
                    _mapData.Tiles[targetIndex].Underlay = newUnderlay;
                    OnDirty(x + i, y + j, IconChangeType.Underlay);
                }

                ushort newOverlay = block.Tiles[targetBlockIndex].Overlay;
                if ((layers & ChangeIconLayers.Overlay) != 0 && (overwrite || newOverlay > 1))
                {
                    _mapData.Tiles[targetIndex].Overlay = newOverlay;
                    OnDirty(x + i, y + j, IconChangeType.Overlay);
                }
            }
        }
    }
}