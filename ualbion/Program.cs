using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Game.Gui;

namespace UAlbion
{
    class ConsoleLogger : IComponent
    {
        EventExchange _exchange;

        public void Attach(EventExchange exchange)
        {
            _exchange = exchange;
            exchange.Subscribe<IEvent>(this);
            Task.Run(ConsoleReaderThread);
        }

        public void Receive(IEvent @event, object sender)
        {
            switch(@event)
            {
                case EngineUpdateEvent e:
                    break;

                default:
                    Console.WriteLine(@event.ToString());
                    break;
            }
        }

        public void ConsoleReaderThread()
        {
            do
            {
                var command = Console.ReadLine();
                try
                {
                    var @event = Event.Parse(command);
                    _exchange.Raise(@event, this);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Parse error: {0}", e);
                }

            } while (true);
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
                var scene = engine.Create2DScene();
                scene.AddComponent(new ConsoleLogger());

                var menu = new MainMenu();
                scene.AddComponent(menu);

                //MeshData planeMesh = PrimitiveShapes.Plane(1, 1, 1);
                //Skybox skybox = Skybox.LoadDefaultSkybox();
                //scene.AddRenderable(skybox);
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
