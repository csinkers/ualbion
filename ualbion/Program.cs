using System.IO;
using System.Numerics;
using System.Reflection;
using System.Text;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Textures;
using UAlbion.Core.Visual;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Config;
using UAlbion.Game;
using UAlbion.Game.Assets;
using UAlbion.Game.Entities;
using UAlbion.Game.Events;
using UAlbion.Game.Gui;
using UAlbion.Game.Gui.Inventory;
using UAlbion.Game.Input;
using UAlbion.Game.Scenes;
using UAlbion.Game.State;
using Veldrid;

namespace UAlbion
{
    static class Program
    {
        static unsafe void Main()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // Required for code page 850 support in .NET Core

            /*
            Console.WriteLine("Entry point reached. Press enter to continue");
            Console.ReadLine(); //*/

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

            using var assets = new AssetManager();
            var logger = new ConsoleLogger();
            var globalExchange = new EventExchange("Global", logger);
            globalExchange
                // Need to register settings first, as the AssetConfigLocator relies on it.
                .Register<ISettings>(new Settings { BasePath = baseDir }) 
                .Register<IAssetManager>(assets)
                ;

            // Dump.CoreSprites(assets, baseDir);
            // Dump.CharacterSheets(assets);
            // Dump.Chests(assets);
            // Dump.ItemData(assets, baseDir);
            Dump.MapEvents(assets, baseDir, MapDataId.Toronto2DGesamtkarteSpielbeginn);

            //return;

            RunGame(globalExchange, baseDir);
        }

        static void RunGame(EventExchange global, string baseDir)
        {
            var backend =
                //VeldridStartup.GetPlatformDefaultBackend()
                //GraphicsBackend.Metal /*
                //GraphicsBackend.Vulkan /*
                //GraphicsBackend.OpenGL /*
                //GraphicsBackend.OpenGLES /*
                GraphicsBackend.Direct3D11 /*
                //*/
                ;

            using var engine = new Engine(backend,
#if DEBUG
                true);
#else
                 false);
#endif

            global.Attach(engine);
            InputConfig inputConfig = InputConfig.Load(baseDir);

            engine
                .AddRenderer(new SpriteRenderer())
                .AddRenderer(new ExtrudedTileMapRenderer())
                .AddRenderer(new FullScreenQuad())
                .AddRenderer(new DebugGuiRenderer())
                .AddRenderer(new ScreenDuplicator())
                ;

            var sceneManager = new SceneManager()
                .AddScene(new AutomapScene())
                .AddScene(new FlatScene())
                .AddScene(new DungeonScene())
                .AddScene(new MenuScene())
                .AddScene(new InventoryScene())
                ;

            var inputManager = new InputManager();
            global
                .Register<IInputManager>(inputManager)
                .Register<ILayoutManager>(new LayoutManager())
                .Register<IPaletteManager>(new PaletteManager())
                .Register<ISceneManager>(sceneManager)
                .Register<ISpriteResolver>(new SpriteResolver())
                .Register<IStateManager>(new StateManager())
                .Register<ITextManager>(new TextManager())
                .Register<ITextureManager>(new TextureManager())
                .Attach(new CursorManager())
                .Attach(new DebugMapInspector())
                .Attach(new GameClock())
                .Attach(new InputBinder(inputConfig))
                .Attach(new InputModeStack())
                .Attach(new MapManager())
                .Attach(new MouseModeStack())
                .Attach(new SceneStack())
                .Attach(new StatusBar())
                ;

            inputManager
                .RegisterInputMode(InputMode.ContextMenu, new ContextMenuInputMode())
                .RegisterInputMode(InputMode.World2D, new World2DInputMode())
                .RegisterMouseMode(MouseMode.DebugPick, new DebugPickMouseMode())
                .RegisterMouseMode(MouseMode.Exclusive, new ExclusiveMouseMode())
                .RegisterMouseMode(MouseMode.MouseLook, new MouseLookMouseMode())
                .RegisterMouseMode(MouseMode.Normal, new NormalMouseMode())
                ;

            var inventoryConfig = InventoryConfig.Load(baseDir);
            sceneManager.GetExchange(SceneId.Inventory)
                .Attach(new InventoryScreen(inventoryConfig))
                ;

            var menuBackground = new ScreenSpaceSprite<PictureId>(PictureId.MenuBackground8, new Vector2(0.0f, 1.0f), new Vector2(2.0f, -2.0f));
            sceneManager.GetExchange(SceneId.MainMenu)
                .Attach(new MainMenu())
                .Attach(menuBackground)
                ;

            global.Raise(new NewGameEvent(), null);
            /*
            global.Raise(new LoadMapEvent(MapDataId.AltesFormergebäude), null); /*
            global.Raise(new LoadMapEvent(MapDataId.Jirinaar3D), null); /*
            global.Raise(new LoadMapEvent(MapDataId.HausDesJägerclans), null); //*/
            /*
            global.Raise(new SetSceneEvent(SceneId.Inventory), null);
            //*/

            //global.Raise(new SetSceneEvent((int)SceneId.MainMenu), null);
            engine.Run();
        }
    }
}
