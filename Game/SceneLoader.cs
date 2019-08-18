using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Entities;
using UAlbion.Game.Events;

namespace UAlbion.Game
{
    public class SceneLoader : Component
    {
        static readonly IList<Handler> Handlers = new Handler[]
        {
            new Handler<SceneLoader, LoadMapEvent>((x, e) => x._pendingMapChange = (MapDataId)e.MapId), 
            new Handler<SceneLoader, BeginFrameEvent>((x, e) => x.LoadMap()), 
        };

        readonly Assets _assets;
        readonly Engine _engine;
        MapDataId? _pendingMapChange;

        void LoadMap()
        {
            if (_pendingMapChange == null)
                return;

            if (_pendingMapChange == 0) // 0 = Build a blank scene for testing / debugging
            {
                var (sceneExchange, scene) = _engine.Create2DScene("TestScene", Vector2.One);
                var paletteManager = new PaletteManager(_assets);
                paletteManager.Attach(sceneExchange);
                _engine.SetScene(scene);
                _pendingMapChange = null;
                return;
            }

            var mapData2D = _assets.LoadMap2D(_pendingMapChange.Value);
            if (mapData2D != null)
            {
                var map = new Map2D(_assets, _pendingMapChange.Value);
                var (sceneExchange, scene) = _engine.Create2DScene(_pendingMapChange.Value.ToString(), map.TileSize);
                var paletteManager = new PaletteManager(_assets);
                paletteManager.Attach(sceneExchange);

                map.Attach(sceneExchange);
                scene.Camera.Position = new Vector3(map.PhysicalSize.X / 2, map.PhysicalSize.Y / 2, 0);
                scene.Camera.Magnification = 1.0f;
                _engine.SetScene(scene);
                Raise(new LogEvent((int)LogEvent.Level.Info, $"Loaded map {(int)_pendingMapChange}: {_pendingMapChange}"));
            }

            var mapData3D = _assets.LoadMap3D(_pendingMapChange.Value);
            if (mapData3D != null)
            {
                var map = new Map3D(_assets, _pendingMapChange.Value);
                var (sceneExchange, scene) = _engine.Create3DScene(_pendingMapChange.Value.ToString());
                var paletteManager = new PaletteManager(_assets);
                paletteManager.Attach(sceneExchange);

                map.Attach(sceneExchange);
                scene.Camera.Position = Vector3.Zero;
                _engine.SetScene(scene);
                Raise(new LogEvent((int)LogEvent.Level.Info, $"Loaded map {(int)_pendingMapChange}: {_pendingMapChange}"));
            }

            _pendingMapChange = null;
        }

        public SceneLoader(Assets assets, Engine engine) : base(Handlers)
        {
            _assets = assets;
            _engine = engine;
            // TODO: Get it to generate its own scenes.
        }
    }

}