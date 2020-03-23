using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Entities;
using UAlbion.Game.Events;
using UAlbion.Game.Scenes;

namespace UAlbion.Game
{
    public class MapManager : Component, IMapManager
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<MapManager, LoadMapEvent>((x, e) =>
            {
                x._pendingMapChange = e.MapId;
                x.LoadMap();
            }),
            H<MapManager, BeginFrameEvent>((x, e) => x.LoadMap()),
            H<MapManager, RefreshMapSubscribersEvent>((x, e) =>
            {
                x._allMapsExchange.IsActive = false;
                x._allMapsExchange.IsActive = true;
            }),
            H<MapManager, TeleportEvent>((x,e) => x.Teleport(e))
        );

        EventExchange _allMapsExchange;
        MapDataId? _pendingMapChange;

        public IMap Current { get; private set; }
        public MapManager() : base(Handlers) { }

        public override void Subscribed()
        {
            _allMapsExchange ??= new EventExchange("Maps", Exchange);
            base.Subscribed();
        }

        void LoadMap()
        {
            if (_pendingMapChange == null) // TODO: Check for when new map == current map
                return;

            var pendingMapChange = _pendingMapChange.Value;
            _pendingMapChange = null;
            Raise(new UnloadMapEvent());
            if (pendingMapChange == 0) // 0 = Build a blank scene for testing / debugging
            {
                Raise(new SetSceneEvent(SceneId.World2D));
                return;
            }

            foreach (var exchange in _allMapsExchange.Children)
                exchange.IsActive = false;
            _allMapsExchange.PruneInactiveChildren();

            Raise(new MuteEvent());
            var map = BuildMap(pendingMapChange);
            if (map != null)
            {
                Current = map;
                var mapExchange = new EventExchange(pendingMapChange.ToString(), _allMapsExchange);
                mapExchange.Attach(map);

                // Set the scene first to ensure scene-local components from other scenes are disabled.
                Raise(new SetSceneEvent(map is Entities.Map3D.Map ? SceneId.World3D : SceneId.World2D));
                Raise(new LogEvent(LogEvent.Level.Info, $"Loaded map {(int) pendingMapChange}: {pendingMapChange}"));
                Enqueue(new MapInitEvent());
            }
        }

        IMap BuildMap(MapDataId mapId)
        {
            var assets = Resolve<IAssetManager>();
            return  assets.LoadMap(mapId) switch
            {
                MapData2D map2d => new Entities.Map2D.Map(mapId, map2d),
                MapData3D map3d => new Entities.Map3D.Map(mapId, map3d),
                _ => null
            };
        }

        void Teleport(TeleportEvent e)
        {
            if (e.MapId != Current.MapId)
            {
                _pendingMapChange = e.MapId;
                LoadMap();
            }

            Raise(new PartyJumpEvent(e.X, e.Y));
            if(e.Direction != TeleportDirection.Unchanged)
                Raise(new PartyTurnEvent(e.Direction));
            Raise(new CameraJumpEvent(e.X, e.Y));
        }
    }
}
