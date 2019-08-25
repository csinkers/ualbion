using System.IO;
using System.Reflection;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Config;
using UAlbion.Game;
using UAlbion.Game.Events;
using UAlbion.Game.Input;
using Veldrid;

namespace UAlbion
{
    static class Program
    {
        public static Scene Create2DScene(Assets assets, EventExchange allScenesExchange)
        {
            // TODO: Build scenes from config
            var id = SceneId.World2D;
            var sceneExchange = new EventExchange(id.ToString(), allScenesExchange);
            var camera = new OrthographicCamera();
            var renderers = new[]
            {
                typeof(DebugGuiRenderer),
                typeof(FullScreenQuad),
                typeof(ScreenDuplicator),
                typeof(SpriteRenderer),
            };

            var scene = new Scene((int)id, camera, renderers);
            scene.Attach(sceneExchange);
            camera.Attach(sceneExchange);
            return scene;
        }

        public static Scene Create3DScene(Assets assets, EventExchange allScenesExchange)
        {
            var id = SceneId.World3D;
            var sceneExchange = new EventExchange(id.ToString(), allScenesExchange);
            var renderers = new[]
            {
                typeof(DebugGuiRenderer),
                typeof(FullScreenQuad),
                typeof(ScreenDuplicator),
                typeof(ExtrudedTileMapRenderer)
            };

            var camera = new PerspectiveCamera();
            var scene = new Scene((int)SceneId.World3D, camera, renderers);
            scene.Attach(sceneExchange);
            camera.Attach(sceneExchange);
            return scene;
        }

        static unsafe void Main()
        {
            Veldrid.Sdl2.SDL_version version;
            Veldrid.Sdl2.Sdl2Native.SDL_GetVersion(&version);

            var baseDir = Directory.GetParent(
                Path.GetDirectoryName(
                Assembly.GetExecutingAssembly()
                .Location)) // ./ualbion/bin/Debug
                ?.Parent    // ./ualbion/bin
                ?.Parent    // ./ualbion
                ?.Parent    // .
                ?.FullName;

            if (string.IsNullOrEmpty(baseDir))
                return;

            AssetConfig assetConfig = AssetConfig.Load(baseDir);
            CoreSpriteConfig coreSpriteConfig = CoreSpriteConfig.Load(baseDir);
            InputConfig inputConfig = InputConfig.Load(baseDir);

            var backend =
                //VeldridStartup.GetPlatformDefaultBackend()
                //GraphicsBackend.Metal /*
                //GraphicsBackend.Vulkan /*
                GraphicsBackend.OpenGL /*
                //GraphicsBackend.OpenGLES /*
                GraphicsBackend.Direct3D11 /*
                //*/
                ;

            /*
            Scenes:
                Menu Screen
                Inventory Screen
                2D World
                3D World
                Automap for 3D world
                Combat
             */

            using (var assets = new Assets(assetConfig, coreSpriteConfig))
            using (var engine = new Engine(backend))
            {
                var sceneExchange = new EventExchange("Scenes", engine.GlobalExchange);
                var mapExchange = new EventExchange("Maps", engine.GlobalExchange);
                var spriteResolver = new SpriteResolver(assets);
                engine.AddRenderer(new SpriteRenderer(engine.TextureManager, spriteResolver));
                engine.AddRenderer(new ExtrudedTileMapRenderer(engine.TextureManager));
                engine.AddScene(Create2DScene(assets, sceneExchange));
                engine.AddScene(Create3DScene(assets, sceneExchange));

                assets.Attach(engine.GlobalExchange);
                new ConsoleLogger().Attach(engine.GlobalExchange);
                new GameClock().Attach(engine.GlobalExchange);
                new MapManager(assets, engine, mapExchange).Attach(engine.GlobalExchange);
                new DebugMapInspector().Attach(engine.GlobalExchange);
                new NormalMouseMode().Attach(engine.GlobalExchange);
                new DebugPickMouseMode().Attach(engine.GlobalExchange);
                new ContextMenuMouseMode().Attach(engine.GlobalExchange);
                new InventoryMoveMouseMode().Attach(engine.GlobalExchange);
                new InputBinder(inputConfig).Attach(engine.GlobalExchange);
                new CursorManager(assets).Attach(engine.GlobalExchange);
                new PaletteManager(assets).Attach(engine.GlobalExchange);
                engine.GlobalExchange.Raise(new SetMouseModeEvent((int)MouseModeId.Normal), null);
                engine.GlobalExchange.Raise(new LoadMapEvent((int)MapDataId.TestMapToronto1), null);

                /*
                var menu = new MainMenu();
                scene.AddComponent(menu);

                var background = new Billboard2D<PictureId>(PictureId.MenuBackground8, 0)
                {
                    Position = new Vector2(-1.0f, 1.0f),
                    Size = new Vector2(2.0f, -2.0f)
                };
                engine.AddComponent(background);

                var statusBackground = assets.LoadPicture(PictureId.StatusBar);
                var status = new SpriteRenderer(statusBackground, new Vector2(0.0f, 0.8f), new Vector2(1.0f, 0.2f));
                scene.AddRenderable(status);
                //*/
                //engine.GlobalExchange.Raise(new LoadRenderDocEvent(), null);
                engine.Run();
            }
        }
    }
}
