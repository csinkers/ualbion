using System.Numerics;
using UAlbion.Api;
using UAlbion.Api.Visual;
using UAlbion.Core;
using UAlbion.Core.Veldrid;
using UAlbion.Core.Veldrid.Etm;
using UAlbion.Core.Veldrid.Sprites;
using UAlbion.Core.Veldrid.Textures;
using UAlbion.Core.Visual;
using UAlbion.Formats.Config;
using UAlbion.Game;
using UAlbion.Game.Assets;
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
using UAlbion.Game.Text;
using UAlbion.Game.Veldrid.Audio;
using UAlbion.Game.Veldrid.Debugging;
using UAlbion.Editor;
using UAlbion.Formats.Assets;
using UAlbion.Game.Veldrid;
using UAlbion.Game.Veldrid.Input;

namespace UAlbion
{
    static class Albion
    {
        public static void RunGame(EventExchange global, IContainer services, string baseDir, CommandLineOptions commandLine)
        {
            RegisterComponents(global, services, baseDir, commandLine);

            PerfTracker.StartupEvent("Running game");
            global.Raise(new SetSceneEvent(SceneId.Empty), null);

            if (commandLine.Commands != null)
            {
                foreach (var command in commandLine.Commands)
                    global.Raise(Event.Parse(command), null);
            }
            else global.Raise(new SetSceneEvent(SceneId.MainMenu), null);

            global.Resolve<IEngine>().Run();
            // TODO: Ensure all sprite leases returned etc to weed out memory leaks
        }

        static void RegisterComponents(EventExchange global, IContainer services, string baseDir, CommandLineOptions commandLine)
        {
            PerfTracker.StartupEvent("Creating main components");

            global
                .Register<ICommonColors>(new CommonColors())
                ;

            if (!commandLine.Mute)
                services.Add(new AudioManager(false));

            services
                .Add(new VeldridGameFactory())
                .Add(new GameState())
                .Add(new GameClock())
                .Add(new IdleClock())
                .Add(new SlowClock())
                .Add(new EtmManager())
                .Add(new SpriteManager())
                .Add(new TextureSource())
                .Add(new SpriteSamplerSource())
                .Add(new VideoManager())
                .Add(new EventChainManager())
                .Add(new Querier())
                .Add(new MapManager())
                .Add(new CollisionManager())
                .Add(new SceneStack())
                .Add(new SceneManager()
                    .AddScene((IScene)new EmptyScene()
                        .Add(new StatusBar())
                        .Add(new PaletteManager()))

                    .AddScene((IScene)new AutomapScene()
                        .Add(new StatusBar())
                        .Add(new PaletteManager()))

                    .AddScene((IScene)new FlatScene()
                        .Add(new StatusBar())
                        .Add(new ConversationManager())
                        .Add(new PaletteManager())
                        .Add(new ClockWidget())
                        .Add(new MonsterEye()))

                    .AddScene((IScene)new DungeonScene()
                        .Add(new SceneGraph())
                        .Add(new StatusBar())
                        .Add(new ConversationManager())
                        .Add(new PaletteManager())
                        .Add(new ClockWidget())
                        .Add(new Compass())
                        .Add(new MonsterEye()))

                    .AddScene((IScene)new MenuScene()
                        .Add(new StatusBar())
                        .Add(new PaletteManager())
                        .Add(new MainMenu())
                        .Add(new Sprite(
                            (SpriteId)Base.Picture.MenuBackground8,
                            new Vector3(-1.0f, 1.0f, 0),
                            DrawLayer.Interface,
                            SpriteKeyFlags.NoTransform,
                            SpriteFlags.LeftAligned) { Size = new Vector2(2.0f, -2.0f) }))

                    .AddScene((IScene)new InventoryScene()
                        .Add(new StatusBar())
                        .Add(new ConversationManager())
                        .Add(new PaletteManager())
                        .Add(new InventoryInspector()))

                    .AddScene((IScene)new EditorScene()
                        .Add(new RawAssetManager())
                        .Add(new PaletteManager())
                        .Add(new EditorAssetManager())
                        .Add(new EditorUi()))
                )

                .Add(new TextFormatter())
                .Add(new TextManager())
                .Add(new LayoutManager())
                .Add(new DialogManager())
                .Add(new InventoryScreenManager())
                .Add(new DebugMapInspector(services)
                    .AddBehaviour(new SpriteInstanceDataDebugBehaviour())
                    .AddBehaviour(new FormatTextEventBehaviour()))
                    // .AddBehaviour(new QueryEventDebugBehaviour()))
                .Add(new ContextMenu())
                .Add(new CursorManager())
                .Add(new InputManager()
                    .RegisterInputMode(InputMode.ContextMenu, new ContextMenuInputMode())
                    .RegisterInputMode(InputMode.World2D, new World2DInputMode())
                    .RegisterMouseMode(MouseMode.DebugPick, new DebugPickMouseMode())
                    .RegisterMouseMode(MouseMode.MouseLook, new MouseLookMouseMode())
                    .RegisterMouseMode(MouseMode.Normal, new NormalMouseMode())
                    .RegisterMouseMode(MouseMode.RightButtonHeld, new RightButtonHeldMouseMode())
                    .RegisterMouseMode(MouseMode.ContextMenu, new ContextMenuMouseMode()))
                .Add(new SelectionManager())
                .Add(new InputBinder(disk => InputConfig.Load(baseDir, disk)))
                .Add(new ItemTransitionManager())
                ;
        }
    }
}
