using System;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Events;
using UAlbion.Game.State;

namespace UAlbion.Game.Entities.Map2D
{
    public class Map : Component, IMap
    {
        LogicalMap _logicalMap;
        public MapDataId MapId { get; }
        public MapType MapType => _logicalMap.UseSmallSprites ? MapType.Small : MapType.Large;
        public Vector2 LogicalSize => new Vector2(_logicalMap.Width, _logicalMap.Height);
        public Vector3 TileSize
        {
            get; private set;
        }

        public float BaseCameraHeight => 1.0f;
        static readonly HandlerSet Handlers = new HandlerSet(
            H<Map, MapInitEvent>((x, e) => x.FireEventChains(TriggerType.MapInit, true)),
            H<Map, SlowClockEvent>((x, e) => x.FireEventChains(TriggerType.EveryStep, false)),
            H<Map, HourElapsedEvent>((x, e) => x.FireEventChains(TriggerType.EveryHour, true)),
            H<Map, DayElapsedEvent>((x, e) => x.FireEventChains(TriggerType.EveryDay, true)),
            H<Map, PlayerEnteredTileEvent>((x,e) => x.OnPlayerEnteredTile(e)),
            H<Map, NpcEnteredTileEvent>((x,e) => x.OnNpcEnteredTile(e)),
            H<Map, ChangeIconEvent>((x,e) => x.ChangeIcon(e))
            // H<Map, UnloadMapEvent>((x, e) => x.Unload()),
        );

        public override string ToString() { return $"Map2D: {MapId} ({(int)MapId})"; }

        public Map(MapDataId mapId) : base(Handlers) => MapId = mapId;

        public override void Subscribed()
        {
            Raise(new SetClearColourEvent(0,0,0));
            if (_logicalMap != null)
                return;

            var assetManager = Resolve<IAssetManager>();
            _logicalMap = new LogicalMap(assetManager, MapId);

            var tileset = assetManager.LoadTexture(_logicalMap.TilesetId);
            var renderable = AttachChild(new Renderable(_logicalMap, tileset));
            var selector = AttachChild(new SelectionHandler(_logicalMap, renderable.TileSize));
            selector.HighlightIndexChanged += (sender, x) => renderable.HighlightIndex = x;
            TileSize = new Vector3(renderable.TileSize, 1.0f);
            _logicalMap.TileSize = renderable.TileSize;

            AttachChild(new Collider(_logicalMap));
            IMovement partyMovement = AttachChild(_logicalMap.UseSmallSprites
                ?  (IMovement)new SmallPartyMovement(Vector2.Zero, MovementDirection.Right)
                :  new LargePartyMovement(Vector2.Zero, MovementDirection.Right)); // TODO: Initial position.

            foreach (var npc in _logicalMap.Npcs)
            {
                if (npc.Id == 0)
                    continue;

                AttachChild(_logicalMap.UseSmallSprites
                    ? new SmallNpc(npc) as IComponent
                    : new LargeNpc(npc));
            }

            var state = Resolve<IGameState>();
            foreach(var player in state.Party.StatusBarOrder)
            {
                player.GetPosition = () => partyMovement.GetPositionHistory(player.Id).Item1;
                AttachChild(_logicalMap.UseSmallSprites
                    ? (IComponent)new SmallPlayer(player.Id, (SmallPartyGraphicsId)player.Id, () => partyMovement.GetPositionHistory(player.Id)) // TODO: Use a function to translate logical to sprite id
                    : new LargePlayer(player.Id, (LargePartyGraphicsId)player.Id, () => partyMovement.GetPositionHistory(player.Id))); // TODO: Use a function to translate logical to sprite id
            }
        }

        void FireEventChains(TriggerType type, bool log)
        {
            var zones = _logicalMap.GetZonesOfType(type);
            if (!log)
                Raise(new SetLogLevelEvent(LogEvent.Level.Warning));

            foreach (var zone in zones)
                Raise(new TriggerChainEvent(zone.Chain, zone.Node, type, zone.X, zone.Y));

            if (!log)
                Raise(new SetLogLevelEvent(LogEvent.Level.Info));
        }

        void OnNpcEnteredTile(NpcEnteredTileEvent e)
        {
            MapEventZone zone;
            while (true)
            {
                zone = _logicalMap.GetZone(e.X, e.Y);
                if (zone == null || !zone.Trigger.HasFlag(TriggerType.Npc))
                    return;

                if (!(zone.Chain.FirstEvent is OffsetEvent offset))
                    break;
                e = new NpcEnteredTileEvent(e.Id, e.X + offset.X, e.Y + offset.Y);
            }

            Raise(new TriggerChainEvent(zone.Chain, zone.Node, TriggerType.Npc, e.X, e.Y));
        }

        void OnPlayerEnteredTile(PlayerEnteredTileEvent e)
        {
            MapEventZone zone;
            while (true)
            {
                zone = _logicalMap.GetZone(e.X, e.Y);
                if (zone == null || !zone.Trigger.HasFlag(TriggerType.Normal))
                    return;

                if (!(zone.Chain.FirstEvent is OffsetEvent offset))
                    break;
                e = new PlayerEnteredTileEvent(e.X + offset.X, e.Y + offset.Y);
            }

            Raise(new TriggerChainEvent(zone.Chain, zone.Node, TriggerType.Normal, e.X, e.Y));
        }

        void ChangeIcon(ChangeIconEvent e)
        {
            byte x = (byte)(e.X + e.Context.X);
            byte y = (byte)(e.Y + e.Context.Y);
            switch (e.ChangeType)
            {
                case ChangeIconEvent.IconChangeType.Underlay: _logicalMap.ChangeUnderlay(x, y, e.Value); break;
                case ChangeIconEvent.IconChangeType.Overlay: _logicalMap.ChangeOverlay(x, y, e.Value); break;
                case ChangeIconEvent.IconChangeType.Wall: break; // N/A for 2D map
                case ChangeIconEvent.IconChangeType.Floor: break; // N/A for 2D map
                case ChangeIconEvent.IconChangeType.Ceiling: break; // N/A for 2D map
                case ChangeIconEvent.IconChangeType.NpcMovement: break;
                case ChangeIconEvent.IconChangeType.NpcSprite: break;
                case ChangeIconEvent.IconChangeType.Chain: _logicalMap.ChangeTileEventChain(x, y, e.Value); break;
                case ChangeIconEvent.IconChangeType.BlockHard: _logicalMap.PlaceBlock(x, y, e.Value, true); break;
                case ChangeIconEvent.IconChangeType.BlockSoft: _logicalMap.PlaceBlock(x, y, e.Value, false); break;
                case ChangeIconEvent.IconChangeType.Trigger: _logicalMap.ChangeTileEventTrigger(x, y, e.Value); break;
                default: throw new ArgumentOutOfRangeException();
            }
        }
    }
}
