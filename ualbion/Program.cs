using System;
using System.IO;
using System.Numerics;
using System.Reflection;
using UAlbion.Core;
using UAlbion.Core.Objects;
using UAlbion.Formats;
using UAlbion.Game;

namespace UAlbion
{
    class ConsoleLogger : IComponent
    {
        public void Attach(EventExchange exchange)
        {
            exchange.Subscribe<IEvent>(this);
        }

        public void Receive(IEvent @event)
        {
            Console.WriteLine(@event.ToString());
        }
    }

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
            var menuBackground = assets.LoadTexture(AssetType.Picture, 19);

            using (var engine = new Engine())
            {
                var scene = engine.Create3DScene();

                Skybox skybox = Skybox.LoadDefaultSkybox();
                scene.AddRenderable(skybox);

                var camera = (PerspectiveCamera)scene.Camera;
                camera.Position = new Vector3(-80, 25, -4.3f);
                camera.Yaw = -MathF.PI / 2;
                camera.Pitch = -MathF.PI / 9;

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
