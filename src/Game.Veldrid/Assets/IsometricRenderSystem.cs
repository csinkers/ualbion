using System;
using System.Diagnostics.CodeAnalysis;
using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Core.Veldrid;
using UAlbion.Core.Veldrid.Etm;
using UAlbion.Core.Veldrid.Sprites;
using UAlbion.Core.Veldrid.Textures;
using UAlbion.Core.Visual;
using UAlbion.Formats;
using UAlbion.Formats.Ids;
using UAlbion.Game.Scenes;
using UAlbion.Game.State;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Game.Veldrid.Assets;

[SuppressMessage("", "CA2000")] // Analysis thinks disposable resources won't get cleaned up, but the manager, systems & passes will.
[SuppressMessage("", "CA2213")] // Analysis thinks disposable resources won't get cleaned up, but the manager, systems & passes will.
public sealed class IsometricRenderSystem : Component, IDisposable
{
    readonly RenderManager _manager;
    public RenderSystem OnScreen { get; }
    public RenderSystem OffScreen { get; }
    public IsometricBuilder Builder { get; }
    public IFramebufferHolder IsoBuffer { get; }

    public IsometricRenderSystem(int tileWidth, int tileHeight, int baseHeight, int tilesPerRow)
    {
        OutputDescription screenFormat = SimpleFramebuffer.Output;

        var sceneManager = AttachChild(new SceneManager());
        _manager = RenderManagerBuilder.Create()
            .Framebuffer("fb_iso", new SimpleFramebuffer("fb_iso", (uint)(tileWidth * tilesPerRow), (uint)tileHeight))
            .Renderer("r_sprite", new SpriteRenderer(screenFormat))
            .Renderer("r_etm", new EtmRenderer(screenFormat))
            .Renderer("r_quad", new FullscreenQuadRenderer())
            .Source("s_sprite", new BatchManager<SpriteKey, SpriteInfo>(static (key, f) => f.CreateSpriteBatch(key)))
            .Source("s_etm", new EtmManager())
            .System("sys_onscreen", pipe => 
                pipe
                .Framebuffer("fb_screen", new MainFramebuffer("fb_screen"))
                .Resources(new GlobalResourceSetProvider())
                .Pass("p_iso", pb => 
                    pb
                    .Resources(new MainPassResourceProvider(pipe.GetFramebuffer("fb_iso"), sceneManager))
                    .Renderers("r_sprite", "r_etm")
                    .Sources("s_sprite", "s_etm")
                    .Target("fb_iso")
                    .Build())
                .Pass("p_copy", pb =>
                    pb
                    .Renderer("r_quad")
                    .Target("fb_screen")
                    .Build())
                .Build()
            )
            .System("sys_offscreen", pipe => 
                pipe
                .Resources(new GlobalResourceSetProvider())
                .Pass("p_iso", pb => 
                    pb
                    .Resources(new MainPassResourceProvider(pipe.GetFramebuffer("fb_iso"), sceneManager))
                    .Renderers("r_sprite", "r_etm")
                    .Sources("s_sprite", "s_etm")
                    .Target("fb_iso")
                    .Build())
                .Build()
            )
            .Build();

        OnScreen = _manager.GetSystem("sys_onscreen");
        OffScreen = _manager.GetSystem("sys_offscreen");
        IsoBuffer = _manager.GetFramebuffer("fb_iso");
        AttachChild(_manager);

        Builder = new IsometricBuilder(_manager.GetFramebuffer("fb_iso"), sceneManager, tileWidth, tileHeight, baseHeight, tilesPerRow);

        AddHelpers();
        sceneManager
            .Add(new EmptyScene())
            .Add((IScene)new IsometricBakeScene()
                .Add(new PaletteManager())
                .Add(Builder));
    }

    void AddHelpers()
    {
        Mesh LoadMesh(MeshId id)
        {
            var assets = Resolve<IAssetManager>();
            if (assets.LoadMapObject((MapObjectId)id.Id) is not Mesh mesh)
                throw new InvalidOperationException($"Could not load mesh for {id}");

            return mesh;
        }

        AttachChild(new SpriteSamplerSource());
        AttachChild(new TextureSource());
        AttachChild(new ResourceLayoutSource());
        AttachChild(new VeldridCoreFactory(LoadMesh));
        AttachChild(new SceneStack());
    }

    public void Dispose()
    {
        foreach(var child in Children)
            if (child is IDisposable disposable)
                disposable.Dispose();
        RemoveAllChildren();
        GC.SuppressFinalize(this);
    }
}

