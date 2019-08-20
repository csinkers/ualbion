﻿using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Api;
using UAlbion.Core.Events;
using UAlbion.Core.Textures;
using Veldrid;

namespace UAlbion.Core
{
    [Event("set_scene", "Set the active scene")]
    public class SetSceneEvent : EngineEvent
    {
        public SetSceneEvent(int sceneId)
        {
            SceneId = sceneId;
        }

        [EventPart("id", "The identifier of the scene to activate")]
        public int SceneId { get; }
    }

    public class Scene : Component
    {
        readonly IList<Type> _activeRendererTypes;

        static readonly IList<Handler> Handlers = new Handler[]
        {
            new Handler<Scene, SetRawPaletteEvent>((x, e) => x._palette = new Palette(e.Name, e.Entries)),
            new Handler<Scene, SetSceneEvent>((x, e) => x._isActive = e.SceneId == x.Id),
        };

        readonly IDictionary<Type, IList<IRenderable>> _renderables = new Dictionary<Type, IList<IRenderable>>();
        readonly IDictionary<int, IList<IRenderable>> _processedRenderables = new Dictionary<int, IList<IRenderable>>();
        Palette _palette;
        CommandList _resourceUpdateCl;
        bool _isActive;

        public int Id { get; }
        public ICamera Camera { get; }
        public EventExchange SceneExchange => Exchange;

        public Scene(int sceneId, ICamera camera, IList<Type> activeActiveRendererTypeTypes) : base(Handlers)
        {
            Id = sceneId;
            Camera = camera;
            _activeRendererTypes = activeActiveRendererTypeTypes;
        }

        public void RenderAllStages(GraphicsDevice gd, CommandList cl, SceneContext sc, IDictionary<Type, IRenderer> renderers)
        {
            if (!_isActive)
                return;

            sc.SetCurrentScene(this);

            // Collect all renderables from components
            foreach(var renderer in _renderables.Values)
                renderer.Clear();

            Exchange.Raise(new RenderEvent(x =>
            {
                if (!_activeRendererTypes.Contains(x.Renderer))
                    return;
                if (!_renderables.ContainsKey(x.Renderer))
                    _renderables[x.Renderer] = new List<IRenderable>();
                _renderables[x.Renderer].Add(x);
            }), this);

            foreach(var renderer in _renderables)
                CoreTrace.Log.CollectedRenderables(renderer.Key.Name, renderer.Value.Count);

            sc.PaletteView?.Dispose();
            sc.PaletteTexture?.Dispose();
            CoreTrace.Log.Info("Scene", "Disposed palette device texture");
            sc.PaletteTexture = _palette.CreateDeviceTexture(gd, gd.ResourceFactory, TextureUsage.Sampled);
            sc.PaletteView = gd.ResourceFactory.CreateTextureView(sc.PaletteTexture);
            CoreTrace.Log.Info("Scene", "Created palette device texture");

            // Update frame resources
            _resourceUpdateCl.Begin();
            _processedRenderables.Clear();
            foreach (var renderableGroup in _renderables)
            {
                var renderer = renderers[renderableGroup.Key];
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
