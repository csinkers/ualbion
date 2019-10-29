using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Entities;
using UAlbion.Game.Events;

namespace UAlbion.Game
{
    public class MapManager : Component
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<MapManager, LoadMapEvent>((x, e) =>
            {
                x._pendingMapChange = e.MapId;
                x.LoadMap();
            }), 
            H<MapManager, BeginFrameEvent>((x, e) => x.LoadMap())
        );

        EventExchange _mapExchange;
        MapDataId? _pendingMapChange;

        public MapManager() : base(Handlers) { }

        protected override void Subscribed()
        {
            _mapExchange ??= new EventExchange("Maps", Exchange);
            base.Subscribed();
        }

        void LoadMap()
        {
            if (_pendingMapChange == null) // TODO: Check for when new map == current map
                return;

            var pendingMapChange = _pendingMapChange.Value;
            Raise(new UnloadMapEvent());
            if (pendingMapChange == 0) // 0 = Build a blank scene for testing / debugging
            {
                Raise(new SetSceneEvent(SceneId.World2D));
                _pendingMapChange = null;
                return;
            }

            foreach (var exchange in _mapExchange.Children)
                exchange.IsActive = false;
            _mapExchange.PruneInactiveChildren();

            var assets = Resolve<IAssetManager>();
            var mapData2D = assets.LoadMap2D(pendingMapChange);
            if (mapData2D != null)
            {
                var exchange = new EventExchange(pendingMapChange.ToString(), _mapExchange);
                var map = new Map2D(pendingMapChange);
                Raise(new SetSceneEvent(SceneId.World2D)); // Set the scene first to ensure scene-local components from other scenes are disabled.
                exchange.Attach(map);
                Raise(new CameraJumpEvent((int)map.LogicalSize.X / 2, (int)map.LogicalSize.Y / 2));
                Raise(new LogEvent(LogEvent.Level.Info, $"Loaded map {(int)pendingMapChange}: {pendingMapChange}"));
            }

            var mapData3D = assets.LoadMap3D(pendingMapChange);
            if (mapData3D != null)
            {
                var exchange = new EventExchange(pendingMapChange.ToString(), _mapExchange);
                var map = new Map3D(pendingMapChange);
                Raise(new SetSceneEvent(SceneId.World3D)); // Set the scene first to ensure scene-local components from other scenes are disabled.
                exchange.Attach(map);
                Raise(new CameraJumpEvent((int)map.LogicalSize.X / 2, (int)map.LogicalSize.Y / 2));
                Raise(new LogEvent(LogEvent.Level.Info, $"Loaded map {(int)pendingMapChange}: {pendingMapChange}"));
            }

            _pendingMapChange = null;
        }
    }
}