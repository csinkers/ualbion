using System.IO;
using System.Reflection;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Game;
using UAlbion.Game.AssetIds;
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

            var baseDir = Directory.GetParent(Path.GetDirectoryName(
                Assembly.GetExecutingAssembly()
                    .Location)) // ./ualbion/bin/Debug
                ?.Parent        // ./ualbion/bin
                ?.Parent        // ./ualbion
                ?.Parent        // .
                ?.FullName;

            if (string.IsNullOrEmpty(baseDir))
                return;

            AssetConfig config = AssetConfig.Load(baseDir);

            /* Dump out info on all 2D maps
            for (int i = 100; i < 400; i++)
            {
                var map = assets.LoadMap2D((MapDataId) i);
                if (map == null)
                    continue;

                int minUnderlay = map.Underlay.Where(x => x > 1).Select(x => (int?)x).Min() ?? 0;
                int maxUnderlay = map.Underlay.Max();
                int minOverlay = map.Overlay.Where(x => x > 1).Select(x => (int?)x).Min() ?? 0;
                int maxOverlay = map.Overlay.Max();

                Console.WriteLine($"TS{map.TilesetId} P{map.PaletteId} Underlays: {minUnderlay}-{maxUnderlay} Overlays: {minOverlay}-{maxOverlay} Map {i} W{map.Width} H{map.Height}");
            }

            Console.ReadLine();
            return;
            //*/

            var backend =
                //VeldridStartup.GetPlatformDefaultBackend()
                //GraphicsBackend.Metal /*
                //GraphicsBackend.Vulkan /*
                //GraphicsBackend.OpenGL /*
                //GraphicsBackend.OpenGLES /*
                GraphicsBackend.Direct3D11 /*
                //*/
                ;

            using (var assets = new Assets(config))
            using (var engine = new Engine(backend))
            {
                var spriteResolver = new SpriteResolver(assets);
                assets.Attach(engine.GlobalExchange);
                new ConsoleLogger().Attach(engine.GlobalExchange);
                new GameClock().Attach(engine.GlobalExchange);
                new SceneLoader(assets, engine, spriteResolver).Attach(engine.GlobalExchange);
                new DebugMapInspector().Attach(engine.GlobalExchange);
                engine.GlobalExchange.Raise(new LoadMapEvent((int)MapDataId.HausDesJägerclans), null);

                new NormalMouseMode().Attach(engine.GlobalExchange);
                new DebugPickMouseMode().Attach(engine.GlobalExchange);
                new ContextMenuMouseMode().Attach(engine.GlobalExchange);
                new InventoryMoveMouseMode().Attach(engine.GlobalExchange);
                new InputBinder().Attach(engine.GlobalExchange);
                engine.GlobalExchange.Raise(new SetMouseModeEvent((int)MouseModeId.Normal), null);

                //engine.GlobalExchange.Raise(new LoadMapEvent((int)0), null);

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
                /*
                var map = new Billboard2D<PictureId>(PictureId.TestMap, 0) { Position = new Vector2(0.0f, 0.0f) };
                scene.AddComponent(map);
                /*
                scene.AddComponent(new Billboard2D<DungeonFloorId>(DungeonFloorId.Water, 0) { Position =
 new Vector2(-64.0f, 0.0f) });
                scene.AddComponent(new Billboard2D<DungeonFloorId>(DungeonFloorId.Water, 0) { Position =
 new Vector2(-128.0f, 0.0f) });
                scene.AddComponent(new Billboard2D<DungeonFloorId>(DungeonFloorId.Water, 0) { Position =
 new Vector2(-128.0f, 64.0f) });
                scene.AddComponent(new Billboard2D<DungeonFloorId>(DungeonFloorId.Water, 0) { Position =
 new Vector2(-64.0f, 64.0f) });
                //*/
                //scene.Exchange.Raise(new LoadRenderDocEvent(), null);
                engine.Run();
            }
        }
    }
}
