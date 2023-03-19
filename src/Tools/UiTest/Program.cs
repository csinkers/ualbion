using System.Reflection;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Api.Settings;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Core.Veldrid;
using UAlbion.Core.Veldrid.Sprites;
using UAlbion.Core.Veldrid.Textures;
using UAlbion.Core.Visual;
using UAlbion.Formats.Ids;
using Veldrid;

namespace UiTest
{
    public static class Program
    {
        public static void Main()
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
            var renderSystem = BuildRenderSystem(new SimpleCameraProvider(camera));

            exchange
                .Register<IFileSystem>(disk)
                .Register<IPathResolver>(pathResolver)
                .Register<IVarSet>(new VarSet("Default"))
                .Register<IPaletteManager>(new DummyPaletteManager(BuildPalette(), null))
                .Attach(new ResourceLayoutSource())
                .Attach(new TextureSource())
                .Attach(shaderCache)
                .Attach(shaderLoader)
                .Attach(camera)
                .Attach(renderSystem)
                .Attach(engine)
                ;

            engine.RenderSystem = renderSystem.GetPipeline("pl_main");

            // var texture = new SimpleTexture<uint>(new AssetId(AssetType.))

            engine.Run();
        }

        static RenderSystem BuildRenderSystem(ICameraProvider cameraProvider)
        {
            return RenderSystemBuilder.Create()
                .Framebuffer("fb_screen", new MainFramebuffer("fb_screen"))
                .Framebuffer("fb_game", new SimpleFramebuffer("fb_game", 1, 1))
                .Renderer("r_ui", new DebugGuiRenderer(SimpleFramebuffer.Output))
                .Renderer("r_sprite", new SpriteRenderer(SimpleFramebuffer.Output))
                .Source("s_ui", new DebugGuiRenderable())
                .Source("s_sprite", new BatchManager<SpriteKey, SpriteInfo>(static (key, f) => f.CreateSpriteBatch(key)))
                .Pipeline("pl_main", pb => pb
                    .Resources(new GlobalResourceSetProvider())
                    .Pass("rp_game", rp => rp
                        .Target("fb_game")
                        .Resources(new MainPassResourceProvider(pb.GetFramebuffer("fb_game"), cameraProvider))
                        .Renderer("r_sprite")
                        .Source("s_sprite")
                        .Render((pass, device, cl, set1) =>
                        {
                            cl.SetFullViewports();
                            cl.SetFullScissorRects();
                            cl.ClearDepthStencil(device.IsDepthRangeZeroToOne ? 1f : 0f);
                            cl.ClearColorTarget(0, RgbaFloat.Clear);
                            pass.CollectAndDraw(device, cl, set1);
                        })
                        .Build())
                    .Pass("rp_main", rp => rp
                        .Dependency("rp_game")
                        .Target("fb_screen")
                        .Renderer("r_ui")
                        .Source("s_ui")
                        .Render((pass, device, cl, set1) =>
                        {
                            cl.SetFullViewports();
                            cl.SetFullScissorRects();
                            cl.ClearDepthStencil(device.IsDepthRangeZeroToOne ? 1f : 0f);
                            cl.ClearColorTarget(0, RgbaFloat.Grey);
                            pass.CollectAndDraw(device, cl, set1);
                        })
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

