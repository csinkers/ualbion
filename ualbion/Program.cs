using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UAlbion.Api;
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
using UAlbion.Game.Settings;
using UAlbion.Game.State;
using Veldrid;

namespace UAlbion
{
    static class Program
    {
        static GraphicsBackend _backend = GraphicsBackend.Direct3D11;
        static bool _startupOnly;
        static bool _useRenderDoc;

        public static EventExchange Global { get; private set; }
        static void Main(string[] args)
        {
            _startupOnly = args.Contains("--startuponly");
            _useRenderDoc = args.Contains("--renderdoc") || args.Contains("-rd");
            if (args.Contains("-gl") || args.Contains("--opengl")) _backend = GraphicsBackend.OpenGL;
            if (args.Contains("-gles") || args.Contains("--opengles")) _backend = GraphicsBackend.OpenGLES;
            if (args.Contains("-vk") || args.Contains("--vulkan")) _backend = GraphicsBackend.Vulkan;
            if (args.Contains("-metal") || args.Contains("--metal")) _backend = GraphicsBackend.Metal;
            if (args.Contains("-d3d") || args.Contains("--direct3d")) _backend = GraphicsBackend.Direct3D11;

            PerfTracker.StartupEvent("Entered main");
            //GraphTests();
            //return;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // Required for code page 850 support in .NET Core
            PerfTracker.StartupEvent("Registered encodings");

            /*
            Console.WriteLine("Entry point reached. Press enter to continue");
            Console.ReadLine(); //*/

            var curDir = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            while (curDir != null && !File.Exists(Path.Combine(curDir.FullName, "data", "assets.json")))
                curDir = curDir.Parent;

            var baseDir = curDir?.FullName; 
            if (string.IsNullOrEmpty(baseDir))
                return;

            PerfTracker.StartupEvent($"Found base directory {baseDir}");

            var logger = new ConsoleLogger();
            var settings = new Settings { BasePath = baseDir };
            PerfTracker.StartupEvent("Registering asset manager");
            using var assets = new AssetManager();
            using var global = new EventExchange("Global", logger);
            Global = global;

            Global
                // Need to register settings first, as the AssetConfigLocator relies on it.
                .Register<ISettings>(settings) 
                .Register<IEngineSettings>(settings)
                .Register<IAssetManager>(assets)
                ;
            PerfTracker.StartupEvent("Registered asset manager");

            // Dump.CoreSprites(assets, baseDir);
            // Dump.CharacterSheets(assets);
            // Dump.Chests(assets);
            // Dump.ItemData(assets, baseDir);
            // Dump.MapEvents(assets, baseDir, MapDataId.Toronto2DGesamtkarteSpielbeginn);

            //return;

            RunGame(Global, baseDir);
        }

        static void RunGame(EventExchange global, string baseDir)
        {
            PerfTracker.StartupEvent("Creating engine");
            using var engine = new Engine(_backend, _useRenderDoc)
                .AddRenderer(new SpriteRenderer())
                .AddRenderer(new ExtrudedTileMapRenderer())
                .AddRenderer(new FullScreenQuad())
                .AddRenderer(new DebugGuiRenderer())
                .AddRenderer(new ScreenDuplicator())
                ;

            global
                .Register<IShaderCache>(new ShaderCache(
                    Path.Combine(baseDir, "Core", "Visual"),
                    Path.Combine(baseDir, "data", "ShaderCache")))
                .Attach(engine);

            var backgroundThreadInitTask = Task.Run(() =>
            {
                PerfTracker.StartupEvent("Creating main components");
                global
                    .Register<IInputManager>(new InputManager()
                        .RegisterInputMode(InputMode.ContextMenu, new ContextMenuInputMode())
                        .RegisterInputMode(InputMode.World2D, new World2DInputMode())
                        .RegisterMouseMode(MouseMode.DebugPick, new DebugPickMouseMode())
                        .RegisterMouseMode(MouseMode.Exclusive, new ExclusiveMouseMode())
                        .RegisterMouseMode(MouseMode.MouseLook, new MouseLookMouseMode())
                        .RegisterMouseMode(MouseMode.Normal, new NormalMouseMode())
                    )
                    .Register<ILayoutManager>(new LayoutManager())
                    .Register<IMapManager>(new MapManager())
                    .Register<IPaletteManager>(new PaletteManager())
                    .Register<ISceneManager>(new SceneManager()
                        .AddScene(new AutomapScene())
                        .AddScene(new FlatScene())
                        .AddScene(new DungeonScene())
                        .AddScene(new MenuScene())
                        .AddScene(new InventoryScene())
                    )
                    .Register<IClock>(new GameClock())
                    .Register<ISpriteResolver>(new SpriteResolver())
                    .Register<IGameState>(new GameState())
                    .Register<ITextManager>(new TextManager())
                    .Register<ITextureManager>(new TextureManager())
                    .Attach(new CursorManager())
                    .Attach(new DebugMapInspector())
                    .Attach(new InputBinder(InputConfig.Load(baseDir)))
                    .Attach(new InputModeStack())
                    .Attach(new MouseModeStack())
                    .Attach(new SceneStack())
                    .Attach(new StatusBar())
                    ;

                PerfTracker.StartupEvent("Creating scene-specific components");
                var inventoryConfig = InventoryConfig.Load(baseDir);
                global.Resolve<ISceneManager>().GetExchange(SceneId.Inventory)
                    .Attach(new InventoryScreen(inventoryConfig))
                    ;

                var menuBackground = new ScreenSpaceSprite<PictureId>(
                    PictureId.MenuBackground8,
                    new Vector2(0.0f, 1.0f),
                    new Vector2(2.0f, -2.0f));

                global.Resolve<ISceneManager>().GetExchange(SceneId.MainMenu)
                    .Attach(new MainMenu())
                    .Attach(menuBackground)
                    ;

                PerfTracker.StartupEvent("Starting new game");
                global.Raise(new NewGameEvent(), null);
                /*
                global.Raise(new LoadMapEvent(MapDataId.AltesFormergebäude), null); /*
                global.Raise(new LoadMapEvent(MapDataId.Jirinaar3D), null); /*
                global.Raise(new LoadMapEvent(MapDataId.HausDesJägerclans), null); //*/
                /*
                global.Raise(new SetSceneEvent(SceneId.Inventory), null);
                //*/

                // global.Raise(new SetSceneEvent((int)SceneId.MainMenu), null);
                ReflectionHelper.ClearTypeCache();
            });

            engine.Initialise();
            backgroundThreadInitTask.Wait();

            PerfTracker.StartupEvent("Running game");
            if(_startupOnly)
                global.Raise(new QuitEvent(), null);

            engine.Run();
        }
    }
}
