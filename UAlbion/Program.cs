using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Textures;
using UAlbion.Core.Visual;
using UAlbion.Formats;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Config;
using UAlbion.Game;
using UAlbion.Game.Assets;
using UAlbion.Game.Debugging;
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

        static void Main(string[] args)
        {
            //TransformTests();
            _startupOnly = args.Contains("--startuponly");
            _useRenderDoc = args.Contains("--renderdoc") || args.Contains("-rd");
            if (args.Contains("-gl") || args.Contains("--opengl")) _backend = GraphicsBackend.OpenGL;
            if (args.Contains("-gles") || args.Contains("--opengles")) _backend = GraphicsBackend.OpenGLES;
            if (args.Contains("-vk") || args.Contains("--vulkan")) _backend = GraphicsBackend.Vulkan;
            if (args.Contains("-metal") || args.Contains("--metal")) _backend = GraphicsBackend.Metal;
            if (args.Contains("-d3d") || args.Contains("--direct3d")) _backend = GraphicsBackend.Direct3D11;
            if (args.Contains("-h") || args.Contains("--help") || args.Contains("/?") || args.Contains("help"))
            {
                DisplayUsage();
                return;
            }

            PerfTracker.StartupEvent("Entered main");
            //GraphTests();
            //return;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // Required for code page 850 support in .NET Core
            PerfTracker.StartupEvent("Registered encodings");

            /*
            Console.WriteLine("Entry point reached. Press enter to continue");
            Console.ReadLine(); //*/

            var baseDir = FormatUtil.FindBasePath();
            if(baseDir == null)
                throw new InvalidOperationException("No base directory could be found.");

            PerfTracker.StartupEvent($"Found base directory {baseDir}");

            var logger = new ConsoleLogger();
            var settings = new Settings { BasePath = baseDir };
            PerfTracker.StartupEvent("Registering asset manager");
            using var assets = new AssetManager();
            using var global = new EventExchange("Global", logger);
            Engine.Global = global;

            global
                // Need to register settings first, as the AssetConfigLocator relies on it.
                .Register<ISettings>(settings) 
                .Register<IEngineSettings>(settings)
                .Register<IDebugSettings>(settings)
                .Register<IAssetManager>(assets)
                .Register<ITextureLoader>(assets)
                ;
            PerfTracker.StartupEvent("Registered asset manager");

            // Dump.CoreSprites(assets, baseDir);
            // Dump.CharacterSheets(assets);
            // Dump.Chests(assets);
            // Dump.ItemData(assets, baseDir);
            // Dump.MapEvents(assets, baseDir);
            // return;

            RunGame(global, baseDir);
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
                    Path.Combine(baseDir, "Core", "Visual", "Shaders"),
                    Path.Combine(baseDir, "data", "ShaderCache")))
                .Register<IEngine>(engine);

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
                        .RegisterMouseMode(MouseMode.RightButtonHeld, new RightButtonHeldMouseMode())
                        .RegisterMouseMode(MouseMode.ContextMenu, new ContextMenuMouseMode())
                    )
                    .Register<ILayoutManager>(new LayoutManager())
                    .Register<IMapManager>(new MapManager())
                    .Register<IPaletteManager>(new PaletteManager())
                    .Register<ISceneManager>(new SceneManager()
                        .AddScene(new EmptyScene())
                        .AddScene(new AutomapScene())
                        .AddScene(new FlatScene())
                        .AddScene(new DungeonScene())
                        .AddScene(new MenuScene())
                        .AddScene(new InventoryScene())
                    )
                    .Register<IClock>(new GameClock())
                    .Register<ISpriteManager>(new SpriteManager())
                    .Register<IGameState>(new GameState())
                    .Register<ITextManager>(new TextManager())
                    .Register<ITextureManager>(new TextureManager())
                    .Register<IDeviceObjectManager>(new DeviceObjectManager())
                    .Register<ICollisionManager>(new CollisionManager())
                    .Register<ICursorManager>(new CursorManager())
                    .Register<ISelectionManager>(new SelectionManager())
                    .Attach(new SlowClock())
                    .Attach(new DebugMapInspector()
                        .AddBehaviour(new SpriteInstanceDataDebugBehaviour())
                        .AddBehaviour(new FormatTextEventBehaviour()))
                    .Attach(new InputBinder(InputConfig.Load(baseDir)))
                    .Attach(new InputModeStack())
                    .Attach(new MouseModeStack())
                    .Attach(new SceneStack())
                    .Attach(new StatusBar())
                    .Attach(new ContextMenu())
                    .Attach(new EventChainManager())
                    ;

                PerfTracker.StartupEvent("Creating scene-specific components");
                var inventoryConfig = InventoryConfig.Load(baseDir);
                global.Resolve<ISceneManager>().GetExchange(SceneId.Inventory)
                    .Attach(new InventoryScreen(inventoryConfig))
                    ;

                var menuBackground = Sprite<PictureId>.ScreenSpaceSprite(
                    PictureId.MenuBackground8,
                    new Vector2(-1.0f, 1.0f),
                    new Vector2(2.0f, -2.0f));

                global.Resolve<ISceneManager>().GetExchange(SceneId.MainMenu)
                    .Attach(new MainMenu())
                    .Attach(menuBackground)
                    ;

                PerfTracker.StartupEvent("Starting new game");
                //global.Raise(new LoadMapEvent(MapDataId.Jirinaar3D), null); /*
                /*
                global.Raise(new LoadMapEvent(MapDataId.AltesFormergebäude), null); /*
                global.Raise(new LoadMapEvent(MapDataId.HausDesJägerclans), null); //*/
                /*
                global.Raise(new SetSceneEvent(SceneId.Inventory), null);
                //*/

                global.Raise(new SetSceneEvent(SceneId.MainMenu), null);
                //global.Raise(new NewGameEvent(), null);
                ReflectionHelper.ClearTypeCache();
            });

            engine.Initialise();
            backgroundThreadInitTask.Wait();

            PerfTracker.StartupEvent("Running game");
            if(_startupOnly)
                global.Raise(new QuitEvent(), null);

            engine.Run();
            // TODO: Ensure all sprite leases returned etc to weed out memory leaks
        }

        static void DisplayUsage()
        {
            Console.WriteLine(@"UAlbion
Command Line Options:

    -h --help /? help  : Display this help
    --startuponly      : Exit immediately after the first frame (for profiling startup time etc)
    -rd    --renderdoc : Load the RenderDoc plugin on startup

Set Rendering Backend:
    -gl    --opengl     Use OpenGL
    -gles  --opengles   Use OpenGLES
    -vk    --vulkan     Use Vulkan
    -metal --metal      Use Metal
    -d3d   --direct3d   Use Direct3D11
");
        }

        static void TransformTests()
        {
            Vector3 Transform(SpriteInstanceData instance, Vector3 vector)
            {
                var vec4 = new Vector4(vector, 1.0f);
                var m = instance.Transform;
                var transformed = Vector4.Transform(vec4, m);
                return new Vector3(transformed.X, transformed.Y, transformed.Z);
            }

            var origin = new Vector3();
            var oneX = new Vector3(1,0,0);
            var oneY = new Vector3(0,1,0);
            var oneZ = new Vector3(0,0,1);

            void Test(string name, SpriteInstanceData instance)
            {
                var origin2 = Transform(instance, origin);
                var x2 = Transform(instance, oneX);
                var y2 = Transform(instance, oneY);
                var z2 = Transform(instance, oneZ);
                Console.WriteLine(name + ":");
                Console.WriteLine($"  0: {origin2}");
                Console.WriteLine($"  X: {x2}");
                Console.WriteLine($"  Y: {y2}");
                Console.WriteLine($"  Z: {z2}");
            }

            SpriteInstanceData Make(Vector3 position, Vector2 size) =>
                SpriteInstanceData.TopLeft(
                    position, size,
                    Vector2.Zero, Vector2.One,
                    0, 0);


            Test("Neutral", Make(Vector3.Zero, Vector2.One));
            Test("+1X", Make(new Vector3(1,0,0), Vector2.One));
            Test("+1Y", Make(new Vector3(0,1,0), Vector2.One));
            Test("+1Z", Make(new Vector3(0,0,1), Vector2.One));
            Test("*2X", Make(Vector3.Zero, new Vector2(2, 1)));
            Test("*2Y", Make(Vector3.Zero, new Vector2(1, 2)));

            var x = Make(new Vector3(1, 0, 0), Vector2.One);
            x.OffsetBy(new Vector3(0,1,0));
            Test("+1X+1Y", x);

            Console.ReadLine();
        }
    }
}
