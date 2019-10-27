using System;
using System.Collections.Generic;
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
using UAlbion.Formats.Assets;
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

            using var assets = new AssetManager(assetConfig, coreSpriteConfig);
            // DumpCoreSprites(assets, baseDir); return;
            // DumpCharacterSheets(assets);
            // DumpChests(assets);
            RunGame(assets, baseDir);
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

            using var engine = new Engine(backend,
#if DEBUG
                true);
#else
                 false);
#endif

            InputConfig inputConfig = InputConfig.Load(baseDir);
            var mapExchange = new EventExchange("Maps", engine.GlobalExchange);

            engine.AddRenderer(new SpriteRenderer());
            engine.AddRenderer(new ExtrudedTileMapRenderer());

            var allScenesExchange = new EventExchange("Scenes", engine.GlobalExchange);
            var map = new MapScene(allScenesExchange);
            var flat = new FlatScene(allScenesExchange);
            var dungeon = new DungeonScene(allScenesExchange);
            var menuScene = new MenuScene(allScenesExchange);
            var inventoryScene = new InventoryScene(allScenesExchange);
            var statusBar = new StatusBar();

            engine.AddScene(flat)
                .AddScene(dungeon)
                .AddScene(map)
                .AddScene(menuScene)
                .AddScene(inventoryScene);

            var inputManager = new InputManager();
            engine.GlobalExchange
                .Register<IAssetManager>(assets)
                .Register<IInputManager>(inputManager)
                .Register<ILayoutManager>(new LayoutManager())
                .Register<IStateManager>(new StateManager())
                .Register<ITextureManager>(new TextureManager())
                .Register<ISpriteResolver>(new SpriteResolver())
                .Register<ISettings>(new Settings())
                .Register<IFlatScene>(flat)
                .Register<IDungeonScene>(dungeon)
                .Register<IMapScene>(map)
                .Register<IMenuScene>(menuScene)
                .Register<IInventoryScene>(inventoryScene)
                .Attach(new ConsoleLogger())
                .Attach(new GameClock())
                .Attach(new MapManager(mapExchange))
                .Attach(new DebugMapInspector())
                .Attach(new InputBinder(inputConfig))
                .Attach(new InputModeStack())
                .Attach(new MouseModeStack())
                .Attach(new SceneStack())
                .Attach(new CursorManager())
                .Attach(new PaletteManager())
                .Attach(statusBar)
                ;

            inputManager
                .RegisterInputMode(InputMode.ContextMenu, new ContextMenuInputMode())
                .RegisterInputMode(InputMode.World2D, new World2DInputMode())
                .RegisterMouseMode(MouseMode.Normal, new NormalMouseMode())
                .RegisterMouseMode(MouseMode.Exclusive, new ExclusiveMouseMode())
                .RegisterMouseMode(MouseMode.DebugPick, new DebugPickMouseMode())
                .RegisterMouseMode(MouseMode.MouseLook, new MouseLookMouseMode())
                ;

            engine.GlobalExchange.Raise(new NewGameEvent(), null);
            /*
            engine.GlobalExchange.Raise(new LoadMapEvent(MapDataId.AltesFormergebäude), null); /*
            engine.GlobalExchange.Raise(new LoadMapEvent(MapDataId.Jirinaar3D), null); /*
            engine.GlobalExchange.Raise(new LoadMapEvent(MapDataId.HausDesJägerclans), null); //*/

            /*
            var menu = new MainMenu();
            var menuBackground = new ScreenSpaceSprite<PictureId>(PictureId.MenuBackground8, new Vector2(0.0f, 1.0f), new Vector2(2.0f, -2.0f));
            menuScene.SceneExchange
                .Attach(menu)
                .Attach(menuBackground)
                //.Attach(new Starfield())
                ;

            engine.GlobalExchange.Raise(new SetSceneEvent((int)SceneId.MainMenu), null);
            engine.GlobalExchange.Raise(new SetCursorEvent(CoreSpriteId.Cursor), null);
            engine.GlobalExchange.Raise(new SetMouseModeEvent(MouseMode.Normal), null);
            engine.GlobalExchange.Raise(new SetInputModeEvent(InputMode.Dialog), null);
            //*/

            /*
            var inventory = new InventoryScreen();
            inventoryScene.SceneExchange
                .Attach(inventory)
                ;

            engine.GlobalExchange.Raise(new SetSceneEvent((int)SceneId.Inventory), null);
            engine.GlobalExchange.Raise(new SetCursorEvent(CoreSpriteId.Cursor), null);
            engine.GlobalExchange.Raise(new SetMouseModeEvent(MouseMode.Normal), null);
            engine.GlobalExchange.Raise(new SetInputModeEvent(InputMode.Dialog), null);
            //*/

            engine.Run();
        }

        static void DumpCoreSprites(IAssetManager assets, string baseDir)
        {
            var dir = $@"{baseDir}\data\exported\MAIN.EXE";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            // Dump all core sprites
            var palette = assets.LoadPalette(PaletteId.Main3D);
            for (int i = 0; i < 86; i++)
            {
                var name = $"{i}_{(CoreSpriteId)i}";
                var coreSprite = assets.LoadTexture((CoreSpriteId)i);
                var multiTexture = new MultiTexture(name, palette.GetCompletePalette());
                multiTexture.AddTexture(1, coreSprite, 0, 0, null, false);
                multiTexture.SavePng(1, 0, $@"{dir}\{name}.bmp");
            }
        }

        static void DumpMapAndLabData(IAssetManager assets, string baseDir)
        {
            using var sw = File.CreateText($@"{baseDir}\re\3DInfo.txt");
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

        static void DumpCharacterSheets(IAssetManager assets)
        {
            var chars = new List<CharacterSheet>();
            foreach (PartyCharacterId charId in Enum.GetValues(typeof(PartyCharacterId)))
                chars.Add(assets.LoadCharacter(AssetType.PartyMember, charId));
            foreach (NpcCharacterId charId in Enum.GetValues(typeof(NpcCharacterId)))
                chars.Add(assets.LoadCharacter(AssetType.Npc, charId));
            foreach (MonsterCharacterId charId in Enum.GetValues(typeof(MonsterCharacterId)))
                chars.Add(assets.LoadCharacter(AssetType.Monster, charId));

            chars = chars.Where(x => x != null && (x.GermanName != "" || x.PortraitId != 0)).ToList();
            foreach (var c in chars)
            {
                
            }
        }
        static void DumpChests(IAssetManager assets)
        {
            var chests = Enum.GetValues(typeof(ChestId)).Cast<ChestId>().ToDictionary(x => x, assets.LoadChest);
            var merchants = Enum.GetValues(typeof(MerchantId)).Cast<MerchantId>().ToDictionary(x => x, assets.LoadMerchant);
            foreach (var chest in chests)
            {
            }
        }
    }
}
