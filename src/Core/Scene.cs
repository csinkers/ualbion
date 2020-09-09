using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Api;
using UAlbion.Core.Events;

namespace UAlbion.Core
{
    public class Scene : Container, IScene
    {
        readonly IDictionary<Type, IList<IRenderable>> _renderables = new Dictionary<Type, IList<IRenderable>>();
        readonly IDictionary<(DrawLayer, int), List<IRenderable>> _processedRenderables = new Dictionary<(DrawLayer, int), List<IRenderable>>();
        readonly Dictionary<Type, IRenderer> _rendererLookup = new Dictionary<Type, IRenderer>();
        (float Red, float Green, float Blue) _clearColour;

        public ICamera Camera { get; }

        protected Scene(string name, ICamera camera) : base(name)
        {
            On<CollectScenesEvent>(e => e.Register(this));
            On<SetClearColourEvent>(e => _clearColour = (e.Red, e.Green, e.Blue));

            Camera = AttachChild(camera);
        }

        public override string ToString() => $"Scene:{Name}";

        public void UpdatePerFrameResources(IRendererContext context, IList<IRenderer> renderers)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            context.SetCurrentScene(this);

            // Collect all renderables from components
            foreach (var renderer in _renderables.Values)
                renderer.Clear();

            using (PerfTracker.FrameEvent("6.2.1 Collect renderables"))
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

            using (PerfTracker.FrameEvent("6.2.2 Prepare per-frame resources"))
            using (context.Factory.CreateRenderDebugGroup(context, "Prepare per-frame resources"))
            {
                _processedRenderables.Clear();
                foreach (var renderableGroup in _renderables)
                {
                    var renderer = renderers.FirstOrDefault(x => x.CanRender(renderableGroup.Key)); // TODO: Use a better pattern and remove closure alloc
                    if (renderer == null) continue;

                    foreach (var renderable in renderer.UpdatePerFrameResources(context, renderableGroup.Value)) // TODO: Avoid coroutine / enumerator alloc. Pass in empty list for it to fill etc.
                    {
                        var key = (renderable.RenderOrder, renderable.PipelineId);
                        if (!_processedRenderables.ContainsKey(key))
                            _processedRenderables[key] = new List<IRenderable>();
                        _processedRenderables[key].Add(renderable);

                        var processedType = renderable.GetType();
                        if (!_rendererLookup.ContainsKey(processedType))
                            _rendererLookup[processedType] = renderers.FirstOrDefault(x => x.CanRender(renderableGroup.Key));
                    }
                }

                CoreTrace.Log.CollectedRenderables("ProcessedRenderables",
                        _processedRenderables.Count,
                        _processedRenderables.Sum(x => x.Value.Count));

                context.UpdatePerFrameResources();
            }
        }

        public void RenderAllStages(IRendererContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            context.SetClearColor(_clearColour.Red, _clearColour.Green, _clearColour.Blue);

            var orderedKeys = _processedRenderables.Keys.OrderBy(x => x).ToArray();
            CoreTrace.Log.Info("Scene", "Sorted processed renderables");

            // Main scene
            using (PerfTracker.FrameEvent("6.2.3 Main scene pass"))
            using (context.Factory.CreateRenderDebugGroup(context, "Main Pass"))
            {
                context.StartSwapchainPass();
                foreach (var key in orderedKeys)
                    Render(context, RenderPasses.Standard, _processedRenderables[key]);
            }
        }

        void Render(IRendererContext context, RenderPasses pass, List<IRenderable> renderableList)
        {
            foreach (IRenderable renderable in renderableList)
            {
                var renderer = _rendererLookup[renderable.GetType()];
                if ((renderer.RenderPasses & pass) != 0)
                    renderer.Render(context, pass, renderable);
            }
        }
    }
}
