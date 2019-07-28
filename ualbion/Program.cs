using System.IO;
using System.Numerics;
using System.Reflection;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using UAlbion.Core;
using UAlbion.Core.Objects;
using UAlbion.Formats;
using UAlbion.Game;
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
            var palette = assets.LoadPalette(2);
            Image<Rgba32> menuBackground = assets.LoadPicture(25);
            var statusBackground = assets.LoadPicture(99);

            var gameState = new GameState();
            using (var engine = new Engine())
            {
                var scene = engine.Create2DScene();
                scene.AddComponent(new ConsoleLogger());

                var menu = new MainMenu();
                scene.AddComponent(menu);

                var background = new Sprite(menuBackground, new Vector2(-1.0f, -0.6f), new Vector2(2.0f, 1.6f));
                scene.AddRenderable(background);

                var status = new Sprite(statusBackground, new Vector2(-1.0f, -1.0f), new Vector2(2.0f, 0.4f));
                scene.AddRenderable(status);

/*
                var camera = (PerspectiveCamera)scene.Camera;
                camera.Position = new Vector3(-80, 25, -4.3f);
                camera.Yaw = -MathF.PI / 2;
                camera.Pitch = -MathF.PI / 9;
*/
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
