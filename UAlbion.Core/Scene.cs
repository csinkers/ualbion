using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Veldrid;

namespace UAlbion.Core
{
    public class Scene
    {
        readonly IDictionary<int, IList<IRenderable>> _renderables = new Dictionary<int, IList<IRenderable>>();
        readonly IDictionary<Type, IRenderer> _renderers = new Dictionary<Type, IRenderer>();
        Palette _palette;
        CommandList _resourceUpdateCL;

        public ICamera Camera { get; }

        public EventExchange Exchange { get; } = new EventExchange();

        public Scene(ICamera camera) { Camera = camera; }

        public void SetPalette(uint[] palette)
        {
            _palette = new Palette(palette);
        }

        public void AddRenderer(IRenderer r)
        {
            _renderers.Add(r.GetType(), r);
        }

        public void AddComponent(IComponent component)
        {
            Debug.Assert(component != null);
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
                if (!_renderables.ContainsKey(x.RenderOrder))
                    _renderables[x.RenderOrder] = new List<IRenderable>();
                _renderables[x.RenderOrder].Add(x);
            }, t => _renderers[t]), this);

            sc.Palette?.Dispose();
            sc.PaletteTexture?.Dispose();
            // TODO: Ensure always disposed
            sc.PaletteTexture = _palette.CreateDeviceTexture(gd, gd.ResourceFactory, TextureUsage.Sampled);
            sc.Palette = gd.ResourceFactory.CreateTextureView(sc.PaletteTexture);

            // Update frame resources
            _resourceUpdateCL.Begin();
            foreach (IRenderable r in _renderables.SelectMany(x => x.Value))
            {
                var renderer = _renderers[r.Renderer];
                renderer.UpdatePerFrameResources(gd, _resourceUpdateCL, sc, r);
            }

            _resourceUpdateCL.End();
            gd.SubmitCommands(_resourceUpdateCL);

            var orderedKeys = _renderables.Keys.OrderBy(x => x).ToList();
            float depthClear = gd.IsDepthRangeZeroToOne ? 0f : 1f;

            // Main scene
            cl.PushDebugGroup("Main Scene Pass");
            cl.SetFramebuffer(sc.MainSceneFramebuffer);
            var fbWidth = sc.MainSceneFramebuffer.Width;
            var fbHeight = sc.MainSceneFramebuffer.Height;
            cl.SetViewport(0, new Viewport(0, 0, fbWidth, fbHeight, 0, 1));
            cl.SetFullViewports();
            cl.SetFullScissorRects();
            cl.ClearDepthStencil(depthClear);
            sc.UpdateCameraBuffers(cl); // Re-set because reflection step changed it.
            foreach(var key in orderedKeys)
                Render(gd, cl, sc, RenderPasses.Standard, _renderables[key]);
            cl.PopDebugGroup();

            // 2D Overlays
            cl.PushDebugGroup("Overlay");
            foreach (var key in orderedKeys)
                Render(gd, cl, sc, RenderPasses.Overlay, _renderables[key]);
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
                Render(gd, cl, sc, RenderPasses.Duplicator, _renderables[key]);
            cl.PopDebugGroup();

            // Swapchain
            cl.PushDebugGroup("Swapchain Pass");
            cl.SetFramebuffer(gd.SwapchainFramebuffer);
            cl.SetFullViewports();
            foreach (var key in orderedKeys)
                Render(gd, cl, sc, RenderPasses.SwapchainOutput, _renderables[key]);
            cl.PopDebugGroup();

            cl.End();
            gd.SubmitCommands(cl);
        }

        void Render(GraphicsDevice gd,
            CommandList rc,
            SceneContext sc,
            RenderPasses pass,
            IEnumerable<IRenderable> renderableList)
        {
            foreach (IRenderable renderable in renderableList)
            {
                var renderer = _renderers[renderable.Renderer];
                if ((renderer.RenderPasses & pass) != 0)
                    renderer.Render(gd, rc, sc, pass, renderable);
            }
        }

        internal void DestroyAllDeviceObjects()
        {
            foreach (var r in _renderers.Values)
                r.DestroyDeviceObjects();

            _resourceUpdateCL.Dispose();
        }

        internal void CreateAllDeviceObjects(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
            foreach (var r in _renderers.Values)
                r.CreateDeviceObjects(gd, cl, sc);

            _resourceUpdateCL = gd.ResourceFactory.CreateCommandList();
            _resourceUpdateCL.Name = "Scene Resource Update Command List";
        }
    }
}
