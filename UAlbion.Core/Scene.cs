using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UAlbion.Core.Events;
using UAlbion.Core.Textures;
using Veldrid;

namespace UAlbion.Core
{
    public class RenderDebugGroup : IDisposable
    {
        readonly CommandList _cl;
        readonly string _name;

        public RenderDebugGroup(CommandList cl, string name)
        {
            _cl = cl;
            _name = name;
            CoreTrace.Log.StartDebugGroup(name);
            cl.PushDebugGroup(name);
        }

        public void Dispose()
        {
            _cl.PopDebugGroup();
            CoreTrace.Log.StopDebugGroup(_name);
        }
    }

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
            foreach(var renderer in _renderables.Values)
                renderer.Clear();

            /*
            Func<Vector2, Vector2, bool> cullFunc = (p, s) => true;
            if(Camera is OrthographicCamera c)
            {
                cullFunc = (p, s) =>
                {
                    if (p.X > (c.WindowWidth / c.Magnification) || p.Y > (c.WindowHeight / c.Magnification))
                        return false;

                    if (p.X + s.X < 0 || p.Y + s.Y < 0)
                        return false;

                    return true;
                };
            }*/

            int collected = 0;
            Exchange.Raise(new RenderEvent(x =>
            {
                if(collected % 100 == 0)
                    CoreTrace.Log.CollectedRenderables("Progress", collected);

                if (!_renderables.ContainsKey(x.Renderer))
                    _renderables[x.Renderer] = new List<IRenderable>();
                _renderables[x.Renderer].Add(x);
                collected++;
            }, t => _renderers[t]), this);

            foreach(var renderer in _renderables)
                CoreTrace.Log.CollectedRenderables(renderer.Key.Name, renderer.Value.Count);

            sc.PaletteView?.Dispose();
            sc.PaletteTexture?.Dispose();
            CoreTrace.Log.Info("Scene", "Disposed palette device texture");
            // TODO: Ensure always disposed
            sc.PaletteTexture = _palette.CreateDeviceTexture(gd, gd.ResourceFactory, TextureUsage.Sampled);
            sc.PaletteView = gd.ResourceFactory.CreateTextureView(sc.PaletteTexture);
            CoreTrace.Log.Info("Scene", "Created palette device texture");

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

            CoreTrace.Log.CollectedRenderables("ProcessedRenderableTypes", _processedRenderables.Count);
            CoreTrace.Log.CollectedRenderables("ProcessedRenderables", _processedRenderables.Sum(x => x.Value.Count));

            _resourceUpdateCl.End();
            gd.SubmitCommands(_resourceUpdateCl);
            CoreTrace.Log.Info("Scene", "Submitted resource update commandlist");

            var orderedKeys = _processedRenderables.Keys.OrderBy(x => x).ToList();
            CoreTrace.Log.Info("Scene", "Sorted processed renderables");
            float depthClear = gd.IsDepthRangeZeroToOne ? 0f : 1f;

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
                cl.ClearColorTarget(0, RgbaFloat.Black);
                cl.ClearDepthStencil(depthClear);
                foreach (var key in orderedKeys)
                    Render(gd, cl, sc, RenderPasses.Standard, _processedRenderables[key]);
            }

            // 2D Overlays
            using (new RenderDebugGroup(cl, "Overlay"))
            {
                foreach (var key in orderedKeys)
                    Render(gd, cl, sc, RenderPasses.Overlay, _processedRenderables[key]);
            }

            if (sc.MainSceneColorTexture.SampleCount != TextureSampleCount.Count1)
            {
                cl.ResolveTexture(sc.MainSceneColorTexture, sc.MainSceneResolvedColorTexture);
            }

            using (new RenderDebugGroup(cl, "Duplicator"))
            {
                cl.SetFramebuffer(sc.DuplicatorFramebuffer);
                cl.SetFullViewports();
                foreach (var key in orderedKeys)
                    Render(gd, cl, sc, RenderPasses.Duplicator, _processedRenderables[key]);
            }

            using (new RenderDebugGroup(cl, "Swapchain Pass"))
            {
                cl.SetFramebuffer(gd.SwapchainFramebuffer);
                cl.SetFullViewports();
                foreach (var key in orderedKeys)
                    Render(gd, cl, sc, RenderPasses.SwapchainOutput, _processedRenderables[key]);
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
