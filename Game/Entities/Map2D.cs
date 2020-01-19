using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.State;

namespace UAlbion.Game.Entities
{
    public class Map2D : Component, IMap, ICollider
    {
        MapData2D _mapData;
        TilesetData _tileData;
        MapRenderable2D _renderable;
        bool _useSmallSprites;

        public MapDataId MapId { get; }
        public MapType MapType => _useSmallSprites ? MapType.Small : MapType.Large;
        public Vector2 LogicalSize => new Vector2(_mapData.Width, _mapData.Height);
        public Vector2 PhysicalSize => _renderable.SizePixels;
        public Vector3 TileSize => new Vector3(_renderable.TileSize, 1.0f);
        public float BaseCameraHeight => 1.0f;
        public Vector3 Position => Vector3.Zero;
        public Vector3 Normal => Vector3.UnitZ;
        static readonly HandlerSet Handlers = new HandlerSet(
            H<Map2D, WorldCoordinateSelectEvent>((x, e) => x.Select(e))
            // H<Map2D, UnloadMapEvent>((x, e) => x.Unload()),
        );

        public override string ToString() { return $"Map2D: {MapId} ({(int)MapId})"; }

        public bool IsOccupied(Vector2 tilePosition)
        {
            int index = (int)tilePosition.Y * _mapData.Width + (int)tilePosition.X;
            if (index >= _mapData.Underlay.Length)
                return true;

            var underlayTileId = _mapData.Underlay[index];
            var overlayTileId = _mapData.Overlay[index];
            var underlayTile = underlayTileId == -1 ? null : _tileData.Tiles[underlayTileId];
            var overlayTile = overlayTileId == -1 ? null : _tileData.Tiles[overlayTileId];

            return 
                underlayTile != null && underlayTile.Collision != TilesetData.Passability.Passable 
                || 
                overlayTile != null && overlayTile.Collision != TilesetData.Passability.Passable;
        }

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
            int x = (int)((intersectionPoint.X + TileSize.X/2) / TileSize.X);
            int y = (int)(intersectionPoint.Y / TileSize.Y);
            if (x < 0 || x >= _mapData.Width ||
                y < 0 || y >= _mapData.Height)
                return;

            int index = y * _mapData.Width + x;
            _renderable.HighlightIndex = index;

            var underlayId = _mapData.Underlay[index];
            var overlayId = _mapData.Overlay[index];
            int zoneIndex = _mapData.ZoneLookup[index];

            e.RegisterHit(t, $"Hit Tile ({(intersectionPoint.X + TileSize.X/2) / TileSize.X}, {intersectionPoint.Y / TileSize.Y})");

            if (overlayId != -1)
                e.RegisterHit(t, _tileData.Tiles[overlayId]);
            if (underlayId != -1)
                e.RegisterHit(t, _tileData.Tiles[underlayId]);
            e.RegisterHit(t, this);

            if (zoneIndex != -1)
            {
                var zone = _mapData.Zones[zoneIndex];
                e.RegisterHit(t, zone);
                HashSet<IEventNode> printedEvents = new HashSet<IEventNode>();
                var zoneEvent = zone.Event;
                while (zoneEvent != null && !printedEvents.Contains(zoneEvent))
                {
                    e.RegisterHit(t, zoneEvent);
                    printedEvents.Add(zoneEvent);
                    zoneEvent = zoneEvent.NextEvent;
                } 
            }
        }

        public override void Subscribed()
        {
            if (_mapData == null)
            {
                var assets = Resolve<IAssetManager>();
                _mapData = assets.LoadMap2D(MapId);
                var tileset = assets.LoadTexture((IconGraphicsId) _mapData.TilesetId);
                _tileData = assets.LoadTileData((IconDataId) _mapData.TilesetId);
                _renderable = new MapRenderable2D(_mapData, tileset, _tileData);
                _useSmallSprites = _tileData.UseSmallGraphics;

                Exchange.Attach(_renderable);
                Children.Add(_renderable);
                foreach (var npc in _mapData.Npcs)
                {
                    IComponent sprite =
                        _useSmallSprites
                            ? new SmallNpcSprite((SmallNpcId)npc.ObjectNumber, npc.Waypoints) as IComponent
                            : new LargeNpcSprite((LargeNpcId)npc.ObjectNumber, npc.Waypoints);

                    Exchange.Attach(sprite);
                    Children.Add(sprite);
                }

                IMovement partyMovement = /*_useSmallSprites 
                    ? new SmallPartyMovement() 
                    :*/ new LargePartyMovement(Vector2.Zero, LargePartyMovement.Direction.Right); // TODO: Initial position.

                Exchange.Register(partyMovement);
                Children.Add(partyMovement);

                var state = Resolve<IGameState>();
                foreach(var player in state.Party.StatusBarOrder)
                {
                    player.GetPosition = () => partyMovement.GetPositionHistory(player.Id).Item1;
                    var playerSprite = /*_useSmallSprites 
                        ? new PlayerSprite(player.Id, (SmallPartyGraphicsId)player.Id, () => partyMovement.GetPositionHistory(player.Id)); // TODO: Use a function to translate logical to sprite id
                        :*/ new PlayerSprite(player.Id, (LargePartyGraphicsId)player.Id, () => partyMovement.GetPositionHistory(player.Id)); // TODO: Use a function to translate logical to sprite id
                    Exchange.Attach(playerSprite);
                    Children.Add(playerSprite);
                }
            }

            Raise(new SetClearColourEvent(0,0,0));
        }
    }
}
