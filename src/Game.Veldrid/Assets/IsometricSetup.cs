using System;
using System.Collections.Generic;
using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Core.Veldrid;
using UAlbion.Core.Veldrid.Etm;
using UAlbion.Core.Veldrid.Sprites;
using UAlbion.Core.Veldrid.Textures;
using UAlbion.Core.Visual;
using UAlbion.Game.Scenes;
using UAlbion.Game.State;
using UAlbion.Game.Veldrid.Visual;
using Veldrid;

namespace UAlbion.Game.Veldrid.Assets;

public sealed class IsometricRenderSystem : Component, IRenderSystem, IDisposable
{
    readonly List<IRenderPass<GlobalSet>> _renderPasses = new();
    readonly GlobalResourceSetManager _globalManager;
    readonly MainRenderPass _mainPass;
    readonly SpriteRenderer _spriteRenderer;
    readonly EtmRenderer _etmRenderer;

    public SimpleFramebuffer Framebuffer { get; }
    public IsometricBuilder Builder { get; }

    public IsometricRenderSystem(int tileWidth, int tileHeight, int baseHeight, int tilesPerRow)
    {
        Framebuffer = AttachChild(new SimpleFramebuffer((uint)(tileWidth * tilesPerRow), (uint)tileHeight, "FB_Offscreen"));
        Builder = new IsometricBuilder(Framebuffer, tileWidth, tileHeight, baseHeight, tilesPerRow);
        var outputFormat = Framebuffer.OutputDescription ?? throw new InvalidOperationException("Offscreen framebuffer had no output description");

        _globalManager = AttachChild(new GlobalResourceSetManager());
        _spriteRenderer = AttachChild(new SpriteRenderer(outputFormat));
        _etmRenderer = AttachChild(new EtmRenderer(outputFormat));
        _mainPass = AttachChild(new MainRenderPass(Framebuffer));
        _mainPass.AddRenderer(_spriteRenderer);
        _mainPass.AddRenderer(_etmRenderer);

        var etmManager = AttachChild(new EtmManager());
        var spriteBatcher = AttachChild(new BatchManager<SpriteKey, SpriteInfo>(static (key, f) => f.CreateSpriteBatch(key)));
        _mainPass.AddSource(etmManager);
        _mainPass.AddSource(spriteBatcher);
        _renderPasses.Add(_mainPass);

        AttachChild(new SpriteSamplerSource());
        AttachChild(new TextureSource());
        AttachChild(new ResourceLayoutSource());
        AttachChild(new VeldridCoreFactory());
        AttachChild(new SceneStack());
        AttachChild(new SceneManager()
            .Add(new EmptyScene())
            .Add((IScene)new IsometricBakeScene()
                .Add(new PaletteManager())
                .Add(Builder)));
    }

    public void Render(GraphicsDevice graphicsDevice, CommandList frameCommands, FenceHolder fence)
    {
        if (graphicsDevice == null) throw new ArgumentNullException(nameof(graphicsDevice));
        if (frameCommands == null) throw new ArgumentNullException(nameof(frameCommands));
        if (fence == null) throw new ArgumentNullException(nameof(fence));

        foreach (var phase in _renderPasses)
        {
            frameCommands.Begin();
            phase.Render(graphicsDevice, frameCommands, _globalManager.GlobalSet);
            frameCommands.End();

            fence.Fence.Reset();
            graphicsDevice.SubmitCommands(frameCommands, fence.Fence);
            graphicsDevice.WaitForFence(fence.Fence);
        }
    }

    public void AddRenderPass(IRenderPass<GlobalSet> pass) => _renderPasses.Add(pass);

    public void Dispose()
    {
        _mainPass.Dispose();
        _spriteRenderer.Dispose();
        _etmRenderer.Dispose();
        Framebuffer.Dispose();
    }
}

