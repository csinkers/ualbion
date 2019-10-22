using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Parsers;
using UAlbion.Game.Events;

namespace UAlbion.Game.Entities
{
    public class Map2D : Component, IMap
    {
        MapData2D _mapData;
        TilesetData _tileData;
        MapRenderable2D _renderable;
        bool _useSmallSprites;

        public MapDataId MapId { get; }
        public Vector2 LogicalSize => new Vector2(_mapData.Width, _mapData.Height);
        public Vector2 PhysicalSize => _renderable.SizePixels;
        public Vector2 TileSize => _renderable.TileSize;
        public Vector3 Position => Vector3.Zero;
        public Vector3 Normal => Vector3.UnitZ;
        static readonly HandlerSet Handlers = new HandlerSet(
            H<Map2D, WorldCoordinateSelectEvent>((x, e) => x.Select(e))
            // H<Map2D, UnloadMapEvent>((x, e) => x.Unload()),
        );

        public override string ToString() { return $"Map2D: {MapId} ({(int)MapId}"; }

        public Map2D(MapDataId mapId) : base(Handlers)
        {
            MapId = mapId;
        }

        public void Select(WorldCoordinateSelectEvent e)
        {
            _renderable.HighlightIndex = null;
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
            _renderable.HighlightIndex = index;

            int UnderlayId = _mapData.Underlay[index];
            int OverlayId = _mapData.Overlay[index];
            int zoneIndex = _mapData.ZoneLookup[index];

            e.RegisterHit(t, _tileData.Tiles[OverlayId]);
            e.RegisterHit(t, _tileData.Tiles[UnderlayId]);
            e.RegisterHit(t, this);

            if (zoneIndex != -1)
            {
                var zone = _mapData.Zones[zoneIndex];
                e.RegisterHit(t, zone);
                HashSet<int> printedEvents = new HashSet<int>();
                int eventNumber = zone.EventNumber;
                while (eventNumber != 0xffff && !printedEvents.Contains(eventNumber))
                {
                    var zoneEvent = _mapData.Events[eventNumber];
                    e.RegisterHit(t, zoneEvent);
                    printedEvents.Add(eventNumber);
                    eventNumber = zoneEvent.NextEventId ?? 0xffff;
                } 
            }
        }

        protected override void Subscribed()
        {
            var assets = Resolve<IAssetManager>();
            _mapData = assets.LoadMap2D(MapId);
            var tileset = assets.LoadTexture((IconGraphicsId)_mapData.TilesetId);
            _tileData = assets.LoadTileData((IconDataId)_mapData.TilesetId);
            _renderable = new MapRenderable2D(_mapData, tileset, _tileData);
            _useSmallSprites = _tileData.UseSmallGraphics;

            Exchange.Attach(_renderable);
            foreach (var npc in _mapData.Npcs)
            {
                IComponent sprite = 
                    _useSmallSprites
                        ? new SmallNpcSprite((SmallNpcId)npc.ObjectNumber, npc.Waypoints) as IComponent
                        : new LargeNpcSprite((LargeNpcId)npc.ObjectNumber, npc.Waypoints, assets);

                Exchange.Attach(sprite);
            }

            Raise(new SetClearColourEvent(0,0,0));
            Raise(new SetTileSizeEvent(new Vector3(TileSize, 0.0f), 1.0f));
        }
    }
}