using System;
using System.Linq;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats;
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

        public MapId MapId { get; }
        public MapType MapType => _logicalMap.UseSmallSprites ? MapType.TwoDOutdoors : MapType.TwoD;
        public Vector2 LogicalSize => new(_logicalMap.Width, _logicalMap.Height);
        public Vector3 TileSize { get; private set; }
        public IMapData MapData => _mapData;
        public float BaseCameraHeight => 0.0f;
        public override string ToString() { return $"FlatMap: {MapId} ({MapId.Id})"; }

        public FlatMap(MapId mapId, MapData2D mapData)
        {
            On<PlayerEnteredTileEvent>(OnPlayerEnteredTile);
            On<NpcEnteredTileEvent>(OnNpcEnteredTile);
            On<ChangeIconEvent>(ChangeIcon);
            On<MapInitEvent>(e => FireEventChains(TriggerTypes.MapInit, true));
            On<SlowClockEvent>(e => FireEventChains(TriggerTypes.EveryStep, false));
            On<HourElapsedEvent>(e => FireEventChains(TriggerTypes.EveryHour, true));
            On<DayElapsedEvent>(e => FireEventChains(TriggerTypes.EveryDay, true));
            On<PartyChangedEvent>(e => RebuildPartyMembers());
            // On<UnloadMapEvent>(e => Unload());

            MapId = mapId;
            _mapData = mapData ?? throw new ArgumentNullException(nameof(mapData));
        }

        protected override void Subscribed()
        {
            Raise(new SetClearColourEvent(0,0,0, 1.0f));
            if (_logicalMap != null)
                return;

            var assetManager = Resolve<IAssetManager>();
            var state = Resolve<IGameState>();
            var gameFactory = Resolve<IGameFactory>();
            _logicalMap = new LogicalMap2D(assetManager, _mapData, state.TemporaryMapChanges, state.PermanentMapChanges);
            var tileset = assetManager.LoadTexture(_logicalMap.TilesetId);
            AttachChild(new ScriptManager());
            AttachChild(new Collider2D(_logicalMap, !_logicalMap.UseSmallSprites));
            var renderable = AttachChild(new MapRenderable2D(_logicalMap, tileset, gameFactory));
            var selector = AttachChild(new SelectionHandler2D(_logicalMap, renderable));
            selector.HighlightIndexChanged += (sender, x) => renderable.SetHighlightIndex(x);
            TileSize = new Vector3(renderable.TileSize, 1.0f);
            _logicalMap.TileSize = renderable.TileSize;

            var movementSettings = _logicalMap.UseSmallSprites ? MovementSettings.Small() : MovementSettings.Large();
            _partyMovement = AttachChild(new PartyCaterpillar(Vector2.Zero, Direction.East, movementSettings));

            foreach (var npc in _logicalMap.Npcs)
            {
                if (npc.SpriteOrGroup.IsNone)
                    continue;

                AttachChild(_logicalMap.UseSmallSprites
                    ? new SmallNpc(npc) as IComponent
                    : new LargeNpc(npc));
            }

            RebuildPartyMembers();
        }

        void RebuildPartyMembers()
        {
            var existing = Children.Where(x => x is SmallPlayer or LargePlayer).ToList();
            foreach (var player in existing)
                player.Remove();

            var state = Resolve<IGameState>();
            if (state.Party != null)
            {
                int i = 0;
                foreach(var player in state.Party.StatusBarOrder)
                {
                    var iCopy = i; // Make a copy to ensure each closure captures its own number.
                    player.GetPosition = () => _partyMovement.GetPositionHistory(iCopy).Item1;
                    (Vector3, int) PositionFunc() => _partyMovement.GetPositionHistory(iCopy);

                    AttachChild(_logicalMap.UseSmallSprites
                        ? (IComponent)new SmallPlayer(player.Id, player.Id.ToSmallPartyGraphics(), PositionFunc)
                        : new LargePlayer(player.Id, player.Id.ToLargePartyGraphics(), PositionFunc));
                    i++;
                }
            }
        }

        void FireEventChains(TriggerTypes type, bool log)
        {
            var zones = _logicalMap.GetZonesOfType(type);
            if (!log)
                Raise(new SetLogLevelEvent(LogLevel.Warning));

            foreach (var zone in zones)
                Raise(new TriggerChainEvent(zone.ChainSource, zone.Chain, zone.Node, new EventSource(_mapData.Id, _mapData.Id.ToMapText(), type, zone.X, zone.Y)));

            if (!log)
                Raise(new SetLogLevelEvent(LogLevel.Info));
        }

        void OnNpcEnteredTile(NpcEnteredTileEvent e)
        {
            MapEventZone zone;
            while (true)
            {
                zone = _logicalMap.GetZone(e.X, e.Y);
                if (zone == null || !zone.Trigger.HasFlag(TriggerTypes.Npc))
                    return;

                if (!(zone.Node.Event is OffsetEvent offset))
                    break;
                e = new NpcEnteredTileEvent(e.Id, e.X + offset.X, e.Y + offset.Y);
            }

            Raise(new TriggerChainEvent(zone.ChainSource, zone.Chain, zone.Node, new EventSource(_mapData.Id, _mapData.Id.ToMapText(), TriggerTypes.Npc, e.X, e.Y)));
        }

        void OnPlayerEnteredTile(PlayerEnteredTileEvent e)
        {
            MapEventZone zone;
            while (true)
            {
                zone = _logicalMap.GetZone(e.X, e.Y);
                if (zone?.Node == null || !zone.Trigger.HasFlag(TriggerTypes.Normal))
                    return;

                if (!(zone.Node.Event is OffsetEvent offset))
                    break;
                e = new PlayerEnteredTileEvent(e.X + offset.X, e.Y + offset.Y);
            }

            Raise(new TriggerChainEvent(zone.ChainSource, zone.Chain, zone.Node, new EventSource(_mapData.Id, _mapData.Id.ToMapText(), TriggerTypes.Normal, e.X, e.Y)));
        }

        void ChangeIcon(ChangeIconEvent e)
        {
            var context = Resolve<IEventManager>().Context;
            if (context.Source.AssetId.Type != AssetType.Map)
            {
                ApiUtil.Assert($"Expected event {e} to be triggered from a map event");
                return;
            }

            bool relative = e.Scopes is EventScopes.RelPerm or EventScopes.RelTemp;
            bool temp = e.Scopes is EventScopes.AbsTemp or EventScopes.RelTemp;

            byte x = relative ? (byte)(e.X + context.Source.X) : (byte)e.X;
            byte y = relative ? (byte)(e.Y + context.Source.Y) : (byte)e.Y;

            _logicalMap.Modify(x, y, e.ChangeType, e.Value, temp);
        }
    }
}
