using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;
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
            })
        );

        EventExchange _allMapsExchange;
        MapDataId? _pendingMapChange;

        public IMap Current { get; private set; }
        public MapManager() : base(Handlers) { }

        protected override void Subscribed()
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

            var map = BuildMap(pendingMapChange);
            if (map != null)
            {
                var mapExchange = new EventExchange(pendingMapChange.ToString(), _allMapsExchange);

                mapExchange.Attach(map);
                if (map is ICollider collider)
                    mapExchange.Register(collider);

                Current = map;

                // Set the scene first to ensure scene-local components from other scenes are disabled.
                Raise(new SetSceneEvent(map is Map3D ? SceneId.World3D : SceneId.World2D)); 
                // Raise(new CameraJumpEvent((int) map.LogicalSize.X / 2, (int) map.LogicalSize.Y / 2));
                Raise(new LogEvent(LogEvent.Level.Info, $"Loaded map {(int) pendingMapChange}: {pendingMapChange}"));
            }
        }

        IMap BuildMap(MapDataId mapId)
        {
            var assets = Resolve<IAssetManager>();
            if (assets.LoadMap2D(mapId) != null)
                return new Map2D(mapId);

            if (assets.LoadMap3D(mapId) != null)
                return new Map3D(mapId);

            return null;
        }
    }
}