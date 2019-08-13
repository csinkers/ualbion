using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.Parsers;
using UAlbion.Game.AssetIds;

namespace UAlbion.Game.Entities
{
    public interface IMap
    {
        Vector2 LogicalSize { get; }
    }

    public class Map2D : Component, IMap
    {
        readonly MapData2D _mapData;
        readonly TilesetData _tileData;
        readonly MapRenderable2D _renderable;
        readonly bool _useSmallSprites;

        public MapDataId MapId { get; }
        public Vector2 LogicalSize => new Vector2(_mapData.Width, _mapData.Height);
        public Vector2 PhysicalSize => _renderable.SizePixels;
        public Vector2 TileSize => _renderable.TileSize;

        static readonly IList<Handler> Handlers = new Handler[]
        {
            new Handler<Map2D, SubscribedEvent>((x, e) => x.Subscribed()),
            new Handler<Map2D, SelectEvent>((x, e) => x.Select(e)),
        };

        void Select(SelectEvent e)
        {
            if (e.X < 0 || e.X >= _mapData.Width ||
                e.Y < 0 || e.Y >= _mapData.Height)
                return;

            int index = e.Y * _mapData.Width + e.X;
            e.RegisterHit("Map", this);
            int UnderlayId = _mapData.Underlay[index];
            int OverlayId = _mapData.Overlay[index];
            e.RegisterHit("Underlay", _tileData.Tiles[UnderlayId]);
            e.RegisterHit("Overlay", _tileData.Tiles[OverlayId]);
            int zoneIndex = _mapData.ZoneLookup[index];
            if (zoneIndex != -1)
                e.RegisterHit("Zone", _mapData.Zones[zoneIndex]);
        }

        public Map2D(Assets assets, MapDataId mapId) : base(Handlers)
        {
            MapId = mapId;
            _mapData = assets.LoadMap2D(mapId);
            var tileset = assets.LoadTexture((IconGraphicsId)_mapData.TilesetId);
            _tileData = assets.LoadTileData((IconDataId) _mapData.TilesetId);
            _renderable = new MapRenderable2D(_mapData, tileset, _tileData);
            _useSmallSprites = _tileData.UseSmallGraphics;
        }

        void Subscribed()
        {
            _renderable.Attach(Exchange);
            foreach (var npc in _mapData.Npcs)
            {
                IComponent sprite = 
                    _useSmallSprites
                        ? (IComponent)new SmallNpcSprite((SmallNpcId)npc.ObjectNumber, npc.Waypoints)
                        : new LargeNpcSprite((LargeNpcId)npc.ObjectNumber, npc.Waypoints);

                sprite.Attach(Exchange);
            }
        }
    }
}