using System;
using System.IO;
using System.Reflection;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Config;
using UAlbion.Game;
using UAlbion.Game.Entities;
using UAlbion.Game.Events;
using UAlbion.Game.Input;
using UAlbion.Game.State;
using Veldrid;

namespace UAlbion
{
    static class Program
    {
        public static Scene Create2DScene(Assets assets, EventExchange allScenesExchange)
        {
            // TODO: Build scenes from config
            var id = SceneId.World2D;
            var camera = new OrthographicCamera();
            var renderers = new[]
            {
                typeof(DebugGuiRenderer),
                typeof(FullScreenQuad),
                typeof(ScreenDuplicator),
                typeof(SpriteRenderer),
            };

            var sceneExchange = new EventExchange(id.ToString(), allScenesExchange);
            var scene = new Scene((int)id, camera, renderers, sceneExchange);
            allScenesExchange.Attach(scene);
            var cameraMotion = new CameraMotion2D(camera);
            sceneExchange
                .Attach(camera)
                .Attach(cameraMotion);
            return scene;
        }

        public static Scene Create3DScene(Assets assets, EventExchange allScenesExchange)
        {
            var id = SceneId.World3D;
            var renderers = new[]
            {
                typeof(DebugGuiRenderer),
                typeof(FullScreenQuad),
                typeof(ScreenDuplicator),
                typeof(ExtrudedTileMapRenderer),
                typeof(SpriteRenderer),
            };

            var camera = new PerspectiveCamera();
            var sceneExchange = new EventExchange(id.ToString(), allScenesExchange);
            var scene = new Scene((int)SceneId.World3D, camera, renderers, sceneExchange);
            allScenesExchange.Attach(scene);
            var cameraMotion = new CameraMotion3D(camera);
            sceneExchange
                .Attach(camera)
                .Attach(cameraMotion);
            return scene;
        }

        static unsafe void Main()
        {
            //*
            Console.WriteLine("Entry point reached. Press enter to continue");
            Console.ReadLine(); //*/

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
            InputConfig inputConfig = InputConfig.Load(baseDir);

            var backend =
                //VeldridStartup.GetPlatformDefaultBackend()
                //GraphicsBackend.Metal /*
                GraphicsBackend.Vulkan /*
                //GraphicsBackend.OpenGL /*
                //GraphicsBackend.OpenGLES /*
                GraphicsBackend.Direct3D11 /*
                //*/
                ;

            /*
            Scenes:
                Menu Screen
                Inventory Screen
                2D World
                3D World
                Automap for 3D world
                Combat
             */

            using (var assets = new Assets(assetConfig, coreSpriteConfig))
            using (var engine = new Engine(backend))
            //using(var sw = File.CreateText(@"C:\Depot\Main\bitbucket\ualbion\re\3DInfo.txt"))
            {
                /*
                for(int i = 100; i < 400; i++)
                {
                    var map = assets.LoadMap3D((MapDataId)i);
                    if (map == null)
                        continue;

                    sw.WriteLine($"{i} {(MapDataId)i} {map.Width}x{map.Height} L{(int?)map.LabDataId} P{(int)map.PaletteId}:{map.PaletteId}");
                    var floors = map.Floors.GroupBy(x => x).Select(x => (x.Key, x.Count())).OrderBy(x => x.Item1);
                    sw.WriteLine("    Floors: " + string.Join(" ", floors.Select(x => $"{x.Item1}:{x.Item2}")));
                    var ceilings = map.Ceilings.GroupBy(x => x).Select(x => (x.Key, x.Count())).OrderBy(x => x.Item1);
                    sw.WriteLine("    Ceilings: " + string.Join(" ", ceilings.Select(x => $"{x.Item1}:{x.Item2}")));
                    var contents = map.Contents.GroupBy(x => x).Select(x => (x.Key, x.Count())).OrderBy(x => x.Item1);
                    sw.WriteLine("    Contents: " + string.Join(" ", contents.Select(x => $"{x.Item1}:{x.Item2}")));
                }

                for(int i = 0; i < 300; i++)
                {
                    var l = assets.LoadLabyrinthData((LabyrinthDataId) i);
                    if(l == null)
                        continue;

                    sw.WriteLine($"L{i}");
                    for (int j = 0; j < l.FloorAndCeilings.Count; j++)
                    {
                        var fc = l.FloorAndCeilings[j];
                        sw.WriteLine($"    F/C {j}: {fc.TextureNumber} {fc.AnimationCount}");
                    }

                    for (int j = 0; j < l.Walls.Count; j++)
                    {
                        var w = l.Walls[j];
                        sw.WriteLine($"    W {j}: {w.TextureNumber} {w.AnimationFrames} P{w.PaletteId}");
                    }

                    for (int j = 0; j < l.Objects.Count; j++)
                    {
                        var o = l.Objects[j];
                        sw.WriteLine($"    Obj {j}: {o.AutoGraphicsId} [{string.Join(", ",o.SubObjects.Select(x => x.ObjectInfoNumber.ToString()))}]");
                    }

                    for (int j = 0; j < l.ExtraObjects.Count; j++)
                    {
                        var o = l.ExtraObjects[j];
                        sw.WriteLine($"    Extra {j}: {o.TextureNumber} {o.AnimationFrames} {o.Width}x{o.Height} M:{o.MapWidth}x{o.MapHeight}");
                    }
                }

                return; */
                var stateManager = new StateManager();
                var sceneExchange = new EventExchange("Scenes", engine.GlobalExchange);
                var mapExchange = new EventExchange("Maps", engine.GlobalExchange);
                var spriteResolver = new SpriteResolver(assets);
                engine.AddRenderer(new SpriteRenderer(engine.TextureManager, spriteResolver));
                engine.AddRenderer(new ExtrudedTileMapRenderer(engine.TextureManager));
                engine.AddScene(Create2DScene(assets, sceneExchange));
                engine.AddScene(Create3DScene(assets, sceneExchange));

                engine.GlobalExchange
                    .Attach(assets)
                    .Attach(stateManager)
                    .Attach(new ConsoleLogger())
                    .Attach(new GameClock(stateManager))
                    .Attach(new MapManager(assets, mapExchange))
                    .Attach(new DebugMapInspector(stateManager))
                    .Attach(new World2DInputMode())
                    .Attach(new DebugPickInputMode())
                    .Attach(new ContextMenuInputMode())
                    .Attach(new MouseLookInputMode())
                    .Attach(new InputBinder(inputConfig))
                    .Attach(new InputModeStack())
                    .Attach(new SceneStack())
                    .Attach(new CursorManager(assets))
                    .Attach(new PaletteManager(assets));

                engine.GlobalExchange.Raise(new LoadMapEvent((int)MapDataId.AltesFormergebäude), null); /*
                engine.GlobalExchange.Raise(new LoadMapEvent((int)MapDataId.Jirinaar3D), null); /*
                engine.GlobalExchange.Raise(new LoadMapEvent((int)MapDataId.HausDesJägerclans), null); //*/

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
