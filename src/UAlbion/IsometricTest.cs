using System;
using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid;
using UAlbion.Formats;
using UAlbion.Formats.Config;
using UAlbion.Game.Events;
using UAlbion.Game.Input;
using UAlbion.Game.Scenes;
using UAlbion.Game.Veldrid.Assets;
using UAlbion.Game.Veldrid.Input;
using Rectangle = UAlbion.Core.Rectangle;

namespace UAlbion;

sealed class IsometricTest : Component, IDisposable // The engine construction code here should mostly parallel that in IsometricLabyrinthLoader.cs in Game.Veldrid
{
    readonly CommandLineOptions _cmdLine;
    MainFramebuffer _mainFramebuffer;
    CopyRenderPass _copyPass;
    ShaderLoader _shaderLoader;
    Engine _engine;
    IsometricRenderSystem _renderSystem;

    public IsometricTest(CommandLineOptions cmdLine) => _cmdLine = cmdLine ?? throw new ArgumentNullException(nameof(cmdLine));
    protected override void Subscribed()
    {
#pragma warning disable CA2000 // Dispose objects before losing scope
        var windowRect = new Rectangle(0, 0,
            IsometricLabyrinthLoader.DefaultWidth * IsometricLabyrinthLoader.DefaultTilesPerRow,
            IsometricLabyrinthLoader.DefaultHeight * 10);

        var pathResolver = Resolve<IPathResolver>();
        AttachChild(new ShaderCache(pathResolver.ResolvePath("$(CACHE)/ShaderCache")));
        _shaderLoader = AttachChild(new ShaderLoader());

        foreach (var shaderPath in Resolve<IModApplier>().ShaderPaths)
            _shaderLoader.AddShaderDirectory(shaderPath);

        _engine = AttachChild(new Engine(_cmdLine.Backend, _cmdLine.UseRenderDoc, true, windowRect));
        _renderSystem = AttachChild(new IsometricRenderSystem(
            IsometricLabyrinthLoader.DefaultWidth,
            IsometricLabyrinthLoader.DefaultHeight,
            IsometricLabyrinthLoader.DefaultBaseHeight,
            IsometricLabyrinthLoader.DefaultTilesPerRow));

        _engine.RenderSystem = _renderSystem;

        Raise(new SetSceneEvent(SceneId.IsometricBake));
        Raise(new SetClearColourEvent(0, 0, 0, 0));
        // Raise(new EngineFlagEvent(FlagOperation.Set, EngineFlags.ShowBoundingBoxes));

        AttachChild(new InputManager().RegisterMouseMode(MouseMode.Normal, new NormalMouseMode()));
        AttachChild(new InputBinder());

        _mainFramebuffer = AttachChild(new MainFramebuffer());
        _copyPass = AttachChild(new CopyRenderPass(_renderSystem.Framebuffer.Color, _mainFramebuffer));

        void UpdateDestRectangle()
        {
            if (_mainFramebuffer.Width == 0 || _mainFramebuffer.Height == 0) return;

            var w = (float)_renderSystem.Framebuffer.Width;
            var h = (float)_renderSystem.Framebuffer.Height;
            var normW = w / _mainFramebuffer.Width;
            var normH = h / _mainFramebuffer.Height;
            _copyPass.NormalisedDestRectangle = new Vector4(0, 0, normW, normH);
        }

        _renderSystem.Framebuffer.PropertyChanged += (_, _) => UpdateDestRectangle();
        _mainFramebuffer.PropertyChanged += (_, _) => UpdateDestRectangle();
        _renderSystem.AddRenderPass(_copyPass);

        Raise(new InputModeEvent(InputMode.IsoBake));
        Raise(new SetSceneEvent(SceneId.IsometricBake));
        Raise(new SetClearColourEvent(0, 0, 0, 0));
        Raise(new EngineFlagEvent(FlagOperation.Set, EngineFlags.ShowBoundingBoxes));
#pragma warning restore CA2000 // Dispose objects before losing scope
    }

    public void Dispose()
    {
        _mainFramebuffer?.Dispose();
        _copyPass?.Dispose();
    }
}