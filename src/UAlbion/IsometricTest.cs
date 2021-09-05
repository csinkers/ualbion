using System;
using System.Numerics;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid;
using UAlbion.Formats.Config;
using UAlbion.Game.Events;
using UAlbion.Game.Input;
using UAlbion.Game.Scenes;
using UAlbion.Game.Veldrid;
using UAlbion.Game.Veldrid.Assets;
using UAlbion.Game.Veldrid.Input;

namespace UAlbion
{
    sealed class IsometricTest : Component, IDisposable // The engine construction code here should mostly parallel that in IsometricLabyrinthLoader.cs in Game.Veldrid
    {
        readonly CommandLineOptions _cmdLine;
        MainFramebuffer _mainFramebuffer;
        FullscreenQuadRenderer _quadRenderer;

        public static void Run(EventExchange exchange, CommandLineOptions cmdLine)
        {
            using var test = new IsometricTest(cmdLine);
            exchange.Attach(test);
            test.Detach();
        }

        IsometricTest(CommandLineOptions cmdLine)
        {
            _cmdLine = cmdLine ?? throw new ArgumentNullException(nameof(cmdLine));
        }

        protected override void Subscribed()
        {
            var (services, builder) = IsometricSetup.SetupEngine(Exchange,
                IsometricLabyrinthLoader.DefaultWidth,
                IsometricLabyrinthLoader.DefaultHeight,
                IsometricLabyrinthLoader.DefaultBaseHeight,
                IsometricLabyrinthLoader.DefaultTilesPerRow,
                _cmdLine.Backend,
                _cmdLine.UseRenderDoc,
                new Rectangle(0, 0,
                    IsometricLabyrinthLoader.DefaultWidth * IsometricLabyrinthLoader.DefaultTilesPerRow,
                    IsometricLabyrinthLoader.DefaultHeight * 10));

            var config = Resolve<IGeneralConfig>();
            services
                .Add(new InputManager().RegisterMouseMode(MouseMode.Normal, new NormalMouseMode()))
                .Add(new InputBinder((disk, jsonUtil) => InputConfig.Load(config.BasePath, disk, jsonUtil)))
                ;

            _mainFramebuffer = new MainFramebuffer();
            _quadRenderer = new FullscreenQuadRenderer();
            var quad = new FullscreenQuad("Quad", DrawLayer.MaxLayer,
                ((SimpleFramebuffer)builder.Framebuffer).Color,
                _mainFramebuffer,
                new Vector4(-1, -1, 2, 2));
            var source = new AdhocRenderableSource(new[] { quad });

            services.Add(_mainFramebuffer);
            services.Add(_quadRenderer);
            services.Add(quad);
            Exchange.Attach(services);

            var renderer = Resolve<ISceneRenderer>();
            renderer.AddRenderer(_quadRenderer, typeof(FullscreenQuad));
            renderer.AddSource(source);

            Raise(new InputModeEvent(InputMode.IsoBake));
            Raise(new SetSceneEvent(SceneId.IsometricBake));
            Raise(new SetClearColourEvent(0, 0, 0, 0));
            Raise(new EngineFlagEvent(FlagOperation.Set, EngineFlags.ShowBoundingBoxes));

            Resolve<IEngine>().Run();
        }

        public void Dispose()
        {
            _mainFramebuffer?.Dispose();
            _quadRenderer?.Dispose();
        }
    }
}