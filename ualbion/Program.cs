using System.IO;
using System.Numerics;
using System.Reflection;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using UAlbion.Core;
using UAlbion.Core.Objects;
using UAlbion.Formats;
using UAlbion.Game;
using UAlbion.Game.AssetIds;
using UAlbion.Game.Gui;

namespace UAlbion
{
    class Program
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
            var textureManager = new AlbionTextureManager(assets);
            using (var engine = new Engine())
            {
                var spriteRenderer = new SpriteRenderer();
                var scene = engine.Create2DScene();
                scene.AddComponent(new ConsoleLogger());
                scene.Camera.Position = new Vector3(656, 678, 0);
                scene.Camera.Magnification = 4.0f;

                var menu = new MainMenu();
                scene.AddComponent(menu);

                //Image<Rgba32> menuBackground = assets.LoadPicture( PictureId.MenuBackground8);
                //var background = new Sprite(spriteRenderer, menuBackground, new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.8f));
                //scene.AddRenderable(background);

                //var statusBackground = assets.LoadPicture(PictureId.StatusBar);
                //var status = new SpriteRenderer(statusBackground, new Vector2(0.0f, 0.8f), new Vector2(1.0f, 0.2f));
                //scene.AddRenderable(status);

                var mapImage = assets.LoadPicture(PictureId.TestMap);
                var map = new Sprite(spriteRenderer, mapImage) { Position = new Vector2(0.0f, 0.0f));
                scene.AddRenderable(map);

                engine.SetScene(scene);
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
}
