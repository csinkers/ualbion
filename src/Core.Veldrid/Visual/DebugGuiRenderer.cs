using System;
using System.Collections.Generic;
using UAlbion.Api;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid.Events;
using Veldrid;

namespace UAlbion.Core.Veldrid.Visual
{
    public sealed class DebugGuiRenderer : Component, IRenderer, IRenderable
    {
        ImGuiRenderer _imguiRenderer;

        public DebugGuiRenderer()
        {
            On<InputEvent>(e => _imguiRenderer.Update((float)e.DeltaSeconds, e.Snapshot));
            On<WindowResizedEvent>(e => _imguiRenderer.WindowResized(e.Width, e.Height));
        }

        public string Name => "DebugGuiRenderer";
        public Type[] RenderableTypes => new[] { typeof(DebugGuiRenderer) };

        protected override void Subscribed()
        {
            Resolve<IEngine>()?.RegisterRenderable(this);
            base.Subscribed();
        }

        protected override void Unsubscribed()
        {
            Resolve<IEngine>()?.RegisterRenderable(this);
            base.Unsubscribed();
        }

        public RenderPasses RenderPasses => RenderPasses.Standard;
        public DrawLayer RenderOrder => DrawLayer.Debug;
        public int PipelineId => 1;

        public void CreateDeviceObjects(IRendererContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            var c = (VeldridRendererContext)context;
            if (_imguiRenderer == null)
            {
                var window = Resolve<IWindowManager>();
                _imguiRenderer = new ImGuiRenderer(c.GraphicsDevice, c.GraphicsDevice.SwapchainFramebuffer.OutputDescription, window.PixelWidth, window.PixelHeight, ColorSpaceHandling.Linear);
            }
            else
            {
                _imguiRenderer.CreateDeviceResources(c.GraphicsDevice, c.GraphicsDevice.SwapchainFramebuffer.OutputDescription, ColorSpaceHandling.Linear);
            }
        }

        public void UpdatePerFrameResources(IRendererContext context, IEnumerable<IRenderable> renderables, IList<IRenderable> results)
        {
            if (renderables == null) throw new ArgumentNullException(nameof(renderables));
            if (results == null) throw new ArgumentNullException(nameof(results));
            foreach (var r in renderables)
                results.Add(r);
        }

        public void DestroyDeviceObjects()
        {
            _imguiRenderer?.Dispose();
        }

        public void Render(IRendererContext context, RenderPasses renderPass, IRenderable r)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (r == null) throw new ArgumentNullException(nameof(r));
            var c = (VeldridRendererContext)context;
            ApiUtil.Assert(renderPass == RenderPasses.Standard);
            _imguiRenderer.Render(c.GraphicsDevice, c.CommandList);
            c.CommandList.SetFullScissorRects();
        }

        public void Dispose()
        {
            DestroyDeviceObjects();
            GC.SuppressFinalize(this);
        }
    }
}
