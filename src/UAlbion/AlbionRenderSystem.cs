using System;
using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid;
using UAlbion.Core.Veldrid.Etm;
using UAlbion.Core.Veldrid.Events;
using UAlbion.Core.Veldrid.Meshes;
using UAlbion.Core.Veldrid.Skybox;
using UAlbion.Core.Veldrid.Sprites;
using UAlbion.Core.Visual;
using UAlbion.Game.Veldrid.Diag;
using UAlbion.Game.Veldrid.Visual;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion;

public sealed class AlbionRenderSystem : Component, IDisposable
{
    readonly RenderManager _manager;
    readonly RenderSystem _default;
    readonly RenderSystem _debug;
    (float Red, float Green, float Blue, float Alpha) _clearColour;
    bool _debugMode;
    bool _modeDirty;

    public AlbionRenderSystem(ICameraProvider mainCamera, IImGuiMenuManager menus)
    {
        OutputDescription screenFormat = new(
            new OutputAttachmentDescription(PixelFormat.D24_UNorm_S8_UInt),
            new OutputAttachmentDescription(PixelFormat.B8_G8_R8_A8_UNorm));

        _manager = RenderManagerBuilder.Create()
            .Renderer("r_sprite", new SpriteRenderer(screenFormat))
            .Renderer("r_blended", new BlendedSpriteRenderer(screenFormat))
            .Renderer("r_tile", new TileRenderer(screenFormat))
            .Renderer("r_etm", new EtmRenderer(screenFormat))
            .Renderer("r_mesh", new MeshRenderer(screenFormat))
            .Renderer("r_sky", new SkyboxRenderer(screenFormat))
            .Renderer("r_debug", new ImGuiRenderer(screenFormat))

            .Source("s_sprite",  new BatchManager<SpriteKey, SpriteInfo>(       static (key, f) => f.CreateSpriteBatch(key)))
            .Source("s_blended", new BatchManager<SpriteKey, BlendedSpriteInfo>(static (key, f) => f.CreateBlendedSpriteBatch(key)))
            .Source("s_mesh",    new BatchManager<MeshId, GpuMeshInstanceData>( static (key, f) => ((VeldridCoreFactory)f).CreateMeshBatch(key)))
            .Source("s_tile", new TileRenderableManager())
            .Source("s_etm", new EtmManager())
            .Source("s_sky", new SkyboxManager())
            .Source("s_debug", new DebugGuiRenderable())

            .System("sys_default", sys => 
                sys
                .Framebuffer("fb_screen", new MainFramebuffer("fb_screen"))
                .Component("c_inputrouter", new AdHocComponent("c_inputrouter",
                    static x =>
                    { 
                        // When running fullscreen, just echo the mouse input through to the game's mouse modes,
                        // when showing the debug UI the pass-through of input is done in ImGuiGameWindow.
                        var mouseEvent = new MouseInputEvent();
                        var keyboardEvent = new KeyboardInputEvent();
                        x.On<InputEvent>(e =>
                        {
                            keyboardEvent.DeltaSeconds   = e.DeltaSeconds;
                            keyboardEvent.KeyCharPresses = e.Snapshot.KeyCharPresses;
                            keyboardEvent.KeyEvents      = e.Snapshot.KeyEvents;
                            x.Raise(keyboardEvent);

                            mouseEvent.DeltaSeconds  = e.DeltaSeconds;
                            mouseEvent.MouseDelta    = e.MouseDelta;
                            mouseEvent.WheelDelta    = e.Snapshot.WheelDelta;
                            mouseEvent.MousePosition = e.Snapshot.MousePosition;
                            mouseEvent.MouseEvents   = e.Snapshot.MouseEvents;
                            mouseEvent.IsMouseDown   = e.Snapshot.IsMouseDown;
                            x.Raise(mouseEvent);
                        });
                    }))
                .Component("c_gamewindow", new GameWindow(1,1))
                .Component("c_windowupdater", // Minimal component to ensure the game resizes with the window
                    AdHocComponent.Build("c_windowupdater",
                        (GameWindow)sys.GetComponent("c_gamewindow"),
                        static (gameWindow, x) 
                            => x.On<WindowResizedEvent>(e => gameWindow.Resize(e.Width, e.Height))))
                .Resources(new GlobalResourceSetProvider())
                .Pass("p_game", pass => 
                    pass
                    .Renderers("r_sprite", "r_blended", "r_tile", "r_etm", "r_mesh", "r_sky")
                    .Sources("s_sprite", "s_blended", "s_tile", "s_etm", "s_mesh", "s_sky")
                    .Target("fb_screen")
                    .Resources(new MainPassResourceProvider(sys.GetFramebuffer("fb_screen"), mainCamera))
                    .Render(MainRenderFunc)
                    .Build()
                )
                .Build()
            )
            .System("sys_debug", sys => 
                sys
                .Framebuffer("fb_screen", new MainFramebuffer("fb_screen"))
                .Framebuffer("fb_game", new SimpleFramebuffer("fb_game", 360, 240))
                .Component("c_gamewindow", new GameWindow(360, 240))
                .Component("c_imgui", new ImGuiManager((ImGuiRenderer)sys.GetRenderer("r_debug")))
                .Action(() =>
                {
                    var framebuffer = sys.GetFramebuffer("fb_game");
                    var window =  (GameWindow)sys.GetComponent("c_gamewindow");
                    menus.AddMenuItem(new ShowWindowMenuItem(
                        "Game",
                        "Windows",
                        x => new ImGuiGameWindow(x, framebuffer, window)));

                    menus.AddMenuItem(new ShowWindowMenuItem(
                        "Positions",
                        "Windows",
                        name => new PositionsWindow(name, mainCamera)));
                })
                .Resources(new GlobalResourceSetProvider())
                .Pass("p_game", pass => 
                    pass
                    .Renderers("r_sprite", "r_blended", "r_tile", "r_etm", "r_mesh", "r_sky")
                    .Sources("s_sprite", "s_blended", "s_tile", "s_etm", "s_mesh", "s_sky")
                    .Target("fb_game")
                    .Resources(new MainPassResourceProvider(sys.GetFramebuffer("fb_game"), mainCamera))
                    .Render(MainRenderFunc)
                    .Build()
                )
                .Pass("Debug", pass =>
                    pass
                    .Renderer("r_debug")
                    .Source("s_debug")
                    .Target("fb_screen")
                    .ClearColor(RgbaFloat.Grey)
                    .Dependency("p_game")
                    .Build()
                )
                .Build()
            )
            .Build();

        AttachChild(_manager);

        _default = _manager.GetSystem("sys_default");
        _debug = _manager.GetSystem("sys_debug");

        On<ToggleDiagnosticsEvent>(_ =>
        {
            _debugMode = !_debugMode;
            _modeDirty = true;
        });

        On<BeginFrameEvent>(_ =>
        {
            if (_modeDirty)
                SetRenderSystem();
        });
        On<SetClearColourEvent>(e => _clearColour = (e.Red, e.Green, e.Blue, e.Alpha));
    }

    protected override void Subscribed() => SetRenderSystem();

    void SetRenderSystem()
    {
        var activeSystem = _debugMode ? _debug : _default;
        var inactiveSystem = _debugMode ? _default : _debug;

        inactiveSystem.IsActive = false; // Make sure both systems aren't active at the same time, or any overlapping ServiceComponents will throw
        activeSystem.IsActive = true;

        Enqueue(new ShowHardwareCursorEvent(_debugMode));

        var engine = (Engine)TryResolve<IEngine>();
        if (engine != null)
            engine.RenderSystem = _debugMode ? _debug : _default;

        _modeDirty = false;
    }

    void MainRenderFunc(RenderPass pass, GraphicsDevice device, CommandList cl, IResourceSetHolder set1)
    {
        cl.SetFramebuffer(pass.Target.Framebuffer);
        cl.SetFullViewports();
        cl.SetFullScissorRects();
        cl.ClearColorTarget(0, new RgbaFloat(_clearColour.Red, _clearColour.Green, _clearColour.Blue, _clearColour.Alpha));
        cl.ClearDepthStencil(device.IsDepthRangeZeroToOne ? 1f : 0f);
        pass.CollectAndDraw(device, cl, set1);
    }

    public void Dispose() => _manager.Dispose();
}
