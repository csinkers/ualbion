using System;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid;
using UAlbion.Core.Veldrid.Textures;
using UAlbion.Core.Veldrid.Visual;
using UAlbion.Core.Visual;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Config;
using UAlbion.Game;
using UAlbion.Game.Debugging;
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
                .Attach(new ShaderCache(
                    Path.Combine(baseDir, "Core", "Visual", "Shaders"),
                    Path.Combine(baseDir, "data", "ShaderCache")))
                .Attach(engine);

            var backgroundThreadInitTask = Task.Run(() => RegisterComponents(global, baseDir, commandLine));
            engine.Initialise();
            backgroundThreadInitTask.Wait();

            if(commandLine.StartupOnly)
                global.Raise(new QuitEvent(), null);

            PerfTracker.StartupEvent("Running game");
            global.Raise(new SetSceneEvent(SceneId.Empty), null);
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
                .Register<ICommonColors>(new CommonColors(factory))
                .Attach(new GameClock())
                .Attach(new IdleClock())
                .Attach(new SlowClock())
                .Attach(new CollisionManager())
                .Attach(new CursorManager())
                .Attach(new DeviceObjectManager())
                .Attach(new GameState())
                .Attach(new LayoutManager())
                .Attach(new InputManager()
                    .RegisterInputMode(InputMode.ContextMenu, new ContextMenuInputMode())
                    .RegisterInputMode(InputMode.World2D, new World2DInputMode())
                    .RegisterMouseMode(MouseMode.DebugPick, new DebugPickMouseMode())
                    .RegisterMouseMode(MouseMode.MouseLook, new MouseLookMouseMode())
                    .RegisterMouseMode(MouseMode.Normal, new NormalMouseMode())
                    .RegisterMouseMode(MouseMode.RightButtonHeld, new RightButtonHeldMouseMode())
                    .RegisterMouseMode(MouseMode.ContextMenu, new ContextMenuMouseMode())
                )
                .Attach(new MapManager())
                .Attach(new SelectionManager())
                .Attach(new SceneManager()
                    .AddScene((GameScene)new EmptyScene()
                        .Add(new PaletteManager()))
                    .AddScene((GameScene)new AutomapScene()
                        .Add(new PaletteManager()))
                    .AddScene((GameScene)new FlatScene()
                        .Add(new PaletteManager()))
                    .AddScene((GameScene)new DungeonScene()
                        .Add(new PaletteManager()))
                    .AddScene((GameScene)new MenuScene()
                        .Add(new PaletteManager()))
                    .AddScene((GameScene)new InventoryScene()
                        .Add(new PaletteManager()))
                )
                .Attach(new SpriteManager())
                .Attach(new TextManager())
                .Attach(new TextureManager())
                .Attach(new DebugMapInspector()
                    .AddBehaviour(new SpriteInstanceDataDebugBehaviour())
                    .AddBehaviour(new FormatTextEventBehaviour())
                    .AddBehaviour(new QueryEventDebugBehaviour())
                )
                .Attach(new InputBinder(InputConfig.Load(baseDir)))
                .Attach(new SceneStack())
                .Attach(new StatusBar())
                .Attach(new ContextMenu())
                .Attach(new EventChainManager())
                .Attach(new Querier())
                ;

            PerfTracker.StartupEvent("Creating scene-specific components");
            var inventoryConfig = InventoryConfig.Load(baseDir);
            global.Resolve<ISceneManager>().GetScene(SceneId.Inventory)
                .Add(new InventoryScreen(inventoryConfig))
                ;

            var menuBackground = Sprite<PictureId>.ScreenSpaceSprite(
                PictureId.MenuBackground8,
                new Vector2(-1.0f, 1.0f),
                new Vector2(2.0f, -2.0f));

            global.Resolve<ISceneManager>().GetScene(SceneId.MainMenu)
                .Add(new MainMenu())
                .Add(menuBackground)
                ;
        }
    }
}
