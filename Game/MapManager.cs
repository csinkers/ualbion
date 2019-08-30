using System.Collections.Generic;
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
        static readonly IList<Handler> Handlers = new Handler[]
        {
            new Handler<MapManager, LoadMapEvent>((x, e) => x._pendingMapChange = (MapDataId)e.MapId), 
            new Handler<MapManager, BeginFrameEvent>((x, e) => x.LoadMap()), 
        };

        readonly Assets _assets;
        readonly EventExchange _mapExchange;
        MapDataId? _pendingMapChange;

        void LoadMap()
        {
            if (_pendingMapChange == null) // TODO: Check for when new map == current map
                return;

            Raise(new UnloadMapEvent());
            if (_pendingMapChange == 0) // 0 = Build a blank scene for testing / debugging
            {
                Raise(new SetSceneEvent((int)SceneId.World2D));
                _pendingMapChange = null;
                return;
            }

            foreach (var exchange in _mapExchange.Children)
                exchange.IsActive = false;
            _mapExchange.PruneInactiveChildren();

            var mapData2D = _assets.LoadMap2D(_pendingMapChange.Value);
            if (mapData2D != null)
            {
                var exchange = new EventExchange(_pendingMapChange.Value.ToString(), _mapExchange);
                var map = new Map2D(_assets, _pendingMapChange.Value);
                Raise(new SetSceneEvent((int)SceneId.World2D)); // Set the scene first to ensure scene-local components from other scenes are disabled.
                exchange.Attach(map);
                Raise(new SetInputModeEvent((int)InputMode.World2D));
                Raise(new CameraJumpEvent((int)map.LogicalSize.X / 2, (int)map.LogicalSize.Y / 2));
                Raise(new LogEvent((int)LogEvent.Level.Info, $"Loaded map {(int)_pendingMapChange}: {_pendingMapChange}"));
            }

            var mapData3D = _assets.LoadMap3D(_pendingMapChange.Value);
            if (mapData3D != null)
            {
                var exchange = new EventExchange(_pendingMapChange.Value.ToString(), _mapExchange);
                var map = new Map3D(_assets, _pendingMapChange.Value);
                Raise(new SetSceneEvent((int)SceneId.World3D)); // Set the scene first to ensure scene-local components from other scenes are disabled.
                exchange.Attach(map);
                Raise(new SetInputModeEvent((int)InputMode.MouseLook));
                Raise(new CameraJumpEvent((int)map.LogicalSize.X / 2, (int)map.LogicalSize.Y / 2));
                Raise(new LogEvent((int)LogEvent.Level.Info, $"Loaded map {(int)_pendingMapChange}: {_pendingMapChange}"));
            }

            _pendingMapChange = null;
        }

        public MapManager(Assets assets, EventExchange mapExchange) : base(Handlers)
        {
            _assets = assets;
            _mapExchange = mapExchange;
        }
    }
}