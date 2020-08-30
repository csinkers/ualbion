﻿using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Api;
using UAlbion.Core.Events;

namespace UAlbion.Core
{
    public class Scene : Container, IScene
    {
        readonly IDictionary<Type, IList<IRenderable>> _renderables = new Dictionary<Type, IList<IRenderable>>();
        readonly IDictionary<(DrawLayer, int), IList<IRenderable>> _processedRenderables = new Dictionary<(DrawLayer, int), IList<IRenderable>>();
        (float Red, float Green, float Blue) _clearColour;

        public ICamera Camera { get; }

        protected Scene(string name, ICamera camera) : base(name)
        {
            On<CollectScenesEvent>(e => e.Register(this));
            On<SetClearColourEvent>(e => _clearColour = (e.Red, e.Green, e.Blue));

            Camera = AttachChild(camera);
        }

        public override string ToString() => $"Scene:{Name}";

        public void RenderAllStages(IRendererContext context, IList<IRenderer> renderers)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            context.SetCurrentScene(this);
            context.SetClearColor(_clearColour.Red, _clearColour.Green, _clearColour.Blue);

            // Collect all renderables from components
            foreach (var renderer in _renderables.Values)
                renderer.Clear();

            using (PerfTracker.FrameEvent("6.1.1 Collect renderables"))
            {
                Raise(new RenderEvent(x =>
                {
                    var type = x.GetType();
                    if (!_renderables.ContainsKey(type))
                        _renderables[type] = new List<IRenderable>();
                    _renderables[type].Add(x);
                }));
            }

            foreach (var renderer in _renderables)
                CoreTrace.Log.CollectedRenderables(renderer.Key.Name, 0, renderer.Value.Count);

            var paletteManager = Resolve<IPaletteManager>();
            context.SetCurrentPalette(paletteManager.PaletteTexture, paletteManager.Version);

            CoreTrace.Log.Info("Scene", "Created palette device texture");

            var rendererLookup = new Dictionary<Type, IRenderer>();

            using (PerfTracker.FrameEvent("6.1.2 Prepare per-frame resources"))
            using (context.Factory.CreateRenderDebugGroup(context, "Prepare per-frame resources"))
            {
                _processedRenderables.Clear();
                foreach (var renderableGroup in _renderables)
                {
                    var renderer = renderers.FirstOrDefault(x => x.CanRender(renderableGroup.Key));
                    if (renderer == null) continue;

                    foreach (var renderable in renderer.UpdatePerFrameResources(context, renderableGroup.Value))
                    {
                        var key = (renderable.RenderOrder, renderable.PipelineId);
                        if (!_processedRenderables.ContainsKey(key))
                            _processedRenderables[key] = new List<IRenderable>();
                        _processedRenderables[key].Add(renderable);

                        var processedType = renderable.GetType();
                        if (!rendererLookup.ContainsKey(processedType))
                            rendererLookup[processedType] = renderers.FirstOrDefault(x => x.CanRender(renderableGroup.Key));
                    }
                }

                CoreTrace.Log.CollectedRenderables("ProcessedRenderables",
                        _processedRenderables.Count,
                        _processedRenderables.Sum(x => x.Value.Count));

                context.UpdatePerFrameResources();
            }

            var orderedKeys = _processedRenderables.Keys.OrderBy(x => x).ToList();
            CoreTrace.Log.Info("Scene", "Sorted processed renderables");

            // Main scene
            using (PerfTracker.FrameEvent("6.1.3 Main scene pass"))
            using (context.Factory.CreateRenderDebugGroup(context, "Main Pass"))
            {
                context.StartSwapchainPass();
                foreach (var key in orderedKeys)
                    Render(context, RenderPasses.Standard, rendererLookup, _processedRenderables[key]);
            }
        }

        static void Render(IRendererContext context,
            RenderPasses pass,
            IDictionary<Type, IRenderer> renderers,
            IEnumerable<IRenderable> renderableList)
        {
            foreach (IRenderable renderable in renderableList)
            {
                var renderer = renderers[renderable.GetType()];
                if ((renderer.RenderPasses & pass) != 0)
                    renderer.Render(context, pass, renderable);
            }
        }
    }
}
