using System;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Textures;
using UAlbion.Core.Veldrid;
using UAlbion.Core.Veldrid.Textures;
using UAlbion.Core.Veldrid.Visual;
using UAlbion.Core.Visual;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Config;
using UAlbion.Game;
using UAlbion.Game.Debugging;
using UAlbion.Game.Entities;
using UAlbion.Game.Events;
using UAlbion.Game.Gui;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Inventory;
using UAlbion.Game.Gui.Menus;
using UAlbion.Game.Gui.Status;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.Input;
using UAlbion.Game.Scenes;
using UAlbion.Game.State;
using UAlbion.Game.Veldrid.Audio;
using UAlbion.Game.Veldrid.Debugging;
using UAlbion.Game.Veldrid.Input;

namespace UAlbion
{
    static class Albion
    {
        public static void RunGame(EventExchange global, string baseDir, CommandLineOptions commandLine)
        {
            PerfTracker.StartupEvent("Creating engine");
            using var engine = new VeldridEngine(commandLine.Backend, commandLine.UseRenderDoc)
                .AddRenderer(new SkyboxRenderer())
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

            var backgroundThreadInitTask = Task.Run(() => RegisterComponents(global, baseDir, commandLine));
            engine.Initialise();
            backgroundThreadInitTask.Wait();

            if(commandLine.StartupOnly)
                global.Raise(new QuitEvent(), null);

            PerfTracker.StartupEvent("Running game");
            switch(commandLine.GameMode)
            {
                case GameMode.MainMenu:
                    global.Raise(new SetSceneEvent(SceneId.MainMenu), null);
                    break;
                case GameMode.NewGame:
                    global.Raise(new NewGameEvent(MapDataId.Toronto2DGesamtkarteSpielbeginn, 30, 75), null);
                    break;
                case GameMode.LoadGame:
                    global.Raise(new LoadGameEvent(commandLine.GameModeArgument), null);
                    break;
                case GameMode.LoadMap:
                    global.Raise(new NewGameEvent((MapDataId)int.Parse(commandLine.GameModeArgument), 40, 40), null); 
                    break;
                case GameMode.Inventory:
                    global.Raise(new SetSceneEvent(SceneId.Inventory), null);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            // global.Raise(new StartDialogueEvent((EventSetId)112), null);
            engine.Run();
            // TODO: Ensure all sprite leases returned etc to weed out memory leaks
        }

        static void RegisterComponents(EventExchange global, string baseDir, CommandLineOptions commandLine)
        {
            PerfTracker.StartupEvent("Creating main components");
            var factory = global.Resolve<ICoreFactory>();

            if (commandLine.AudioMode == AudioMode.InProcess)
                global.Register<IAudioManager>(new AudioManager(false));

            global
                .Register<IClock>(new GameClock())
                .Register<ICollisionManager>(new CollisionManager())
                .Register<ICommonColors>(new CommonColors(factory))
                .Register<ICursorManager>(new CursorManager())
                .Register<IDeviceObjectManager>(new DeviceObjectManager())
                .Register<IGameState>(new GameState())
                .Register<ILayoutManager>(new LayoutManager())
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
                .Register<IMapManager>(new MapManager())
                .Register<IPaletteManager>(new PaletteManager())
                .Register<ISelectionManager>(new SelectionManager())
                .Register<ISceneManager>(new SceneManager()
                    .AddScene(new EmptyScene())
                    .AddScene(new AutomapScene())
                    .AddScene(new FlatScene())
                    .AddScene(new DungeonScene())
                    .AddScene(new MenuScene())
                    .AddScene(new InventoryScene())
                )
                .Register<ISpriteManager>(new SpriteManager())
                .Register<ITextManager>(new TextManager())
                .Register<ITextureManager>(new TextureManager())
                .Attach(new SlowClock())
                .Attach(new DebugMapInspector()
                    .AddBehaviour(new SpriteInstanceDataDebugBehaviour())
                    .AddBehaviour(new FormatTextEventBehaviour())
                    .AddBehaviour(new QueryEventDebugBehaviour())
                )
                .Attach(new InputBinder(InputConfig.Load(baseDir)))
                .Attach(new InputModeStack())
                .Attach(new MouseModeStack())
                .Attach(new SceneStack())
                .Attach(new StatusBar())
                .Attach(new ContextMenu())
                .Attach(new EventChainManager())
                .Attach(new Querier())
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

            ReflectionHelper.ClearTypeCache();
        }
    }
}
