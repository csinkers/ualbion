using System;
using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid;
using UAlbion.Core.Veldrid.Etm;
using UAlbion.Core.Veldrid.Meshes;
using UAlbion.Core.Veldrid.Skybox;
using UAlbion.Core.Veldrid.Sprites;
using UAlbion.Core.Visual;
using UAlbion.Game.Veldrid.Visual;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion;

public class AlbionRenderSystem : Component, IDisposable
{
    readonly RenderSystem _system;
    readonly RenderPipeline _default;
    readonly RenderPipeline _debug;
    (float Red, float Green, float Blue, float Alpha) _clearColour;
    bool _debugMode;

    public AlbionRenderSystem()
    {
        OutputDescription screenFormat = new(
            new OutputAttachmentDescription(PixelFormat.R32_Float),
            new OutputAttachmentDescription(PixelFormat.B8_G8_R8_A8_UNorm));

        _system = RenderSystemBuilder.Create()
            .Renderer("r_sprite", new SpriteRenderer(screenFormat))
            .Renderer("r_blended", new BlendedSpriteRenderer(screenFormat))
            .Renderer("r_tile", new TileRenderer(screenFormat))
            .Renderer("r_etm", new EtmRenderer(screenFormat))
            .Renderer("r_mesh", new MeshRenderer(screenFormat))
            .Renderer("r_sky", new SkyboxRenderer(screenFormat))
            .Renderer("r_debug", new DebugGuiRenderer(screenFormat))

            .Source("s_sprite", new BatchManager<SpriteKey, SpriteInfo>(static (key, f) => f.CreateSpriteBatch(key)))
            .Source("s_blended", new BatchManager<SpriteKey, BlendedSpriteInfo>(static (key, f) => f.CreateBlendedSpriteBatch(key)))
            .Source("s_tile", new TileRenderableManager())
            .Source("s_etm", new EtmManager())
            .Source("s_mesh", new BatchManager<MeshId, GpuMeshInstanceData>(static (key, f) => ((VeldridCoreFactory)f).CreateMeshBatch(key)))
            .Source("s_sky", new SkyboxManager())
            .Source("s_debug", new DebugGuiRenderable())

            .Pipeline("pl_default", pb => 
                pb
                .Framebuffer("fb_screen", new MainFramebuffer("fb_screen"))
                .Resources(new GlobalResourceSetProvider())
                .Pass("p_game", passBuilder => 
                    passBuilder
                    .Renderers("r_sprite", "r_blended", "r_tile", "r_etm", "r_mesh", "r_sky")
                    .Sources("s_sprite", "s_blended", "s_tile", "s_etm", "s_mesh", "s_sky")
                    .Target("fb_screen")
                    .Resources(new MainPassResourceProvider(pb.GetFramebuffer("fb_screen")))
                    .Render(MainRenderFunc)
                    .Build()
                )
                .Build()
            )
            .Pipeline("pl_debug", pb => 
                pb
                .Framebuffer("fb_screen", new MainFramebuffer("fb_screen"))
                .Framebuffer("fb_game", new SimpleFramebuffer("fb_game", 1, 1))
                .Resources(new GlobalResourceSetProvider())
                .Pass("p_game", passBuilder => 
                    passBuilder
                    .Renderers("r_sprite", "r_blended", "r_tile", "r_etm", "r_mesh", "r_sky")
                    .Sources("s_sprite", "s_blended", "s_tile", "s_etm", "s_mesh", "s_sky")
                    .Target("fb_game")
                    .Resources(new MainPassResourceProvider(pb.GetFramebuffer("fb_game")))
                    .Render(MainRenderFunc)
                    .Build()
                )
                .Pass("Debug", passBuilder =>
                    passBuilder
                    .Renderer("r_debug")
                    .Source("s_debug")
                    .Target("fb_screen")
                    .Dependency("p_game")
                    .Render(DebugRenderFunc)
                    .Build()
                )
                .Build()
            )
            .Build();

        _default = _system.GetPipeline("pl_default");
        _debug = _system.GetPipeline("pl_debug");
        _default.IsActive = !_debugMode;
        _debug.IsActive = _debugMode;
        AttachChild(_system);

        On<ToggleDiagnosticsEvent>(_ =>
        {
            _debugMode = !_debugMode;
            SetRenderSystem();
        });

        On<SetClearColourEvent>(e => _clearColour = (e.Red, e.Green, e.Blue, e.Alpha));
    }

    protected override void Subscribed() => SetRenderSystem();

    void SetRenderSystem()
    {
        _default.IsActive = !_debugMode;
        _debug.IsActive = _debugMode;

        var engine = (Engine)Resolve<IEngine>();
        engine.RenderSystem = _debugMode ? _debug : _default;
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

    void DebugRenderFunc(RenderPass pass, GraphicsDevice device, CommandList cl, IResourceSetHolder set1)
    {
        cl.SetFramebuffer(pass.Target.Framebuffer);
        cl.SetFullViewports();
        cl.SetFullScissorRects();
        cl.ClearColorTarget(0, new RgbaFloat(_clearColour.Red, _clearColour.Green, _clearColour.Blue, _clearColour.Alpha));
        cl.ClearDepthStencil(device.IsDepthRangeZeroToOne ? 1f : 0f);
        pass.CollectAndDraw(device, cl, set1);
    }

    public void Dispose()
    {
        _system.Dispose();
    }
}