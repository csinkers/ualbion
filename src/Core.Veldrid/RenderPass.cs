using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid.Textures;
using UAlbion.Core.Visual;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid
{
    public sealed class RenderPass : Component, IRenderPass, IDisposable
    {
        readonly Dictionary<Type, IRenderer> _rendererLookup = new();
        readonly List<IRenderer> _renderers = new();
        readonly List<IRenderable> _renderList = new();
        readonly List<IRenderableSource> _sources = new();
        readonly SingleBuffer<GlobalInfo> _globalInfo;
        readonly SingleBuffer<ProjectionMatrix> _projection;
        readonly SingleBuffer<ViewMatrix> _view;
        readonly CommonSet _commonSet;
        ITextureHolder _palette;
        (float Red, float Green, float Blue, float Alpha) _clearColour;

        public RenderPass(string name, IFramebufferHolder framebuffer)
        {
            Name = name;
            Framebuffer = framebuffer ?? throw new ArgumentNullException(nameof(framebuffer));

            On<SetClearColourEvent>(e => _clearColour = (e.Red, e.Green, e.Blue, e.Alpha));
            On<RenderEvent>(_ => UpdatePerFrameResources());

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

        public RenderPass AddRenderer(IRenderer renderer, params Type[] types)
        {
            if (renderer == null) throw new ArgumentNullException(nameof(renderer));
            if (types == null) throw new ArgumentNullException(nameof(types));
            if (types.Length == 0) throw new ArgumentException("Value cannot be an empty collection.", nameof(types));

            if (!_renderers.Contains(renderer))
            {
                _renderers.Add(renderer);
                AttachChild(renderer);
            }

            foreach (var type in types)
            {
                if (!_rendererLookup.TryAdd(type, renderer))
                {
                    throw new InvalidOperationException(
                        $"Tried to register renderer of type {renderer.GetType().Name} for" +
                        $" rendering \"{type.Name}\", but they are already being handled by " +
                        _rendererLookup[type].GetType().Name);
                }
            }

            return this;
        }

        public RenderPass AddSource(IRenderableSource source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            _sources.Add(source);
            return this;
        }

        public string Name { get; }
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
                cl.ClearColorTarget(0, new RgbaFloat(_clearColour.Red, _clearColour.Green, _clearColour.Blue, _clearColour.Alpha));
                cl.ClearDepthStencil(device.IsDepthRangeZeroToOne ? 1f : 0f);

                _renderList.Clear();
                foreach (var source in _sources)
                    source.Collect(_renderList);

                _renderList.Sort((x, y) =>
                {
                    var x2 = (ushort)x.RenderOrder;
                    var y2 = (ushort)y.RenderOrder;
                    return x2 < y2 ? -1 : x2 > y2 ? 1 : 0;
                });

                foreach (var renderable in _renderList)
                {
                    if (_rendererLookup.TryGetValue(renderable.GetType(), out var renderer))
                        renderer.Render(renderable, _commonSet, Framebuffer, cl, device);
                }
            }
        }

        void UpdatePerFrameResources()
        {
            var camera = Resolve<ICamera>();
            var clock = TryResolve<IClock>();
            var settings = TryResolve<IEngineSettings>();
            var paletteManager = Resolve<IPaletteManager>();
            var textureSource = Resolve<ITextureSource>();

            camera.Viewport = new Vector2(Framebuffer.Width, Framebuffer.Height);
            var palette = textureSource.GetSimpleTexture(paletteManager.PaletteTexture, paletteManager.Version);
            if (_palette != palette)
            {
                _palette = palette;
                _commonSet.Palette = _palette;
            }

            var info = new GlobalInfo
            {
                WorldSpacePosition = camera.Position,
                CameraDirection = new Vector2(camera.Pitch, camera.Yaw),
                Resolution =  new Vector2(Framebuffer.Width, Framebuffer.Height),
                Time = clock?.ElapsedTime ?? 0,
                Special1 = settings?.Special1 ?? 0,
                // Special2 = settings?.Special2 ?? 0,
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
            _globalInfo.Dispose();
            _projection.Dispose();
            _view.Dispose();
            foreach (var renderer in _renderers.OfType<IDisposable>())
                renderer.Dispose();
            _renderers.Clear();
            _rendererLookup.Clear();
        }
    }
}
