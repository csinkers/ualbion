using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Core.Events;
using UAlbion.Core.Textures;
using Veldrid;

namespace UAlbion.Core
{
    public class Scene : Component, IScene
    {
        readonly IList<Type> _activeRendererTypes;

        static readonly HandlerSet Handlers = new HandlerSet(
            H<Scene, SetRawPaletteEvent>((x, e) => x._palette = new Palette(e.Name, e.Entries)),
            H<Scene, SetSceneEvent>((x, e) => x.SceneExchange.IsActive = e.SceneId == x.Id),
            H<Scene, SetClearColourEvent>((x, e) => x._clearColour = new RgbaFloat(e.Red, e.Green, e.Blue, 1.0f)),
            H<Scene, SubscribedEvent>((x, e) => x.SceneExchange.Attach(x.Camera))
        );

        readonly IDictionary<Type, IList<IRenderable>> _renderables = new Dictionary<Type, IList<IRenderable>>();
        readonly IDictionary<int, IList<IRenderable>> _processedRenderables = new Dictionary<int, IList<IRenderable>>();
        Palette _palette;
        CommandList _resourceUpdateCl;
        RgbaFloat _clearColour;

        int Id { get; }
        public EventExchange SceneExchange { get; }
        public string Name { get; }
        public ICamera Camera { get; }

        protected Scene(int sceneId, string name, ICamera camera, IList<Type> activeActiveRendererTypeTypes, EventExchange sceneExchange) : base(Handlers)
        {
            Id = sceneId;
            Name = name;
            Camera = camera;
            _activeRendererTypes = activeActiveRendererTypeTypes;
            SceneExchange = sceneExchange;
        }

        public void Add(IRenderable renderable) { } // TODO
        public void Remove(IRenderable renderable) { } // TODO

        public override string ToString() => $"Scene:{Name} {(SceneExchange.IsActive ? "Active" : "")}";

        public void RenderAllStages(GraphicsDevice gd, CommandList cl, SceneContext sc, IDictionary<Type, IRenderer> renderers)
        {
            if (!SceneExchange.IsActive)
                return;

            sc.SetCurrentScene(this);

            // Collect all renderables from components
            foreach(var renderer in _renderables.Values)
                renderer.Clear();

            Exchange.Raise(new RenderEvent(x =>
            {
                if (x == null || !_activeRendererTypes.Contains(x.Renderer))
                    return;
                if (!_renderables.ContainsKey(x.Renderer))
                    _renderables[x.Renderer] = new List<IRenderable>();
                _renderables[x.Renderer].Add(x);
            }), this);

            foreach(var renderer in _renderables)
                CoreTrace.Log.CollectedRenderables(renderer.Key.Name, 0, renderer.Value.Count);

            sc.PaletteView?.Dispose();
            sc.PaletteTexture?.Dispose();
            CoreTrace.Log.Info("Scene", "Disposed palette device texture");
            sc.PaletteTexture = _palette.CreateDeviceTexture(gd, gd.ResourceFactory, TextureUsage.Sampled);
            sc.PaletteView = gd.ResourceFactory.CreateTextureView(sc.PaletteTexture);
            CoreTrace.Log.Info("Scene", "Created palette device texture");

            _resourceUpdateCl.Begin();
            using (new RenderDebugGroup(_resourceUpdateCl, "Prepare per-frame resources"))
            {
                _processedRenderables.Clear();
                foreach (var renderableGroup in _renderables)
                {
                    var renderer = renderers[renderableGroup.Key];
                    foreach (var renderable in renderer.UpdatePerFrameResources(gd, _resourceUpdateCl, sc,
                        renderableGroup.Value))
                    {
                        if (!_processedRenderables.ContainsKey(renderable.RenderOrder))
                            _processedRenderables[renderable.RenderOrder] = new List<IRenderable>();
                        _processedRenderables[renderable.RenderOrder].Add(renderable);
                    }
                }

                CoreTrace.Log.CollectedRenderables("ProcessedRenderables",
                    _processedRenderables.Count,
                    _processedRenderables.Sum(x => x.Value.Count));
            }
            _resourceUpdateCl.End();
            gd.SubmitCommands(_resourceUpdateCl);
            CoreTrace.Log.Info("Scene", "Submitted resource update commandlist");

            var orderedKeys = _processedRenderables.Keys.OrderBy(x => x).ToList();
            CoreTrace.Log.Info("Scene", "Sorted processed renderables");
            float depthClear = gd.IsDepthRangeZeroToOne ? 1f : 0f;

            // Main scene
            using (new RenderDebugGroup(cl, "Main Scene Pass"))
            {
                sc.UpdateCameraBuffers(cl);
                cl.SetFramebuffer(sc.MainSceneFramebuffer);
                var fbWidth = sc.MainSceneFramebuffer.Width;
                var fbHeight = sc.MainSceneFramebuffer.Height;
                cl.SetViewport(0, new Viewport(0, 0, fbWidth, fbHeight, 0, 1));
                cl.SetFullViewports();
                cl.SetFullScissorRects();
                cl.ClearColorTarget(0, _clearColour);
                cl.ClearDepthStencil(depthClear);
                foreach (var key in orderedKeys)
                    Render(gd, cl, sc, RenderPasses.Standard, renderers, _processedRenderables[key]);
            }

            // 2D Overlays
            using (new RenderDebugGroup(cl, "Overlay"))
            {
                foreach (var key in orderedKeys)
                    Render(gd, cl, sc, RenderPasses.Overlay, renderers, _processedRenderables[key]);
            }

            if (sc.MainSceneColorTexture.SampleCount != TextureSampleCount.Count1)
                cl.ResolveTexture(sc.MainSceneColorTexture, sc.MainSceneResolvedColorTexture);

            using (new RenderDebugGroup(cl, "Duplicator"))
            {
                cl.SetFramebuffer(sc.DuplicatorFramebuffer);
                cl.SetFullViewports();
                foreach (var key in orderedKeys)
                    Render(gd, cl, sc, RenderPasses.Duplicator, renderers, _processedRenderables[key]);
            }

            using (new RenderDebugGroup(cl, "Swapchain Pass"))
            {
                cl.SetFramebuffer(gd.SwapchainFramebuffer);
                cl.SetFullViewports();
                foreach (var key in orderedKeys)
                    Render(gd, cl, sc, RenderPasses.SwapchainOutput, renderers, _processedRenderables[key]);
            }

            cl.End();
            CoreTrace.Log.Info("Scene", "Submitting Commands");
            gd.SubmitCommands(cl);
            CoreTrace.Log.Info("Scene", "Submitted commands");
        }

        void Render(GraphicsDevice gd,
            CommandList cl,
            SceneContext sc,
            RenderPasses pass,
            IDictionary<Type, IRenderer> renderers,
            IEnumerable<IRenderable> renderableList)
        {
            foreach (IRenderable renderable in renderableList)
            {
                if(renderable is IScreenSpaceRenderable)
                    sc.UpdateModelTransform(cl, renderable.Transform);
                else
                    sc.UpdateModelTransform(cl, Camera.ViewMatrix * renderable.Transform);

                var renderer = renderers[renderable.Renderer];
                if ((renderer.RenderPasses & pass) != 0)
                    renderer.Render(gd, cl, sc, pass, renderable);
            }
        }

        internal void DestroyAllDeviceObjects()
        {
            _resourceUpdateCl.Dispose();
        }

        internal void CreateAllDeviceObjects(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
            _resourceUpdateCl = gd.ResourceFactory.CreateCommandList();
            _resourceUpdateCl.Name = "Scene Resource Update Command List";
        }
    }
}
