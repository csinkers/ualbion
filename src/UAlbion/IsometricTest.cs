using System;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid;
using UAlbion.Core.Veldrid.Sprites;
using UAlbion.Core.Veldrid.Textures;
using UAlbion.Core.Visual;
using UAlbion.Formats.Config;
using UAlbion.Game;
using UAlbion.Game.Assets;
using UAlbion.Game.Events;
using UAlbion.Game.Input;
using UAlbion.Game.Scenes;
using UAlbion.Game.State;
using UAlbion.Game.Veldrid;
using UAlbion.Game.Veldrid.Assets;
using UAlbion.Game.Veldrid.Input;

namespace UAlbion
{
    class IsometricTest : Component // The engine construction code here should mostly parallel that in IsometricLabyrinthLoader.cs in Game.Veldrid
    {
        readonly CommandLineOptions _cmdLine;

        public static void Run(EventExchange exchange, CommandLineOptions cmdLine)
        {
            var test = new IsometricTest(cmdLine);
            exchange.Attach(test);
        }

        IsometricTest(CommandLineOptions cmdLine)
        {
            _cmdLine = cmdLine ?? throw new ArgumentNullException(nameof(cmdLine));
        }

        protected override void Subscribed()
        {
#pragma warning disable CA2000 // Dispose objects before losing scopes
            var config = Resolve<IGeneralConfig>();
            var shaderCache = new ShaderCache(config.ResolvePath("$(CACHE)/ShaderCache"));

            foreach (var shaderPath in Resolve<IModApplier>().ShaderPaths)
                shaderCache.AddShaderPath(shaderPath);

            var framebuffer = new OffscreenFramebuffer(640, 480);
            var renderer = new SceneRenderer("IsoRenderer", framebuffer)
                    .AddRenderer(new EtmRenderer(framebuffer))
                    .AddRenderer(new SpriteRenderer(framebuffer))
                    .AddSource(new DefaultRenderableSource());

            var engine = new Engine(_cmdLine.Backend, _cmdLine.UseRenderDoc, _cmdLine.StartupOnly, true, renderer);

#pragma warning restore CA2000 // Dispose objects before losing scopes

            var builder = new IsometricBuilder(
                null,
                IsometricLabyrinthLoader.DefaultWidth,
                IsometricLabyrinthLoader.DefaultHeight,
                IsometricLabyrinthLoader.DefaultBaseHeight,
                IsometricLabyrinthLoader.DefaultTilesPerRow);

            var services = AttachChild(new Container("IsoServices"));
            services
                .Add(shaderCache)
                .Add(framebuffer)
                .Add(renderer)
                .Add(engine)
                .Add(new SpriteManager())
                .Add(new TextureSource())
                .Add(new SceneStack())
                .Add(new SceneManager()
                    .AddScene(new EmptyScene())
                    .AddScene((IScene)new IsometricBakeScene()
                        .Add(new PaletteManager())
                        .Add(builder)))
                .Add(new InputManager().RegisterMouseMode(MouseMode.Normal, new NormalMouseMode()))
                .Add(new InputBinder(disk => InputConfig.Load(config.BasePath, disk)))
                ;

            Raise(new InputModeEvent(InputMode.IsoBake));
            Raise(new SetSceneEvent(SceneId.IsometricBake));
            Raise(new SetClearColourEvent(0, 0, 0, 0));
            Raise(new EngineFlagEvent(FlagOperation.Set, EngineFlags.ShowBoundingBoxes));

            engine.Run();
        }
    }
}