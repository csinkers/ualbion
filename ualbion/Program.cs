using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Reflection;
using UAlbion.Core;
using UAlbion.Core.Objects;
using UAlbion.Formats;
using UAlbion.Game;
using UAlbion.Game.AssetIds;
using UAlbion.Game.Gui;

namespace UAlbion
{
    class Billboard : Component
    {
        static IList<Handler> Handlers => new Handler[] { new Handler<Billboard,RenderEvent>((x, e) => x.OnRender(e)), };
        public Vector2 Position { get; set; }

        readonly ITexture _texture;
        readonly SpriteFlags _flags;

        public Billboard(ITexture texture, SpriteFlags flags) : base(Handlers)
        {
            _texture = texture;
            _flags = flags;
        }

        void OnRender(RenderEvent renderEvent)
        {
            var sprite = ((SpriteRenderer)renderEvent.GetRenderer(typeof(SpriteRenderer))).CreateSprite();
            sprite.Initialize(Position, _texture, 0, _flags);
            renderEvent.Add(sprite);
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
            var palette = assets.LoadPalette(PaletteId.Main3D);
            var gameState = new GameState();

            using (var engine = new Engine())
            {
                var scene = engine.Create2DScene();
                scene.SetPalette(palette.Entries); // TODO: Update on game tick
                scene.AddRenderer(new SpriteRenderer(engine.TextureManager));
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

                ITexture mapImage = assets.LoadPicture(PictureId.TestMap);
                var map = new Billboard(mapImage, 0) { Position = new Vector2(0.0f, 0.0f) };
                scene.AddComponent(map);

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
