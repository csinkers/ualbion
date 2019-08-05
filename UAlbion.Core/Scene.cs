using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Veldrid;

namespace UAlbion.Core
{
    public class Scene
    {
        readonly IDictionary<Type, IList<IRenderable>> _renderables = new Dictionary<Type, IList<IRenderable>>();
        readonly IDictionary<int, IList<IRenderable>> _processedRenderables = new Dictionary<int, IList<IRenderable>>();
        readonly IDictionary<Type, IRenderer> _renderers = new Dictionary<Type, IRenderer>();
        Palette _palette;
        CommandList _resourceUpdateCl;

        public ICamera Camera { get; }

        public EventExchange Exchange { get; } = new EventExchange();

        public Scene(ICamera camera) { Camera = camera; }

        public void SetPalette(string name, uint[] palette)
        {
            _palette = new Palette(name, palette);
        }

        public void AddRenderer(IRenderer r)
        {
            _renderers.Add(r.GetType(), r);
        }

        public void AddComponent(IComponent component)
        {
            Debug.Assert(component != null);
            if (component is RegisteredComponent rc)
                Exchange.Register(rc);
            component.Attach(Exchange);
        }

        public void RemoveComponent(IComponent component)
        {
            Exchange.Unsubscribe(component);
        }

        public void RenderAllStages(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
            // Collect all renderables from components
            _renderables.Clear();
            Exchange.Raise(new RenderEvent(x =>
            {
                if (!_renderables.ContainsKey(x.Renderer))
                    _renderables[x.Renderer] = new List<IRenderable>();
                _renderables[x.Renderer].Add(x);
            }, t => _renderers[t]), this);

            sc.PaletteView?.Dispose();
            sc.PaletteTexture?.Dispose();
            // TODO: Ensure always disposed
            sc.PaletteTexture = _palette.CreateDeviceTexture(gd, gd.ResourceFactory, TextureUsage.Sampled);
            sc.PaletteView = gd.ResourceFactory.CreateTextureView(sc.PaletteTexture);

            // Update frame resources
            _resourceUpdateCl.Begin();
            _processedRenderables.Clear();
            foreach (var renderableGroup in _renderables)
            {
                var renderer = _renderers[renderableGroup.Key];
                foreach (var renderable in renderer.UpdatePerFrameResources(gd, _resourceUpdateCl, sc, renderableGroup.Value))
                {
                    if (!_processedRenderables.ContainsKey(renderable.RenderOrder))
                        _processedRenderables[renderable.RenderOrder] = new List<IRenderable>();
                    _processedRenderables[renderable.RenderOrder].Add(renderable);
                }
            }
            _resourceUpdateCl.End();
            gd.SubmitCommands(_resourceUpdateCl);

            var orderedKeys = _processedRenderables.Keys.OrderBy(x => x).ToList();
            float depthClear = gd.IsDepthRangeZeroToOne ? 0f : 1f;

            // Main scene
            cl.PushDebugGroup("Main Scene Pass");
            sc.UpdateCameraBuffers(cl);
            cl.SetFramebuffer(sc.MainSceneFramebuffer);
            var fbWidth = sc.MainSceneFramebuffer.Width;
            var fbHeight = sc.MainSceneFramebuffer.Height;
            cl.SetViewport(0, new Viewport(0, 0, fbWidth, fbHeight, 0, 1));
            cl.SetFullViewports();
            cl.SetFullScissorRects();
            cl.ClearColorTarget(0, RgbaFloat.Black);
            cl.ClearDepthStencil(depthClear);
            foreach(var key in orderedKeys)
                Render(gd, cl, sc, RenderPasses.Standard, _processedRenderables[key]);
            cl.PopDebugGroup();

            // 2D Overlays
            cl.PushDebugGroup("Overlay");
            foreach (var key in orderedKeys)
                Render(gd, cl, sc, RenderPasses.Overlay, _processedRenderables[key]);
            cl.PopDebugGroup();

            if (sc.MainSceneColorTexture.SampleCount != TextureSampleCount.Count1)
            {
                cl.ResolveTexture(sc.MainSceneColorTexture, sc.MainSceneResolvedColorTexture);
            }

            // Duplicator
            cl.PushDebugGroup("Duplicator");
            cl.SetFramebuffer(sc.DuplicatorFramebuffer);
            cl.SetFullViewports();
            foreach (var key in orderedKeys)
                Render(gd, cl, sc, RenderPasses.Duplicator, _processedRenderables[key]);
            cl.PopDebugGroup();

            // Swapchain
            cl.PushDebugGroup("Swapchain Pass");
            cl.SetFramebuffer(gd.SwapchainFramebuffer);
            cl.SetFullViewports();
            foreach (var key in orderedKeys)
                Render(gd, cl, sc, RenderPasses.SwapchainOutput, _processedRenderables[key]);
            cl.PopDebugGroup();

            cl.End();
            gd.SubmitCommands(cl);
        }

        void Render(GraphicsDevice gd,
            CommandList cl,
            SceneContext sc,
            RenderPasses pass,
            IEnumerable<IRenderable> renderableList)
        {
            foreach (IRenderable renderable in renderableList)
            {
                var renderer = _renderers[renderable.Renderer];
                if ((renderer.RenderPasses & pass) != 0)
                    renderer.Render(gd, cl, sc, pass, renderable);
            }
        }

        internal void DestroyAllDeviceObjects()
        {
            foreach (var r in _renderers.Values)
                r.DestroyDeviceObjects();

            _resourceUpdateCl.Dispose();
        }

        internal void CreateAllDeviceObjects(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
            foreach (var r in _renderers.Values)
                r.CreateDeviceObjects(gd, cl, sc);

            _resourceUpdateCl = gd.ResourceFactory.CreateCommandList();
            _resourceUpdateCl.Name = "Scene Resource Update Command List";
        }
    }
}
