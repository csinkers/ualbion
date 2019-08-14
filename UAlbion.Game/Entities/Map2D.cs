using System;
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
        public Vector3 Position => Vector3.Zero;
        public Vector3 Normal => Vector3.UnitZ;

        static readonly IList<Handler> Handlers = new Handler[]
        {
            new Handler<Map2D, SubscribedEvent>((x, e) => x.Subscribed()),
            new Handler<Map2D, WorldCoordinateSelectEvent>((x, e) => x.Select(e)),
        };

        void Select(WorldCoordinateSelectEvent e)
        {
            float denominator = Vector3.Dot(Normal, e.Direction);
            if (Math.Abs(denominator) < 0.00001f)
                return;

            float t = Vector3.Dot(Position - e.Origin, Normal) / denominator;
            if (t < 0)
                return;

            var intersectionPoint = e.Origin + t * e.Direction;
            int x = (int)(intersectionPoint.X / TileSize.X);
            int y = (int)(intersectionPoint.Y / TileSize.Y);
            if (x < 0 || x >= _mapData.Width ||
                y < 0 || y >= _mapData.Height)
                return;

            int index = y * _mapData.Width + x;

            int UnderlayId = _mapData.Underlay[index];
            int OverlayId = _mapData.Overlay[index];
            int zoneIndex = _mapData.ZoneLookup[index];

            if (zoneIndex != -1)
                e.RegisterHit(t, "Zone", _mapData.Zones[zoneIndex]);

            e.RegisterHit(t, "Overlay", _tileData.Tiles[OverlayId]);
            e.RegisterHit(t, "Underlay", _tileData.Tiles[UnderlayId]);
            e.RegisterHit(t, "Map", this);
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