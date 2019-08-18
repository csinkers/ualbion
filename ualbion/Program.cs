using System.IO;
using System.Reflection;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Config;
using UAlbion.Game;
using UAlbion.Game.Events;
using UAlbion.Game.Input;
using Veldrid;

namespace UAlbion
{
    static class Program
    {
        static unsafe void Main()
        {
            Veldrid.Sdl2.SDL_version version;
            Veldrid.Sdl2.Sdl2Native.SDL_GetVersion(&version);

            var baseDir = Directory.GetParent(
                Path.GetDirectoryName(
                Assembly.GetExecutingAssembly()
                .Location)) // ./ualbion/bin/Debug
                ?.Parent    // ./ualbion/bin
                ?.Parent    // ./ualbion
                ?.Parent    // .
                ?.FullName;

            if (string.IsNullOrEmpty(baseDir))
                return;

            AssetConfig assetConfig = AssetConfig.Load(baseDir);
            CoreSpriteConfig coreSpriteConfig = CoreSpriteConfig.Load(baseDir);

            var backend =
                //VeldridStartup.GetPlatformDefaultBackend()
                //GraphicsBackend.Metal /*
                //GraphicsBackend.Vulkan /*
                //GraphicsBackend.OpenGL /*
                //GraphicsBackend.OpenGLES /*
                GraphicsBackend.Direct3D11 /*
                //*/
                ;

            using (var assets = new Assets(assetConfig, coreSpriteConfig))
            using (var engine = new Engine(backend))
            {
                var spriteResolver = new SpriteResolver(assets);
                assets.Attach(engine.GlobalExchange);
                new ConsoleLogger().Attach(engine.GlobalExchange);
                new GameClock().Attach(engine.GlobalExchange);
                new SceneLoader(assets, engine, spriteResolver).Attach(engine.GlobalExchange);
                new DebugMapInspector().Attach(engine.GlobalExchange);

                new NormalMouseMode().Attach(engine.GlobalExchange);
                new DebugPickMouseMode().Attach(engine.GlobalExchange);
                new ContextMenuMouseMode().Attach(engine.GlobalExchange);
                new InventoryMoveMouseMode().Attach(engine.GlobalExchange);
                new InputBinder().Attach(engine.GlobalExchange);
                new CursorManager(assets).Attach(engine.GlobalExchange);
                engine.GlobalExchange.Raise(new SetMouseModeEvent((int)MouseModeId.Normal), null);
                engine.GlobalExchange.Raise(new LoadMapEvent((int)MapDataId.HausDesJägerclans), null);
                //engine.GlobalExchange.Raise(new LoadMapEvent((int)MapDataId.TorontoTeil1), null);

                /*
                var menu = new MainMenu();
                scene.AddComponent(menu);

                var background = new Billboard2D<PictureId>(PictureId.MenuBackground8, 0)
                {
                    Position = new Vector2(-1.0f, 1.0f),
                    Size = new Vector2(2.0f, -2.0f)
                };
                engine.AddComponent(background);

                var statusBackground = assets.LoadPicture(PictureId.StatusBar);
                var status = new SpriteRenderer(statusBackground, new Vector2(0.0f, 0.8f), new Vector2(1.0f, 0.2f));
                scene.AddRenderable(status);
                //*/
                //engine.GlobalExchange.Raise(new LoadRenderDocEvent(), null);
                engine.Run();
            }
        }
    }
}
