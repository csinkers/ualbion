using System.Collections.Generic;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;
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
        readonly Engine _engine;
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
                map.Attach(exchange);
                Raise(new CameraJumpEvent((int)map.LogicalSize.X / 2, (int)map.LogicalSize.Y / 2));
                Raise(new SetSceneEvent((int)SceneId.World2D));
                Raise(new LogEvent((int)LogEvent.Level.Info, $"Loaded map {(int)_pendingMapChange}: {_pendingMapChange}"));
            }

            var mapData3D = _assets.LoadMap3D(_pendingMapChange.Value);
            if (mapData3D != null)
            {
                var exchange = new EventExchange(_pendingMapChange.Value.ToString(), _mapExchange);
                var map = new Map3D(_assets, _pendingMapChange.Value);
                map.Attach(exchange);
                Raise(new CameraJumpEvent((int)map.LogicalSize.X / 2, (int)map.LogicalSize.Y / 2));
                Raise(new SetSceneEvent((int)SceneId.World3D));
                Raise(new LogEvent((int)LogEvent.Level.Info, $"Loaded map {(int)_pendingMapChange}: {_pendingMapChange}"));
            }

            _pendingMapChange = null;
        }

        public MapManager(Assets assets, Engine engine, EventExchange mapExchange) : base(Handlers)
        {
            _assets = assets;
            _engine = engine;
            _mapExchange = mapExchange;
        }
    }
}