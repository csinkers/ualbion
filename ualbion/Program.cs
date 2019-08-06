using System.IO;
using System.Numerics;
using System.Reflection;
using UAlbion.Core;
using UAlbion.Core.Objects;
using UAlbion.Formats;
using UAlbion.Game;
using UAlbion.Game.AssetIds;

namespace UAlbion
{
    static class Program
    {
        static unsafe void Main()
        {
            Veldrid.Sdl2.SDL_version version;
            Veldrid.Sdl2.Sdl2Native.SDL_GetVersion(&version);

            var baseDir = Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))?.Parent?.Parent?.Parent?.FullName;
            if (string.IsNullOrEmpty(baseDir))
                return;

            AssetConfig config = AssetConfig.Load(baseDir);

            var assets = new Assets(config);
            var spriteResolver = new SpriteResolver(assets);

            using (var engine = new Engine())
            {
                var scene = engine.Create2DScene();
                scene.AddComponent(assets);
                scene.AddComponent(new PaletteManager(scene, assets));
                scene.AddRenderer(new SpriteRenderer(engine.TextureManager, spriteResolver));
                scene.AddComponent(new ConsoleLogger());
                scene.AddComponent(new GameClock());
                scene.Camera.Position = new Vector3(0, 0, 0);
                scene.Camera.Magnification = 2.0f;
                scene.Exchange.Raise(new LoadPalEvent((int)PaletteId.Main3D), null);

                // var menu = new MainMenu();
                // scene.AddComponent(menu);

                // Image<Rgba32> menuBackground = assets.LoadPicture( PictureId.MenuBackground8);
                // var background = new Sprite(spriteRenderer, menuBackground, new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.8f));
                // scene.AddRenderable(background);

                // var statusBackground = assets.LoadPicture(PictureId.StatusBar);
                // var status = new SpriteRenderer(statusBackground, new Vector2(0.0f, 0.8f), new Vector2(1.0f, 0.2f));
                // scene.AddRenderable(status);

                var map = new Billboard2D<PictureId>(PictureId.TestMap, 0) { Position = new Vector2(0.0f, 0.0f) };
                scene.AddComponent(map);
                //*
                var testBillboard = new Billboard2D<DungeonFloorId>(DungeonFloorId.Water, 0) { Position = new Vector2(-64.0f, 0.0f) };
                scene.AddComponent(testBillboard);
                scene.AddComponent(new Billboard2D<DungeonFloorId>(DungeonFloorId.Water, 0) { Position = new Vector2(-128.0f, 0.0f) });
                scene.AddComponent(new Billboard2D<DungeonFloorId>(DungeonFloorId.Water, 0) { Position = new Vector2(-128.0f, 64.0f) });
                scene.AddComponent(new Billboard2D<DungeonFloorId>(DungeonFloorId.Water, 0) { Position = new Vector2(-64.0f, 64.0f) });
                scene.AddComponent(new Billboard2D<DungeonFloorId>(DungeonFloorId.Water, 0) { Position = new Vector2(-64.0f, 128.0f) });
                //*/
                engine.SetScene(scene);
                //scene.Exchange.Raise(new LoadRenderDocEvent(), null);
                engine.Run();
            }
        }
    }
}
