using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Api;
using UAlbion.Api.Visual;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;

namespace UAlbion.Core
{
    public class Scene : Container, IScene
    {
        readonly IDictionary<(DrawLayer, int), List<IRenderable>> _processedRenderables = new Dictionary<(DrawLayer, int), List<IRenderable>>();
        (float Red, float Green, float Blue) _clearColour;

        protected Scene(string name) : base(name)
        {
            On<CollectScenesEvent>(e => e.Register(this));
            On<SetClearColourEvent>(e => _clearColour = (e.Red, e.Green, e.Blue));
        }

        public override string ToString() => $"Scene:{Name}";

        public void UpdatePerFrameResources(IRendererContext context, IDictionary<IRenderer, List<IRenderable>> renderables)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (renderables == null) throw new ArgumentNullException(nameof(renderables));

            foreach (var renderer in renderables)
                CoreTrace.Log.CollectedRenderables(renderer.Key.GetType().Name, 0, renderer.Value.Count);

            context.UpdatePerFrameResources();

            using (PerfTracker.FrameEvent("6.2.2 Prepare per-frame resources"))
            using (context.Factory.CreateRenderDebugGroup(context, "Prepare per-frame resources"))
            {

                _processedRenderables.Clear();
                List<IRenderable> processed = new List<IRenderable>();
                foreach (var renderableGroup in renderables)
                {
                    IRenderer renderer = renderableGroup.Key;
                    processed.Clear();
                    renderer.UpdatePerFrameResources(context, renderableGroup.Value, processed);
                    foreach (var renderable in processed)
                    {
                        var key = (renderable.RenderOrder, renderable.PipelineId);
                        if (!_processedRenderables.ContainsKey(key))
                            _processedRenderables[key] = new List<IRenderable>();
                        _processedRenderables[key].Add(renderable);
                    }
                }

                CoreTrace.Log.CollectedRenderables("ProcessedRenderables",
                        _processedRenderables.Count,
                        _processedRenderables.Sum(x => x.Value.Count));
            }
        }

        public void RenderAllStages(IRendererContext context, IDictionary<Type, IRenderer> renderers)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (renderers == null) throw new ArgumentNullException(nameof(renderers));
            context.SetClearColor(_clearColour.Red, _clearColour.Green, _clearColour.Blue);

            var orderedKeys = _processedRenderables.Keys.OrderBy(x => x).ToArray();
            CoreTrace.Log.Info("Scene", "Sorted processed renderables");

            // Main scene
            using (PerfTracker.FrameEvent("6.2.3 Main scene pass"))
            using (context.Factory.CreateRenderDebugGroup(context, "Main Pass"))
            {
                context.StartSwapchainPass();
                foreach (var key in orderedKeys)
                    Render(context, RenderPasses.Standard, _processedRenderables[key], renderers);
            }
        }

        static void Render(IRendererContext context, RenderPasses pass, List<IRenderable> renderableList, IDictionary<Type, IRenderer> renderers)
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
