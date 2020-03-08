using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid.Events;
using Veldrid;

namespace UAlbion.Core.Veldrid.Visual
{
    public class DebugGuiRenderer : Component, IRenderer, IRenderable
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<DebugGuiRenderer, RenderEvent>((x, e) => e.Add(x)),
            H<DebugGuiRenderer, InputEvent>((x, e) => x._imguiRenderer.Update((float)e.DeltaSeconds, e.Snapshot)),
            H<DebugGuiRenderer, WindowResizedEvent>((x, e) => x._imguiRenderer.WindowResized(e.Width, e.Height))
        );

        ImGuiRenderer _imguiRenderer;

        public DebugGuiRenderer() : base(Handlers) { }

        public string Name => "DebugGuiRenderer";
        public bool CanRender(Type renderable) => renderable == typeof(DebugGuiRenderer);

        public RenderPasses RenderPasses => RenderPasses.Standard;
        public DrawLayer RenderOrder => DrawLayer.Debug;
        public int PipelineId => 1;

        public Type Renderer => typeof(DebugGuiRenderer);
        public Matrix4x4 Transform => Matrix4x4.Identity;

        public void CreateDeviceObjects(IRendererContext context)
        {
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
            var c = (VeldridRendererContext)context;
            ApiUtil.Assert(renderPass == RenderPasses.Standard);
            _imguiRenderer.Render(c.GraphicsDevice, c.CommandList);
            c.CommandList.SetFullScissorRects();
        }

        public void UpdatePerFrameResources(IRendererContext context, IRenderable r) { }

        public void Dispose() { DestroyDeviceObjects(); }
    }
}
