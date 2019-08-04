using System;
using System.Collections.Generic;
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
        static unsafe void Main(string[] args)
        {
            Veldrid.Sdl2.SDL_version version;
            Veldrid.Sdl2.Sdl2Native.SDL_GetVersion(&version);

            var baseDir = Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).Parent.Parent.Parent.FullName;
            Config config = Config.Load(baseDir);

            var assets = new Assets(config);
            var palette = assets.LoadPalette(PaletteId.Main3D);
            var gameState = new GameState();

            using (var engine = new Engine())
            {
                Engine.CheckForErrors();
                var scene = engine.Create2DScene();
                Engine.CheckForErrors();
                scene.SetPalette(palette.Name, palette.GetPaletteAtTime(0)); // TODO: Update on game tick
                Engine.CheckForErrors();
                scene.AddComponent(new PaletteManager(scene, palette));
                Engine.CheckForErrors();
                scene.AddRenderer(new SpriteRenderer(engine.TextureManager));
                Engine.CheckForErrors();
                scene.AddComponent(new ConsoleLogger());
                Engine.CheckForErrors();
                scene.AddComponent(new GameClock());
                scene.Camera.Position = new Vector3(0, 0, 0);
                scene.Camera.Magnification = 2.0f;
                Engine.CheckForErrors();

                // var menu = new MainMenu();
                // scene.AddComponent(menu);

                // Image<Rgba32> menuBackground = assets.LoadPicture( PictureId.MenuBackground8);
                // var background = new Sprite(spriteRenderer, menuBackground, new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.8f));
                // scene.AddRenderable(background);

                // var statusBackground = assets.LoadPicture(PictureId.StatusBar);
                // var status = new SpriteRenderer(statusBackground, new Vector2(0.0f, 0.8f), new Vector2(1.0f, 0.2f));
                // scene.AddRenderable(status);


                ITexture mapImage = assets.LoadPicture(PictureId.TestMap);
                Engine.CheckForErrors();
                var map = new Billboard2D(mapImage, 0) { Position = new Vector2(0.0f, 0.0f) };
                Engine.CheckForErrors();
                scene.AddComponent(map);
                Engine.CheckForErrors();
                //*
                AlbionSprite testSprite = assets.LoadTexture(DungeonFloorId.Water);
                Engine.CheckForErrors();
                var frame = testSprite.Frames[0];
                Engine.CheckForErrors();
                var testTexture = new EightBitTexture(testSprite.Name, (uint)frame.Width, (uint)frame.Height, 1, 1, frame.Pixels);
                Engine.CheckForErrors();
                var testBillboard = new Billboard2D(testTexture, 0) { Position = new Vector2(-64.0f, 0.0f), RenderOrder = 1 };
                Engine.CheckForErrors();
                scene.AddComponent(testBillboard);
                Engine.CheckForErrors();
                //*/
                engine.SetScene(scene);
                Engine.CheckForErrors();
                engine.Run();
            }

            /*
            Load palettes
            Load GUI sprites
            Show game frame
                Set mode to main menu
            */

        }
    }

    class PaletteManager : Component
    {
        static readonly IList<Handler> Handlers = new Handler[]
        {
            new Handler<PaletteManager, UpdateEvent>((x, e) =>
            {
                x._ticks++;
                x._scene.SetPalette(x._palette.Name, x._palette.GetPaletteAtTime(x._ticks));
            }),
        };

        readonly Scene _scene;
        AlbionPalette _palette;
        int _ticks;

        public PaletteManager(Scene scene, AlbionPalette palette) : base(Handlers)
        {
            _scene = scene;
            _palette = palette ?? throw new ArgumentNullException(nameof(palette));
        }

        public void SetPalette(AlbionPalette palette)
        {
            _palette = palette ?? throw new ArgumentNullException(nameof(palette));
        }
    }
}
