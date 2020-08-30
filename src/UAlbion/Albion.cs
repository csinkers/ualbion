﻿using System;
using System.Globalization;
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
using UAlbion.Game.Veldrid.Editor;
using UAlbion.Game.Veldrid.Input;

namespace UAlbion
{
    static class Albion
    {
        public static void RunGame(EventExchange global, IContainer services, string baseDir, CommandLineOptions commandLine)
        {
            PerfTracker.StartupEvent("Creating engine");
#pragma warning disable CA2000 // Dispose objects before losing scopes
            using var engine = new VeldridEngine(commandLine.Backend, commandLine.UseRenderDoc)
                .AddRenderer(new SkyboxRenderer())
                .AddRenderer(new SpriteRenderer())
                .AddRenderer(new ExtrudedTileMapRenderer())
                .AddRenderer(new DebugGuiRenderer())
                ;

            var backgroundThreadInitTask = Task.Run(() => RegisterComponents(global, services, baseDir, commandLine));
            services
                .Add(new ShaderCache(
                    Path.Combine(baseDir, "src", "Core", "Visual", "Shaders"),
                    Path.Combine(baseDir, "data", "ShaderCache")))
                .Add(engine);
#pragma warning restore CA2000 // Dispose objects before losing scopes

            if (commandLine.DebugMenus)
                services.Add(new DebugMenus(engine));

            backgroundThreadInitTask.Wait();

            if (commandLine.StartupOnly)
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
                    global.Raise(new LoadGameEvent(ushort.Parse(commandLine.GameModeArgument, CultureInfo.InvariantCulture)), null);
                    break;
                case GameMode.LoadMap:
                    global.Raise(new NewGameEvent((MapDataId)int.Parse(commandLine.GameModeArgument, CultureInfo.InvariantCulture), 40, 40), null); 
                    break;
                case GameMode.Inventory:
                    global.Raise(new SetSceneEvent(SceneId.Inventory), null);
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Unexpected game mode \"{commandLine.GameMode}\"");
            }

            if (commandLine.Commands != null)
            {
                foreach(var command in commandLine.Commands)
                    global.Raise(Event.Parse(command), null);
            }

            engine.Run();
            // TODO: Ensure all sprite leases returned etc to weed out memory leaks
        }

        static void RegisterComponents(EventExchange global, IContainer services, string baseDir, CommandLineOptions commandLine)
        {
            PerfTracker.StartupEvent("Creating main components");
            var factory = global.Resolve<ICoreFactory>();

            global
                .Register<ICommonColors>(new CommonColors(factory))
                .Register(CoreConfig.Load(baseDir))
                .Register(GameConfig.Load(baseDir))
                ;

            if (commandLine.AudioMode == AudioMode.InProcess)
                services.Add(new AudioManager(false));

            services
                .Add(new GameState())
                .Add(new GameClock())
                .Add(new IdleClock())
                .Add(new SlowClock())
                .Add(new DeviceObjectManager())
                .Add(new SpriteManager())
                .Add(new TextureManager())
                .Add(new VideoManager())
                .Add(new EventChainManager())
                .Add(new Querier())
                .Add(new MapManager())
                .Add(new CollisionManager())
                .Add(new SceneStack())
                .Add(new SceneManager()
                    .AddScene((Scene)new EmptyScene()
                        .Add(new StatusBar())
                        .Add(new PaletteManager()))

                    .AddScene((Scene)new AutomapScene()
                        .Add(new StatusBar())
                        .Add(new PaletteManager()))

                    .AddScene((Scene)new FlatScene()
                        .Add(new StatusBar())
                        .Add(new ConversationManager())
                        .Add(new PaletteManager()))

                    .AddScene((Scene)new DungeonScene()
                        .Add(new StatusBar())
                        .Add(new ConversationManager())
                        .Add(new PaletteManager()))

                    .AddScene((Scene)new MenuScene()
                        .Add(new StatusBar())
                        .Add(new PaletteManager())
                        .Add(new MainMenu())
                        .Add(new Sprite<PictureId>(
                            PictureId.MenuBackground8,
                            new Vector3(-1.0f, 1.0f, 0),
                            DrawLayer.Interface,
                            SpriteKeyFlags.NoTransform,
                            SpriteFlags.LeftAligned) { Size = new Vector2(2.0f, -2.0f) }))

                    .AddScene((Scene)new InventoryScene()
                        .Add(new StatusBar())
                        .Add(new ConversationManager())
                        .Add(new PaletteManager())
                        .Add(new InventoryInspector()))

                    .AddScene((Scene)new EditorScene()
                        .Add(new PaletteManager())
                        .Add(new EditorUi()))
                )

                .Add(new TextFormatter())
                .Add(new TextManager())
                .Add(new LayoutManager())
                .Add(new DialogManager())
                .Add(new InventoryScreenManager())
                .Add(new DebugMapInspector(services)
                    .AddBehaviour(new SpriteInstanceDataDebugBehaviour())
                    .AddBehaviour(new FormatTextEventBehaviour())
                    .AddBehaviour(new QueryEventDebugBehaviour()))
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
                .Add(new InputBinder(InputConfig.Load(baseDir)))
                .Add(new ItemTransitionManager())
                ;
        }
    }
}
