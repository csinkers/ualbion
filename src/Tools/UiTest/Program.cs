using System.Numerics;
using System.Reflection;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Api.Settings;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Veldrid;
using UAlbion.Core.Veldrid.Sprites;
using UAlbion.Core.Veldrid.Textures;
using UAlbion.Core.Visual;
using UAlbion.Formats.Ids;
using UAlbion.Game.Veldrid.Diag;
using Veldrid;

namespace UiTest
{
    public static class Program
    {
        public static void Main()
        {
            var exchange = Setup();
            exchange.Resolve<IEngine>().Run();
        }

        static EventExchange Setup()
        {
            var exchange = new EventExchange();
            var disk = new FileSystem(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            var baseDir = ConfigUtil.FindBasePath(disk);
            if (baseDir == null)
                throw new DirectoryNotFoundException("Could not determine base directory");

            var pathResolver = new PathResolver(baseDir, "UiTest");
            var shaderCache = new ShaderCache(pathResolver.ResolvePath("$(CACHE)/ShaderCache"));
            var shaderLoader = new ShaderLoader();
            shaderLoader.AddShaderDirectory(pathResolver.ResolvePath("$(MODS)/Shaders/Shaders"));
            var engine = new Engine(GraphicsBackend.Direct3D11, true, true);
            var camera = new OrthographicCamera();
            var imgui = new ImGuiManager(DiagMenus.Draw);
            var renderManager = BuildRenderManager(new SimpleCameraProvider(camera), imgui);

            exchange
                .Register<IFileSystem>(disk)
                .Register<IPathResolver>(pathResolver)
                .Register<IVarSet>(new VarSet("Default"))
                .Register<IPaletteManager>(new DummyPaletteManager(BuildPalette(), null))
                .Attach(imgui)
                .Attach(new ResourceLayoutSource())
                .Attach(new TextureSource())
                .Attach(new SpriteSamplerSource())
                .Attach(new VeldridCoreFactory(_ => throw new NotSupportedException()))
                .Attach(shaderCache)
                .Attach(shaderLoader)
                .Attach(camera)
                .Attach(renderManager)
                .Attach(engine)
                ;

            engine.RenderSystem = renderManager.GetSystem("pl_main");

            var spriteId = new AssetId(AssetType.Object3D, 1);
            var texture = new SimpleTexture<uint>(spriteId, "Test", 2, 2, new[]
            {
                ApiUtil.PackColor(255, 0, 0, 255),
                ApiUtil.PackColor(0, 255, 0, 255),
                ApiUtil.PackColor(0, 0, 255, 255),
                ApiUtil.PackColor(255, 255, 255, 255),
            });

            var sprite = new Sprite(spriteId, Vector3.Zero, DrawLayer.Billboards, 0, 0, _ => texture);
            exchange.Attach(sprite);

            return exchange;
        }

        static RenderManager BuildRenderManager(ICameraProvider cameraProvider, IImGuiManager imgui)
        {
            return RenderManagerBuilder.Create()
                .Framebuffer("fb_screen", new MainFramebuffer("fb_screen"))
                .Framebuffer("fb_game", new SimpleFramebuffer("fb_game", 1, 1))
                .Renderer("r_ui", new DebugGuiRenderer(SimpleFramebuffer.Output))
                .Renderer("r_sprite", new SpriteRenderer(SimpleFramebuffer.Output))
                .Source("s_ui", new DebugGuiRenderable())
                .Source("s_sprite", new BatchManager<SpriteKey, SpriteInfo>(static (key, f) => f.CreateSpriteBatch(key)))
                .System("pl_main", pb => pb
                    .PreRender(_ => imgui.Draw())
                    .Resources(new GlobalResourceSetProvider())
                    .Pass("rp_game", rp => rp
                        .Target("fb_game")
                        .Resources(new MainPassResourceProvider(pb.GetFramebuffer("fb_game"), cameraProvider))
                        .Renderer("r_sprite")
                        .Source("s_sprite")
                        .Build())
                    .Pass("rp_main", rp => rp
                        .Dependency("rp_game")
                        .Target("fb_screen")
                        .ClearColor(RgbaFloat.Grey)
                        .Renderer("r_ui")
                        .Source("s_ui")
                        .Build())
                    .Build())
                .Build();
        }

        static IPalette BuildPalette()
        {
            var pal = new uint[256];
            pal[0] = 0;
            for (int i = 1; i < 256; i++)
            {
                var f = (float)(i - 1) / 254;
                var b = (byte)(f * 256);
                pal[i] = ApiUtil.PackColor(b, b, b, 255);
            }

            return new SimplePalette(1, "Palette", new SimpleTexture<uint>(new PaletteId(1), "Palette", 256, 1, pal));
        }
    }
}

