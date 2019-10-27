using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Config;
using UAlbion.Game.Entities;
using UAlbion.Game.Events;

namespace UAlbion.Game
{
    public class MapManager : Component
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<MapManager, LoadMapEvent>((x, e) => x._pendingMapChange = e.MapId), 
            H<MapManager, BeginFrameEvent>((x, e) => x.LoadMap())
        );

        EventExchange _mapExchange;
        MapDataId? _pendingMapChange;

        public MapManager() : base(Handlers) { }

        protected override void Subscribed()
        {
            _mapExchange = new EventExchange("Maps", Exchange);
            base.Subscribed();
        }

        void LoadMap()
        {
            if (_pendingMapChange == null) // TODO: Check for when new map == current map
                return;

            Raise(new UnloadMapEvent());
            if (_pendingMapChange == 0) // 0 = Build a blank scene for testing / debugging
            {
                Raise(new SetSceneEvent(SceneId.World2D));
                _pendingMapChange = null;
                return;
            }

            foreach (var exchange in _mapExchange.Children)
                exchange.IsActive = false;
            _mapExchange.PruneInactiveChildren();

            var assets = Resolve<IAssetManager>();
            var mapData2D = assets.LoadMap2D(_pendingMapChange.Value);
            if (mapData2D != null)
            {
                var exchange = new EventExchange(_pendingMapChange.Value.ToString(), _mapExchange);
                var map = new Map2D(_pendingMapChange.Value);
                Raise(new SetSceneEvent(SceneId.World2D)); // Set the scene first to ensure scene-local components from other scenes are disabled.
                exchange.Attach(map);
                Raise(new SetInputModeEvent(InputMode.World2D));
                Raise(new SetMouseModeEvent(MouseMode.Normal));
                Raise(new CameraJumpEvent((int)map.LogicalSize.X / 2, (int)map.LogicalSize.Y / 2));
                Raise(new LogEvent(LogEvent.Level.Info, $"Loaded map {(int)_pendingMapChange}: {_pendingMapChange}"));
            }

            var mapData3D = assets.LoadMap3D(_pendingMapChange.Value);
            if (mapData3D != null)
            {
                var exchange = new EventExchange(_pendingMapChange.Value.ToString(), _mapExchange);
                var map = new Map3D(_pendingMapChange.Value);
                Raise(new SetSceneEvent(SceneId.World3D)); // Set the scene first to ensure scene-local components from other scenes are disabled.
                exchange.Attach(map);
                Raise(new SetInputModeEvent(InputMode.World3D));
                Raise(new SetMouseModeEvent(MouseMode.MouseLook));
                Raise(new CameraJumpEvent((int)map.LogicalSize.X / 2, (int)map.LogicalSize.Y / 2));
                Raise(new LogEvent(LogEvent.Level.Info, $"Loaded map {(int)_pendingMapChange}: {_pendingMapChange}"));
            }

            _pendingMapChange = null;
        }
    }
}