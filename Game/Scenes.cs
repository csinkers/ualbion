using System;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Game.Entities;

namespace UAlbion.Game
{
    public interface IFlatScene : IScene { }
    public interface IDungeonScene : IScene { }
    public interface IMapScene : IScene { }
    public interface IMenuScene : IScene { }
    public interface IInventoryScene : IScene { }

    public class FlatScene : Scene, IFlatScene
    {
        const SceneId Id = SceneId.World2D;
        static readonly Type[] Renderers = {
            typeof(DebugGuiRenderer),
            typeof(FullScreenQuad),
            typeof(ScreenDuplicator),
            typeof(SpriteRenderer),
        };

        public FlatScene(EventExchange allScenesExchange)
            : base((int)Id,
                Id.ToString(),
                new OrthographicCamera(),
                Renderers,
                new EventExchange(Id.ToString(), allScenesExchange))
        {
            var cameraMotion = new CameraMotion2D((OrthographicCamera)Camera);
            SceneExchange
                .Attach(cameraMotion);
        }
    }

    public class DungeonScene : Scene, IDungeonScene
    {
        const SceneId Id = SceneId.World3D;
        static readonly Type[] Renderers = {
            typeof(DebugGuiRenderer),
            typeof(FullScreenQuad),
            typeof(ScreenDuplicator),
            typeof(ExtrudedTileMapRenderer),
            typeof(SpriteRenderer),
        };

        public DungeonScene(EventExchange allScenesExchange) :
            base((int)Id,
                Id.ToString(),
                new PerspectiveCamera(),
                Renderers,
                new EventExchange(Id.ToString(), allScenesExchange))
        {
            var cameraMotion = new CameraMotion3D((PerspectiveCamera)Camera);
            SceneExchange
                .Attach(cameraMotion);
        }
    }

    public class MapScene : Scene, IMapScene
    {
        const SceneId Id = SceneId.Automap;
        static readonly Type[] Renderers = {
            typeof(DebugGuiRenderer),
            typeof(FullScreenQuad),
            typeof(ScreenDuplicator),
            typeof(SpriteRenderer),
        };


        public MapScene(EventExchange allScenesExchange)
            : base((int)Id,
                Id.ToString(),
                new OrthographicCamera(),
                Renderers,
                new EventExchange(Id.ToString(), allScenesExchange))
        {
            var cameraMotion = new CameraMotion2D((OrthographicCamera)Camera);
            SceneExchange
                .Attach(cameraMotion);
        }
    }

    public class MenuScene : Scene, IMenuScene
    {
        const SceneId Id = SceneId.MainMenu;
        static readonly Type[] Renderers = {
            typeof(DebugGuiRenderer),
            typeof(FullScreenQuad),
            typeof(ScreenDuplicator),
            typeof(SpriteRenderer),
        };
        public MenuScene(EventExchange allScenesExchange) :
            base((int)Id,
                Id.ToString(),
                new OrthographicCamera(),
                Renderers,
                new EventExchange(Id.ToString(), allScenesExchange))
        { }
    }

    public class InventoryScene : Scene, IInventoryScene
    {
        const SceneId Id = SceneId.Inventory;
        static readonly Type[] Renderers = {
            typeof(DebugGuiRenderer),
            typeof(FullScreenQuad),
            typeof(ScreenDuplicator),
            typeof(SpriteRenderer),
        };
        public InventoryScene(EventExchange allScenesExchange) :
            base((int)Id,
                Id.ToString(),
                new OrthographicCamera(),
                Renderers,
                new EventExchange(Id.ToString(), allScenesExchange))
        { }
    }
}
