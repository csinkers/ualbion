using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Objects;
using UAlbion.Game.AssetIds;
using UAlbion.Game.Entities;
using UAlbion.Game.Events;

namespace UAlbion.Game
{
    public class SceneLoader : Component
    {
        static readonly IList<Handler> Handlers = new Handler[]
        {
            new Handler<SceneLoader, LoadMapEvent>((x, e) => x._pendingMapChange = (MapDataId)e.MapId), 
            new Handler<SceneLoader, BeginFrameEvent>((x, e) =>  x.LoadMap()), 
        };

        readonly Assets _assets;
        readonly Engine _engine;
        readonly ISpriteResolver _spriteResolver;
        MapDataId? _pendingMapChange;

        void LoadMap()
        {
            if (_pendingMapChange == null)
                return;

            if (_pendingMapChange == 0) // 0 = Build a blank scene for testing / debugging0
            {
                var scene = _engine.Create2DScene();
                scene.AddComponent(new PaletteManager(scene, _assets));
                scene.AddRenderer(new SpriteRenderer(_engine.TextureManager, _spriteResolver));
                _engine.SetScene(scene);
                _pendingMapChange = null;
                return;
            }

            var mapData2D = _assets.LoadMap2D((MapDataId)_pendingMapChange);
            if (mapData2D != null)
            {
                var scene = _engine.Create2DScene();
                scene.AddComponent(new PaletteManager(scene, _assets));
                scene.AddRenderer(new SpriteRenderer(_engine.TextureManager, _spriteResolver));

                var map = new Map(_assets, scene, (MapDataId)_pendingMapChange);
                scene.AddComponent(map);
                scene.Camera.Position = new Vector3(map.Size.X / 2, map.Size.Y / 2, 0);
                scene.Camera.Magnification = 1.0f;
                _engine.SetScene(scene);
            }

            var mapData3D = _assets.LoadMap3D((MapDataId)_pendingMapChange);
            if (mapData3D != null)
            {
            }

            _pendingMapChange = null;
        }

        public SceneLoader(Assets assets, Engine engine, ISpriteResolver spriteResolver) : base(Handlers)
        {
            _assets = assets;
            _engine = engine;
            _spriteResolver = spriteResolver;
            // TODO: Get it to generate its own scenes.
        }
    }
}