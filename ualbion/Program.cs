using System.IO;
using System.Numerics;
using System.Reflection;
using System.Text;
using UAlbion.Core;
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
            RunGame(assets, baseDir);
        }

        static void RunGame(AssetManager assets, string baseDir)
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

            var logger = new ConsoleLogger();
            using var engine = new Engine(logger, backend,
#if DEBUG
                true);
#else
                 false);
#endif

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
            engine.GlobalExchange
                .Register<ISettings>(new Settings { BasePath = baseDir }) // Need to register settings first, as the AssetConfigLocator relies on it.
                .Register<IAssetManager>(assets)
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

            // Dump.CoreSprites(assets, baseDir); return;
            // Dump.CharacterSheets(assets);
            // Dump.Chests(assets);
            // Dump.ItemData(assets, baseDir);

            engine.GlobalExchange.Raise(new NewGameEvent(), null);
            /*
            engine.GlobalExchange.Raise(new LoadMapEvent(MapDataId.AltesFormergebäude), null); /*
            engine.GlobalExchange.Raise(new LoadMapEvent(MapDataId.Jirinaar3D), null); /*
            engine.GlobalExchange.Raise(new LoadMapEvent(MapDataId.HausDesJägerclans), null); //*/
            /*
            engine.GlobalExchange.Raise(new SetSceneEvent(SceneId.Inventory), null);
            //*/

            //engine.GlobalExchange.Raise(new SetSceneEvent((int)SceneId.MainMenu), null);
            engine.Run();
        }
    }
}
