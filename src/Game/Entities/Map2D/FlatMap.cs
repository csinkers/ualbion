using System;
using System.Linq;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Events;
using UAlbion.Game.State;

namespace UAlbion.Game.Entities.Map2D
{
    public class FlatMap : Component, IMap
    {
        readonly MapData2D _mapData;
        LogicalMap2D _logicalMap;
        IMovement _partyMovement;

        public MapDataId MapId { get; }
        public MapType MapType => _logicalMap.UseSmallSprites ? MapType.TwoDOutdoors : MapType.TwoD;
        public Vector2 LogicalSize => new Vector2(_logicalMap.Width, _logicalMap.Height);
        public Vector3 TileSize { get; private set; }
        public IMapData MapData => _mapData;

        public float BaseCameraHeight => 1.0f;

        public override string ToString() { return $"FlatMap: {MapId} ({(int)MapId})"; }

        public FlatMap(MapDataId mapId, MapData2D mapData)
        {
            On<PlayerEnteredTileEvent>(OnPlayerEnteredTile);
            On<NpcEnteredTileEvent>(OnNpcEnteredTile);
            On<ChangeIconEvent>(ChangeIcon);
            On<MapInitEvent>(e => FireEventChains(TriggerTypes.MapInit, true));
            On<SlowClockEvent>(e => FireEventChains(TriggerTypes.EveryStep, false));
            On<HourElapsedEvent>(e => FireEventChains(TriggerTypes.EveryHour, true));
            On<DayElapsedEvent>(e => FireEventChains(TriggerTypes.EveryDay, true));
            On<DisableEventChainEvent>(e => _logicalMap.DisableChain(e.ChainNumber));
            On<PartyChangedEvent>(e => RebuildPartyMembers());
            // On<UnloadMapEvent>(e => Unload());

            MapId = mapId;
            _mapData = mapData ?? throw new ArgumentNullException(nameof(mapData));
        }

        protected override void Subscribed()
        {
            Raise(new SetClearColourEvent(0,0,0));
            if (_logicalMap != null)
                return;

            var assetManager = Resolve<IAssetManager>();
            var state = Resolve<IGameState>();
            _logicalMap = new LogicalMap2D(assetManager, _mapData, state.TemporaryMapChanges, state.PermanentMapChanges);
            var tileset = assetManager.LoadTexture(_logicalMap.TilesetId);
            AttachChild(new ScriptManager());
            AttachChild(new Collider2D(_logicalMap, !_logicalMap.UseSmallSprites));
            var renderable = AttachChild(new MapRenderable2D(_logicalMap, tileset));
            var selector = AttachChild(new SelectionHandler2D(_logicalMap, renderable));
            selector.HighlightIndexChanged += (sender, x) => renderable.SetHighlightIndex(x);
            TileSize = new Vector3(renderable.TileSize, 1.0f);
            _logicalMap.TileSize = renderable.TileSize;

            var movementSettings = _logicalMap.UseSmallSprites ? MovementSettings.Small() : MovementSettings.Large();
            _partyMovement = AttachChild(new PartyCaterpillar(Vector2.Zero, MovementDirection.Right, movementSettings));

            foreach (var npc in _logicalMap.Npcs)
            {
                AttachChild(_logicalMap.UseSmallSprites
                    ? new SmallNpc(npc) as IComponent
                    : new LargeNpc(npc));
            }

            RebuildPartyMembers();
        }

        void RebuildPartyMembers()
        {
            var existing = Children.Where(x => x is SmallPlayer || x is LargePlayer).ToList();
            foreach (var player in existing)
                player.Remove();

            var state = Resolve<IGameState>();
            if (state.Party != null)
            {
                foreach(var player in state.Party.StatusBarOrder)
                {
                    player.GetPosition = () => _partyMovement.GetPositionHistory(player.Id).Item1;
                    AttachChild(_logicalMap.UseSmallSprites
                        ? (IComponent)new SmallPlayer(player.Id, (SmallPartyGraphicsId)player.Id, () => _partyMovement.GetPositionHistory(player.Id)) // TODO: Use a function to translate logical to sprite id
                        : new LargePlayer(player.Id, (LargePartyGraphicsId)player.Id, () => _partyMovement.GetPositionHistory(player.Id))); // TODO: Use a function to translate logical to sprite id
                }
            }
        }

        void FireEventChains(TriggerTypes type, bool log)
        {
            var zones = _logicalMap.GetZonesOfType(type);
            if (!log)
                Raise(new SetLogLevelEvent(LogEvent.Level.Warning));

            foreach (var zone in zones)
                if (zone.Chain.Enabled)
                    Raise(new TriggerChainEvent(zone.Chain, zone.Node, type, _mapData.Id, zone.X, zone.Y));

            if (!log)
                Raise(new SetLogLevelEvent(LogEvent.Level.Info));
        }

        void OnNpcEnteredTile(NpcEnteredTileEvent e)
        {
            MapEventZone zone;
            while (true)
            {
                zone = _logicalMap.GetZone(e.X, e.Y);
                if (zone == null || !zone.Trigger.HasFlag(TriggerTypes.Npc))
                    return;

                if (!(zone.Chain.FirstEvent.Event is OffsetEvent offset))
                    break;
                e = new NpcEnteredTileEvent(e.Id, e.X + offset.X, e.Y + offset.Y);
            }

            if (zone.Chain.Enabled)
                Raise(new TriggerChainEvent(zone.Chain, zone.Node, TriggerTypes.Npc, _mapData.Id, e.X, e.Y));
        }

        void OnPlayerEnteredTile(PlayerEnteredTileEvent e)
        {
            MapEventZone zone;
            while (true)
            {
                zone = _logicalMap.GetZone(e.X, e.Y);
                if (zone?.Chain?.FirstEvent == null || !zone.Trigger.HasFlag(TriggerTypes.Normal))
                    return;

                if (!(zone.Chain.FirstEvent.Event is OffsetEvent offset))
                    break;
                e = new PlayerEnteredTileEvent(e.X + offset.X, e.Y + offset.Y);
            }

            if (zone.Chain.Enabled)
                Raise(new TriggerChainEvent(zone.Chain, zone.Node, TriggerTypes.Normal, _mapData.Id, e.X, e.Y));
        }

        void ChangeIcon(ChangeIconEvent e)
        {
            var context = Resolve<IEventManager>().Context;
            if (!(context.Source is EventSource.Map mapSource))
            {
                ApiUtil.Assert($"Expected event {e} to be triggered from a map event");
                return;
            }

            byte x = (byte)(e.X + mapSource.X);
            byte y = (byte)(e.Y + mapSource.Y);
            _logicalMap.Modify(x, y, e.ChangeType, e.Value, (e.Scopes & EventScopes.Temp) != 0);
        }
    }
}
