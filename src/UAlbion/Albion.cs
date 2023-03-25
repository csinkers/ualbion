using System;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Core;
using UAlbion.Core.Veldrid;
using UAlbion.Core.Veldrid.Sprites;
using UAlbion.Core.Veldrid.Textures;
using UAlbion.Core.Visual;
using UAlbion.Editor;
using UAlbion.Formats;
using UAlbion.Formats.Config;
using UAlbion.Formats.Ids;
using UAlbion.Game;
using UAlbion.Game.Assets;
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
using UAlbion.Game.Veldrid;
using UAlbion.Game.Veldrid.Audio;
using UAlbion.Game.Veldrid.Input;

namespace UAlbion;

static class Albion
{
    public static void RunGame(EventExchange global, CommandLineOptions commandLine)
    {
        RegisterComponents(global, commandLine);

        PerfTracker.StartupEvent("Running game");
        global.Raise(new SetSceneEvent(SceneId.Empty), null);

        if (commandLine.Commands != null)
        {
            foreach (var command in commandLine.Commands)
            {
                var e = Event.Parse(command, out var error);
                if (e == null)
                    ApiUtil.Assert(error);
                else
                    global.Raise(e, null);
            }
        }
        else global.Raise(new SetSceneEvent(SceneId.MainMenu), null);

        var engine = (Engine)global.Resolve<IEngine>();
        engine.Run();
        // TODO: Ensure all sprite leases returned etc to weed out memory leaks
    }

    static void RegisterComponents(EventExchange global, CommandLineOptions commandLine)
    {
#pragma warning disable CA2000 // Dispose objects before losing scope
        PerfTracker.StartupEvent("Creating main components");
        global.Register<ICommonColors>(new CommonColors());

        var sceneManager = new SceneManager();
        sceneManager
            .Add(new EmptyScene()
                .Add(new StatusBar())
                .Add(new PaletteManager()))

            .Add(new AutomapScene()
                .Add(new StatusBar())
                .Add(new PaletteManager()))

            .Add(new FlatScene()
                .Add(new StatusBar())
                .Add(new ConversationManager())
                .Add(new PaletteManager())
                .Add(new ClockWidget())
                .Add(new MonsterEye()))

            .Add(new DungeonScene()
                .Add(new SceneGraph())
                .Add(new StatusBar())
                .Add(new ConversationManager())
                .Add(new PaletteManager())
                .Add(new ClockWidget())
                .Add(new Compass())
                .Add(new MonsterEye()))

            .Add(new MenuScene()
                .Add(new StatusBar())
                .Add(new PaletteManager())
                .Add(new MainMenu())
                .Add(new Sprite(
                    (SpriteId)Base.Picture.MenuBackground8,
                    new Vector3(-1.0f, 1.0f, 0),
                    DrawLayer.Interface,
                    SpriteKeyFlags.NoTransform,
                    SpriteFlags.LeftAligned) { Size = new Vector2(2.0f, -2.0f) }))

            .Add(new InventoryScene()
                .Add(new StatusBar())
                .Add(new ConversationManager())
                .Add(new PaletteManager())
                .Add(new InventoryInspector()))

            .Add(new EditorScene()
                .Add(new RawAssetManager())
                .Add(new PaletteManager())
                .Add(new EditorAssetManager())
                .Add(new EditorUi()));

        Mesh LoadMesh(MeshId id)
        {
            var assets = global.Resolve<IAssetManager>();
            var objId = new MapObjectId(id.Id);
            var mapObj = assets.LoadMapObject(objId);
            if (mapObj is not Mesh mesh)
                throw new InvalidOperationException($"Could not load mesh for {id}");

            return mesh;
        }

        var gameServices = new Container("Game",
            sceneManager,
            new AlbionRenderSystem(sceneManager),
            new TextureSource(),
            new VeldridGameFactory(LoadMesh),
            new MeshManager(LoadMesh),
            new GameState(),
            new GameClock(),
            new IdleClock(),
            new SlowClock(),
            new RandomNumberGenerator(),
            new SpriteSamplerSource(),
            new VideoManager(),
            new EventChainManager(),
            new Querier(),
            new MapManager(),
            new CollisionManager(),
            new SceneStack(),
            new TextFormatter(),
            new TextManager(),
            new LayoutManager(),
            new DialogManager(),
            new InventoryScreenManager(),
            //new DiagWindow()
            //    .Add(new SpriteInstanceDataDebugBehaviour())
            //    .Add(new FormatTextEventBehaviour()),
            //    // .Add(new QueryEventDebugBehaviour()))
            new ContextMenu(),
            new CursorManager(),
            new InputManager()
                .RegisterInputMode(InputMode.ContextMenu, new ContextMenuInputMode())
                .RegisterInputMode(InputMode.World2D, new World2DInputMode())
                .RegisterInputMode(InputMode.TextEntry, new TextEntryInputMode())
                .RegisterMouseMode(MouseMode.DebugPick, new DebugPickMouseMode())
                .RegisterMouseMode(MouseMode.MouseLook, new MouseLookMouseMode())
                .RegisterMouseMode(MouseMode.Normal, new NormalMouseMode())
                .RegisterMouseMode(MouseMode.RightButtonHeld, new RightButtonHeldMouseMode())
                .RegisterMouseMode(MouseMode.ContextMenu, new ContextMenuMouseMode()),
            new SelectionManager(),
            new InputBinder(),
            new ItemTransitionManager())
            ;

        if (!commandLine.Mute)
            gameServices.Add(new AudioManager(false));

        global.Attach(gameServices);

#if DEBUG
        G.Instance.Attach(global); // Add convenience class that holds globals for debugging
#endif
#pragma warning restore CA2000 // Dispose objects before losing scope
    }
}