using System;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Veldrid;
using UAlbion.Core.Veldrid.Etm;
using UAlbion.Core.Veldrid.Sprites;
using UAlbion.Core.Veldrid.Textures;
using UAlbion.Core.Visual;
using UAlbion.Formats;
using UAlbion.Game.Scenes;
using UAlbion.Game.State;
using Veldrid;
using Rectangle = UAlbion.Core.Rectangle;

namespace UAlbion.Game.Veldrid.Assets;

public static class IsometricSetup
{
    public static (Container, IsometricBuilder) SetupEngine(
        EventExchange exchange,
        int tileWidth, int tileHeight,
        int baseHeight, int tilesPerRow,
        GraphicsBackend backend, bool useRenderDoc, Rectangle? windowRect)
    {
        if (exchange == null) throw new ArgumentNullException(nameof(exchange));
        var offscreenFB = new SimpleFramebuffer((uint)(tileWidth * tilesPerRow), (uint)tileHeight, "FB_Offscreen");
        var builder = new IsometricBuilder(offscreenFB, tileWidth, tileHeight, baseHeight, tilesPerRow);

#pragma warning disable CA2000 // Dispose objects before losing scopes
        var pathResolver = exchange.Resolve<IPathResolver>();
        var shaderCache = new ShaderCache(pathResolver.ResolvePath("$(CACHE)/ShaderCache"));
        var shaderLoader = new ShaderLoader();
        var mainPass = new RenderPass("Iso Render Pass", offscreenFB)
                .AddRenderer(new SpriteRenderer(offscreenFB))
                .AddRenderer(new EtmRenderer(offscreenFB))
            ;

        foreach (var shaderPath in exchange.Resolve<IModApplier>().ShaderPaths)
            shaderLoader.AddShaderDirectory(shaderPath);

        var engine = new Engine(backend, useRenderDoc, false, windowRect != null, windowRect);
        engine.AddRenderPass(mainPass);

        var renderableSources = new IRenderableSource[]
        {
            new EtmManager(),
            new SpriteManager<SpriteInfo>(),
        };

        var services = new Container("IsometricLayoutServices");
        services
            .Add(shaderCache)
            .Add(shaderLoader)
            .Add(offscreenFB)
            .Add(mainPass)
            .Add(engine)
            .Add(new SpriteSamplerSource())
            .Add(new TextureSource())
            .Add(new ResourceLayoutSource())
            .Add(new VeldridCoreFactory())
            .Add(new SceneStack())
            .Add(new SceneManager()
                .AddScene(new EmptyScene())
                .AddScene((IScene)new IsometricBakeScene()
                    .Add(new PaletteManager())
                    .Add(builder)))
            ;

        foreach (var source in renderableSources)
        {
            if (source is IComponent component)
                services.Add(component);
            mainPass.AddSource(source);
        }

        return (services, builder);
    }
}
