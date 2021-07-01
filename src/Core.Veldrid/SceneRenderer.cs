using System;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid.Events;
using UAlbion.Core.Veldrid.Sprites;
using UAlbion.Core.Visual;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid
{
    public sealed class SceneRenderer : Container, ISceneRenderer, IDisposable
    {
        readonly SpriteRenderer _spriteRenderer;
        readonly EtmRenderer _etmRenderer;
        readonly SkyboxRenderer _skyboxRenderer;
        readonly DebugGuiRenderer _debugRenderer;

        readonly SingleBuffer<GlobalInfo> _globalInfo;
        readonly SingleBuffer<ProjectionMatrix> _projection;
        readonly SingleBuffer<ViewMatrix> _view;
        readonly CommonSet _commonSet;
        ITextureHolder _palette;
        (float Red, float Green, float Blue, float Alpha) _clearColour;

        public SceneRenderer(string name, IFramebufferHolder framebuffer, Renderers renderers) : base(name)
        {
            Framebuffer = framebuffer ?? throw new ArgumentNullException(nameof(framebuffer));

            On<SetClearColourEvent>(e => _clearColour = (e.Red, e.Green, e.Blue, e.Alpha));
            On<PostEngineUpdateEvent>(_ => UpdatePerFrameResources());

            if ((renderers & Renderers.Sprite) != 0)
            {
                _spriteRenderer = new SpriteRenderer(framebuffer);
                AttachChild(_spriteRenderer);
            }

            if ((renderers & Renderers.ExtrudedTilemap) != 0)
            {
                _etmRenderer = new EtmRenderer(framebuffer);
                AttachChild(_etmRenderer);
            }

            if ((renderers & Renderers.Skybox) != 0)
            {
                _skyboxRenderer = new SkyboxRenderer(framebuffer);
                AttachChild(_skyboxRenderer);
            }

            if ((renderers & Renderers.DebugGui) != 0)
            {
                _debugRenderer = new DebugGuiRenderer(framebuffer);
                AttachChild(_debugRenderer);
            }

            _projection = new SingleBuffer<ProjectionMatrix>(BufferUsage.UniformBuffer | BufferUsage.Dynamic, "M_Projection");
            _view = new SingleBuffer<ViewMatrix>(BufferUsage.UniformBuffer | BufferUsage.Dynamic, "M_View");
            _globalInfo = new SingleBuffer<GlobalInfo>(BufferUsage.UniformBuffer | BufferUsage.Dynamic, "B_GlobalInfo");
            _commonSet = new CommonSet
            {
                Name = "RS_Common",
                GlobalInfo = _globalInfo,
                Projection = _projection,
                View = _view,
            };
            AttachChild(_projection);
            AttachChild(_view);
            AttachChild(_globalInfo);
            AttachChild(_commonSet);
        }

        public IFramebufferHolder Framebuffer { get; }
        public override string ToString() => $"Scene:{Name}";
        public void Render(GraphicsDevice device, CommandList cl)
        {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (cl == null) throw new ArgumentNullException(nameof(cl));

            // Sort:
            // Opaque, front-to-back (map, then sprites)
            // Transparent, back-to-front (1 call per map-tile / sprite)
            // CoreTrace.Log.Info("Scene", "Sorted processed renderables");

            // Main scene
            using (PerfTracker.FrameEvent("6.2.3 Main scene pass"))
            {
                cl.SetFramebuffer(Framebuffer.Framebuffer);
                cl.SetFullViewports();
                cl.SetFullScissorRects();
                cl.ClearColorTarget(0, new RgbaFloat(_clearColour.Red, _clearColour.Green, _clearColour.Blue, 1.0f));
                cl.ClearDepthStencil(device.IsDepthRangeZeroToOne ? 1f : 0f);

                if (_skyboxRenderer != null)
                {
                    var skybox = TryResolve<ISkybox>();
                    if (skybox != null)
                        _skyboxRenderer.Render(cl, (Skybox) skybox, _commonSet, Framebuffer);
                }

                if(_etmRenderer != null)
                    foreach (var tilemap in ((EtmManager)Resolve<IEtmManager>()).Ordered)
                        _etmRenderer.Render(cl, (DungeonTilemap)tilemap, _commonSet, Framebuffer);

                if (_spriteRenderer != null)
                    foreach (var batch in ((SpriteManager)Resolve<ISpriteManager>()).Ordered)
                        _spriteRenderer.Render(cl, (VeldridSpriteBatch)batch, _commonSet, Framebuffer);

                if (_debugRenderer != null)
                    _debugRenderer.Render(device, cl);
            }
        }

        public void UpdatePerFrameResources()
        {
            var camera = Resolve<ICamera>();
            var clock = TryResolve<IClock>();
            var settings = TryResolve<IEngineSettings>();
            var paletteManager = Resolve<IPaletteManager>();
            var textureSource = Resolve<ITextureSource>();

            camera.Viewport = new Vector2(Framebuffer.Width, Framebuffer.Height);
            var palette = textureSource.GetSimpleTexture(paletteManager.PaletteTexture);
            if (_palette != palette)
            {
                _palette = palette;
                _commonSet.Palette = _palette;
            }

            var info = new GlobalInfo
            {
                WorldSpacePosition = camera.Position,
                CameraDirection = camera.LookDirection,
                Resolution =  new Vector2(Framebuffer.Width, Framebuffer.Height),
                Time = clock?.ElapsedTime ?? 0,
                Special1 = settings?.Special1 ?? 0,
                Special2 = settings?.Special2 ?? 0,
                EngineFlags = settings?.Flags ?? 0,
                PaletteBlend = paletteManager.PaletteBlend
            };

            _projection.Data = new ProjectionMatrix(camera.ProjectionMatrix);
            _view.Data = new ViewMatrix(camera.ViewMatrix);
            _globalInfo.Data = info;
        }

        public void Dispose()
        {
            _commonSet?.Dispose();
            _debugRenderer?.Dispose();
            _skyboxRenderer?.Dispose();
            _spriteRenderer?.Dispose();
            _etmRenderer?.Dispose();
            _globalInfo.Dispose();
            _projection.Dispose();
            _view.Dispose();
        }
    }
}
