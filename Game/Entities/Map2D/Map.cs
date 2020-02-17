using System.Linq;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
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
            H<Map, SlowClockEvent>((x, e) => x.FireEventChains(TriggerType.EveryStep)),
            H<Map, HourElapsedEvent>((x, e) => x.FireEventChains(TriggerType.EveryHour)),
            H<Map, DayElapsedEvent>((x, e) => x.FireEventChains(TriggerType.EveryDay)),
            H<Map, PlayerEnteredTileEvent>((x,e) => x.OnPlayerEnteredTile(e)),
            H<Map, NpcEnteredTileEvent>((x,e) => x.OnNpcEnteredTile(e))
            // H<Map, UnloadMapEvent>((x, e) => x.Unload()),
        );

        public override string ToString() { return $"Map2D: {MapId} ({(int)MapId})"; }

        public Map(MapDataId mapId) : base(Handlers) => MapId = mapId;

        public void RunInitialEvents() => FireEventChains(TriggerType.MapInit);
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

        void FireEventChains(TriggerType type)
        {
            var chains = _logicalMap.GetGlobalZonesOfType(type);
            foreach (var chain in chains)
                Raise(new TriggerChainEvent(chain.Event, type));
        }

        void OnNpcEnteredTile(NpcEnteredTileEvent e)
        {
            var chains = _logicalMap.GetZones(e.X, e.Y).Where(x => x.Trigger.HasFlag(TriggerType.Npc));
            foreach (var chain in chains)
                Raise(new TriggerChainEvent(chain.Event, TriggerType.Npc));
        }

        void OnPlayerEnteredTile(PlayerEnteredTileEvent e)
        {
            var chains = _logicalMap.GetZones(e.X, e.Y).Where(x => x.Trigger.HasFlag(TriggerType.Normal));
            foreach (var chain in chains)
                Raise(new TriggerChainEvent(chain.Event, TriggerType.Normal));
        }

    }
}
