using System;
using System.Linq;
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

        IsometricTest(CommandLineOptions cmdLine) => _cmdLine = cmdLine ?? throw new ArgumentNullException(nameof(cmdLine));

        public static void Run(EventExchange exchange, CommandLineOptions cmdLine)
        {
            using var test = new IsometricTest(cmdLine);
            exchange.Attach(test);
            test.Detach();
        }

        protected override void Subscribed()
        {
#pragma warning disable CA2000 // Dispose objects before losing scope
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

            Exchange.Attach(services);
            var config = Resolve<IGeneralConfig>();
            services
                .Add(new InputManager().RegisterMouseMode(MouseMode.Normal, new NormalMouseMode()))
                .Add(new InputBinder((disk, jsonUtil) => InputConfig.Load(config.BasePath, disk, jsonUtil)))
                ;

            var engine = (Engine)Resolve<IEngine>();
            _mainFramebuffer = new MainFramebuffer();
            _quadRenderer = new FullscreenQuadRenderer();
            var quad = new FullscreenQuad("Quad", DrawLayer.Compositing,
                ((SimpleFramebuffer)builder.Framebuffer).Color,
                new Vector4(0, 0, 1, 1));

            var firstPass = (RenderPass)engine.RenderPasses.First();

            void UpdateDestRectangle()
            {
                if (_mainFramebuffer.Width == 0 || _mainFramebuffer.Height == 0) return;

                var w = (float)firstPass.Framebuffer.Width;
                var h = (float)firstPass.Framebuffer.Height;
                var normW = w / _mainFramebuffer.Width;
                var normH = h / _mainFramebuffer.Height;
                quad.NormalisedDestRectangle = new Vector4(0, 0, normW, normH);
            }

            firstPass.Framebuffer.PropertyChanged += (_, _) => UpdateDestRectangle();
            _mainFramebuffer.PropertyChanged += (_, _) => UpdateDestRectangle();

            var source = new AdhocRenderableSource(new[] { quad });
            var copyPass = new RenderPass("Copy Pass", _mainFramebuffer);
            copyPass.AddSource(source);
            copyPass.AddRenderer(_quadRenderer, typeof(FullscreenQuad));

            engine.AddRenderPass(copyPass);

            services.Add(_mainFramebuffer);
            services.Add(_quadRenderer);
            services.Add(quad);
            services.Add(copyPass);

            Raise(new InputModeEvent(InputMode.IsoBake));
            Raise(new SetSceneEvent(SceneId.IsometricBake));
            Raise(new SetClearColourEvent(0, 0, 0, 0));
            Raise(new EngineFlagEvent(FlagOperation.Set, EngineFlags.ShowBoundingBoxes));

            Resolve<IEngine>().Run();
#pragma warning restore CA2000 // Dispose objects before losing scope
        }

        public void Dispose()
        {
            _mainFramebuffer?.Dispose();
            _quadRenderer?.Dispose();
        }
    }
}