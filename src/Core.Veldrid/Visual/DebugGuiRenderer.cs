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
            On<RenderEvent>(e => e.Add(this));
            On<InputEvent>(e => _imguiRenderer.Update((float)e.DeltaSeconds, e.Snapshot));
            On<WindowResizedEvent>(e => _imguiRenderer.WindowResized(e.Width, e.Height));
        }

        public string Name => "DebugGuiRenderer";
        public bool CanRender(Type renderable) => renderable == typeof(DebugGuiRenderer);

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
                _imguiRenderer = new ImGuiRenderer(c.GraphicsDevice, c.SceneContext.MainSceneFramebuffer.OutputDescription, window.PixelWidth, window.PixelHeight, ColorSpaceHandling.Linear);
            }
            else
            {
                _imguiRenderer.CreateDeviceResources(c.GraphicsDevice, c.SceneContext.MainSceneFramebuffer.OutputDescription, ColorSpaceHandling.Linear);
            }
        }

        public IEnumerable<IRenderable> UpdatePerFrameResources(IRendererContext context, IEnumerable<IRenderable> renderables) => renderables;

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
