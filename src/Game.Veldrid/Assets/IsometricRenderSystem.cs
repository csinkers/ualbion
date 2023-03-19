using System;
using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Core.Veldrid;
using UAlbion.Core.Veldrid.Etm;
using UAlbion.Core.Veldrid.Sprites;
using UAlbion.Core.Veldrid.Textures;
using UAlbion.Core.Visual;
using UAlbion.Game.Scenes;
using UAlbion.Game.State;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Game.Veldrid.Assets;

public class IsometricRenderSystem : Component, IDisposable
{
    readonly RenderSystem _system;
    public RenderPipeline OnScreen { get; }
    public RenderPipeline OffScreen { get; }
    public IsometricBuilder Builder { get; }
    public IFramebufferHolder IsoBuffer { get; }

    public IsometricRenderSystem(int tileWidth, int tileHeight, int baseHeight, int tilesPerRow)
    {
        OutputDescription screenFormat = SimpleFramebuffer.Output;

        var sceneManager = AttachChild(new SceneManager());
        _system = RenderSystemBuilder.Create()
            .Framebuffer("fb_iso", new SimpleFramebuffer("fb_iso", (uint)(tileWidth * tilesPerRow), (uint)tileHeight))
            .Renderer("r_sprite", new SpriteRenderer(screenFormat))
            .Renderer("r_etm", new EtmRenderer(screenFormat))
            .Renderer("r_quad", new FullscreenQuadRenderer())
            .Source("s_sprite", new BatchManager<SpriteKey, SpriteInfo>(static (key, f) => f.CreateSpriteBatch(key)))
            .Source("s_etm", new EtmManager())
            .Pipeline("pl_onscreen", pipe => 
                pipe
                .Framebuffer("fb_screen", new MainFramebuffer("fb_screen"))
                .Resources(new GlobalResourceSetProvider())
                .Pass("p_iso", pb => 
                    pb
                    .Resources(new MainPassResourceProvider(pipe.GetFramebuffer("fb_iso"), sceneManager))
                    .Renderers("r_sprite", "r_etm")
                    .Sources("s_sprite", "s_etm")
                    .Target("fb_iso")
                    .Render(RenderFunc)
                    .Build())
                .Pass("p_copy", pb =>
                    pb
                    .Renderer("r_quad")
                    .Target("fb_screen")
                    .Render(CopyFunc)
                    .Build())
                .Build()
            )
            .Pipeline("pl_offscreen", pipe => 
                pipe
                .Resources(new GlobalResourceSetProvider())
                .Pass("p_iso", pb => 
                    pb
                    .Resources(new MainPassResourceProvider(pipe.GetFramebuffer("fb_iso"), sceneManager))
                    .Renderers("r_sprite", "r_etm")
                    .Sources("s_sprite", "s_etm")
                    .Target("fb_iso")
                    .Render(RenderFunc)
                    .Build())
                .Build()
            )
            .Build();

        OnScreen = _system.GetPipeline("pl_onscreen");
        OffScreen = _system.GetPipeline("pl_offscreen");
        IsoBuffer = _system.GetFramebuffer("fb_iso");
        AttachChild(_system);

        Builder = new IsometricBuilder(_system.GetFramebuffer("fb_iso"), sceneManager, tileWidth, tileHeight, baseHeight, tilesPerRow);

        AddHelpers();
        sceneManager
            .Add(new EmptyScene())
            .Add((IScene)new IsometricBakeScene()
                .Add(new PaletteManager())
                .Add(Builder));
    }

    void AddHelpers()
    {

        AttachChild(new SpriteSamplerSource());
        AttachChild(new TextureSource());
        AttachChild(new ResourceLayoutSource());
        AttachChild(new VeldridCoreFactory());
        AttachChild(new SceneStack());
    }

    void CopyFunc(RenderPass pass, GraphicsDevice device, CommandList cl, IResourceSetHolder set1)
    {
        throw new NotImplementedException();
    }

    static void RenderFunc(RenderPass pass, GraphicsDevice device, CommandList cl, IResourceSetHolder set1)
    {
        cl.SetFullViewports();
        cl.SetFullScissorRects();
        cl.ClearColorTarget(0, new RgbaFloat(0, 0, 0, 0.0f));
        cl.ClearDepthStencil(device.IsDepthRangeZeroToOne ? 1f : 0f);
        pass.CollectAndDraw(device, cl, set1);
    }

    public void Dispose()
    {
        _system?.Dispose();
        GC.SuppressFinalize(this);
    }
}

