using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Textures;
using UAlbion.Core.Visual;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Config;
using UAlbion.Game;
using UAlbion.Game.Entities;
using UAlbion.Game.Events;
using UAlbion.Game.Gui;
using UAlbion.Game.Input;
using UAlbion.Game.State;
using Veldrid;

namespace UAlbion
{
    static class Program
    {
        static unsafe void Main()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // Required for code page 850 support in .NET Core

            /*
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
            {
                RunGame(assets, baseDir);
            }
        }

        static void RunGame(IAssetManager assets, string baseDir)
        {
            var backend =
                //VeldridStartup.GetPlatformDefaultBackend()
                //GraphicsBackend.Metal /*
                //GraphicsBackend.Vulkan /*
                //GraphicsBackend.OpenGL /*
                //GraphicsBackend.OpenGLES /*
                GraphicsBackend.Direct3D11 /*
                //*/
                ;

#if DEBUG
            using (var engine = new Engine(backend, true))
#else
            using (var engine = new Engine(backend, false))
#endif
            {
                InputConfig inputConfig = InputConfig.Load(baseDir);

                engine.GlobalExchange
                    .Register<IAssetManager>(assets)
                    .Register<IStateManager>(new StateManager())
                    .Register<ISpriteResolver>(new SpriteResolver())
                    .Register<ITextureManager>(new TextureManager())
                    .Register<ISettings>(new Settings());

                var mapExchange = new EventExchange("Maps", engine.GlobalExchange);
                var sceneExchange = new EventExchange("Scenes", engine.GlobalExchange);

                engine.AddRenderer(new SpriteRenderer());
                engine.AddRenderer(new ExtrudedTileMapRenderer());
                var menuScene = Scenes.CreateMenuScene(sceneExchange);
                engine.AddScene(menuScene);
                engine.AddScene(Scenes.Create2DScene(sceneExchange));
                engine.AddScene(Scenes.Create3DScene(sceneExchange));

                engine.GlobalExchange
                    .Attach(new ConsoleLogger())
                    .Attach(new GameClock())
                    .Attach(new MapManager(mapExchange))
                    .Attach(new DebugMapInspector())
                    .Attach(new World2DInputMode())
                    .Attach(new DebugPickInputMode())
                    .Attach(new ContextMenuInputMode())
                    .Attach(new MouseLookInputMode())
                    .Attach(new InputBinder(inputConfig))
                    .Attach(new InputModeStack())
                    .Attach(new SceneStack())
                    .Attach(new CursorManager())
                    .Attach(new PaletteManager())
                    //.Attach(new Text(assets.LoadFont(MetaFontId.FontColor.White, false), "Hello world", Vector2.Zero))
                    //.Attach(new Text(assets.LoadFont(MetaFontId.FontColor.Red, true), "Test test test", new Vector2(0, -0.2f)))
                    //.Attach(new Text(assets.LoadFont(MetaFontId.FontColor.Yellow, false), "Warning!", new Vector2(0, -0.4f)))
                    ;

                //*
                engine.GlobalExchange.Raise(new LoadMapEvent(MapDataId.AltesFormergebäude), null); /*
                engine.GlobalExchange.Raise(new LoadMapEvent(MapDataId.Jirinaar3D), null); /*
                engine.GlobalExchange.Raise(new LoadMapEvent(MapDataId.HausDesJägerclans), null); //*/

                /*
                var menu = new MainMenu();
                var background = new ScreenSpaceSprite<PictureId>(PictureId.MenuBackground8, new Vector2(0.0f, 1.0f), new Vector2(2.0f, -1.6f));
                var status = new ScreenSpaceSprite<PictureId>(PictureId.StatusBar, new Vector2(0.0f, -0.6f), new Vector2(2.0f, -0.4f));
                menuScene.SceneExchange
                    .Attach(menu)
                    .Attach(background)
                    .Attach(status);

                engine.GlobalExchange.Raise(new SetSceneEvent((int)SceneId.MainMenu), null);
                engine.GlobalExchange.Raise(new SetCursorEvent((int)CoreSpriteId.Cursor), null);
                //*/
                engine.Run();
            }
        }

        static void DumpCoreSprites(Assets assets, string baseDir)
        {
            // Dump all core sprites
            var palette = assets.LoadPalette(PaletteId.Main3D);
            for (int i = 0; i < 86; i++)
            {
                var name = $"{i}_{(CoreSpriteId)i}";
                var coreSprite = assets.LoadTexture((CoreSpriteId)i);
                var multiTexture = new MultiTexture(name, palette.GetCompletePalette());
                multiTexture.AddTexture(1, coreSprite, 0, 0, null, false);
                multiTexture.SavePng(1, 0, $@"{baseDir}\data\exported\MAIN.EXE\{name}.bmp");
            }
        }

        static void DumpMapAndLabData(Assets assets, string baseDir)
        {
            using (var sw = File.CreateText($@"{baseDir}\re\3DInfo.txt"))
            {
                // Dump map and lab data 
                for (int i = 100; i < 400; i++)
                {
                    var map = assets.LoadMap3D((MapDataId) i);
                    if (map == null)
                        continue;

                    sw.WriteLine(
                        $"{i} {(MapDataId) i} {map.Width}x{map.Height} L{(int?) map.LabDataId} P{(int) map.PaletteId}:{map.PaletteId}");
                    var floors = map.Floors.GroupBy(x => x).Select(x => (x.Key, x.Count())).OrderBy(x => x.Item1);
                    sw.WriteLine("    Floors: " + string.Join(" ", floors.Select(x => $"{x.Item1}:{x.Item2}")));
                    var ceilings = map.Ceilings.GroupBy(x => x).Select(x => (x.Key, x.Count())).OrderBy(x => x.Item1);
                    sw.WriteLine("    Ceilings: " + string.Join(" ", ceilings.Select(x => $"{x.Item1}:{x.Item2}")));
                    var contents = map.Contents.GroupBy(x => x).Select(x => (x.Key, x.Count())).OrderBy(x => x.Item1);
                    sw.WriteLine("    Contents: " + string.Join(" ", contents.Select(x => $"{x.Item1}:{x.Item2}")));
                }

                for (int i = 0; i < 300; i++)
                {
                    var l = assets.LoadLabyrinthData((LabyrinthDataId) i);
                    if (l == null)
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
                        sw.WriteLine($"    W {j}: {w.TextureNumber} {w.AnimationFrames} P{w.TransparentColour}");
                    }

                    for (int j = 0; j < l.ObjectGroups.Count; j++)
                    {
                        var o = l.ObjectGroups[j];
                        sw.WriteLine(
                            $"    Obj {j}: {o.AutoGraphicsId} [{string.Join(", ", o.SubObjects.Select(x => x.ObjectInfoNumber.ToString()))}]");
                    }

                    for (int j = 0; j < l.Objects.Count; j++)
                    {
                        var o = l.Objects[j];
                        sw.WriteLine(
                            $"    Extra {j}: {o.TextureNumber} {o.AnimationFrames} {o.Width}x{o.Height} M:{o.MapWidth}x{o.MapHeight}");
                    }
                }
            }
        }
    }
}
