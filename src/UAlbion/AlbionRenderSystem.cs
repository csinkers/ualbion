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
using static UAlbion.Game.Veldrid.AlbionRenderSystemConstants;
using ImGuiRenderer = UAlbion.Core.Veldrid.ImGuiRenderer;

namespace UAlbion;

public sealed class AlbionRenderSystem : Component, IDisposable
{
    readonly RenderManager _manager;
    readonly IRenderSystem _default;
    readonly IRenderSystem _debug;
    (float Red, float Green, float Blue, float Alpha) _clearColour;
    bool _debugMode;
    bool _modeDirty;

    public AlbionRenderSystem(ICameraProvider mainCamera, IImGuiMenuManager menus)
    {
        OutputDescription screenFormat = new(
            new OutputAttachmentDescription(PixelFormat.D24_UNorm_S8_UInt),
            new OutputAttachmentDescription(PixelFormat.B8_G8_R8_A8_UNorm));

        var globalProvider1 = new GlobalResourceSetProvider();
        var globalProvider2 = new GlobalResourceSetProvider();

        _manager = RenderManagerBuilder.Create()
            .Renderer(R_Sprite, new SpriteRenderer(screenFormat))
            .Renderer(R_Blended, new BlendedSpriteRenderer(screenFormat))
            .Renderer(R_Tile, new TileRenderer(screenFormat))
            .Renderer(R_Etm, new EtmRenderer(screenFormat))
            .Renderer(R_Mesh, new MeshRenderer(screenFormat))
            .Renderer(R_Sky, new SkyboxRenderer(screenFormat))
            .Renderer(R_Debug, new ImGuiRenderer(screenFormat))

            .Source(S_Sprite,  new BatchManager<SpriteKey, SpriteInfo>(       static (key, f) => f.CreateSpriteBatch(key)))
            .Source(S_Blended, new BatchManager<SpriteKey, BlendedSpriteInfo>(static (key, f) => f.CreateBlendedSpriteBatch(key)))
            .Source(S_Mesh,    new BatchManager<MeshId, GpuMeshInstanceData>( static (key, f) => ((VeldridCoreFactory)f).CreateMeshBatch(key)))
            .Source(S_Tile, new TileRenderableManager())
            .Source(S_Etm, new EtmManager())
            .Source(S_Sky, new SkyboxManager())
            .Source(S_Debug, new DebugGuiRenderable())

            .System(Sys_Default, sys => 
                sys
                .Framebuffer(FB_Screen, new MainFramebuffer(FB_Screen))
                .Component(C_InputRouter, new AdHocComponent(C_InputRouter,
                    static x =>
                    { 
                        // When running fullscreen, just echo the mouse input through to the game's mouse modes,
                        // when showing the debug UI the pass-through of input is done in ImGuiGameWindow.
                        var mouseEvent = new MouseInputEvent();
                        var keyboardEvent = new KeyboardInputEvent();
                        x.On<InputEvent>(e =>
                        {
                            keyboardEvent.DeltaSeconds   = e.DeltaSeconds;
                            keyboardEvent.InputEvents    = e.Snapshot.InputEvents;
                            keyboardEvent.KeyEvents      = e.Snapshot.KeyEvents;
                            x.Raise(keyboardEvent);

                            mouseEvent.DeltaSeconds  = e.DeltaSeconds;
                            mouseEvent.MouseDelta    = e.MouseDelta;
                            mouseEvent.WheelDelta    = e.Snapshot.WheelDelta;
                            mouseEvent.MousePosition = e.Snapshot.MousePosition;
                            mouseEvent.MouseEvents   = e.Snapshot.MouseEvents;
                            mouseEvent.Snapshot      = e.Snapshot;
                            x.Raise(mouseEvent);
                        });
                    }))
                .Component(C_GameWindow, new GameWindow(1,1))
                .Component(C_WindowUpdater, // Minimal component to ensure the game resizes with the window
                    AdHocComponent.Build(C_WindowUpdater,
                        (GameWindow)sys.GetComponent(C_GameWindow),
                        static (gameWindow, x) 
                            => x.On<WindowResizedEvent>(e => gameWindow.Resize(e.Width, e.Height))))
                .Resources(globalProvider1)
                .Component("c_globalUpdater", new GlobalResourceSetUpdater(globalProvider1))
                .Pass(P_Game, pass => 
                    pass
                    .Renderers(R_Sprite, R_Blended, R_Tile, R_Etm, R_Mesh, R_Sky)
                    .Sources(S_Sprite, S_Blended, S_Tile, S_Etm, S_Mesh, S_Sky)
                    .Target(FB_Screen)
                    .Resources(new MainPassResourceProvider(sys.GetFramebuffer(FB_Screen), mainCamera))
                    .Render(MainRenderFunc)
                    .Build()
                )
                .Build()
            )
            .System(Sys_Debug, sys => 
                sys
                .Framebuffer(FB_Screen, new MainFramebuffer(FB_Screen))
                .Framebuffer(FB_Game, new SimpleFramebuffer(FB_Game, 360, 240))
                .Component(C_GameWindow, new GameWindow(360, 240))
                .Component(C_ImGui, new ImGuiManager((ImGuiRenderer)sys.GetRenderer(R_Debug)))
                .Action(() =>
                {
                    var framebuffer = sys.GetFramebuffer(FB_Game);
                    var window =  (GameWindow)sys.GetComponent(C_GameWindow);
                    menus.AddMenuItem(new ShowWindowMenuItem(
                        "Game",
                        "Windows",
                        x => new ImGuiGameWindow(x, framebuffer, window)));

                    menus.AddMenuItem(new ShowWindowMenuItem(
                        "Positions",
                        "Windows",
                        name => new PositionsWindow(name, mainCamera)));
                })
                .Resources(globalProvider2)
                .Component("c_globalUpdater", new GlobalResourceSetUpdater(globalProvider2))
                .Pass(P_Game, pass => 
                    pass
                    .Renderers(R_Sprite, R_Blended, R_Tile, R_Etm, R_Mesh, R_Sky)
                    .Sources(S_Sprite, S_Blended, S_Tile, S_Etm, S_Mesh, S_Sky)
                    .Target(FB_Game)
                    .Resources(new MainPassResourceProvider(sys.GetFramebuffer(FB_Game), mainCamera))
                    .Render(MainRenderFunc)
                    .Build()
                )
                .Pass(P_Debug, pass =>
                    pass
                    .Renderer(R_Debug)
                    .Source(S_Debug)
                    .Target(FB_Screen)
                    .ClearColor(RgbaFloat.Grey)
                    .Dependency(P_Game)
                    .Build()
                )
                .Build()
            )
            .Build();

        AttachChild(_manager);

        _default = _manager.GetSystem(Sys_Default);
        _debug = _manager.GetSystem(Sys_Debug);

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
