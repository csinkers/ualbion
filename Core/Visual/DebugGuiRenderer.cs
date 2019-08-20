using System;
using System.Collections.Generic;
using System.Diagnostics;
using UAlbion.Core.Events;
using Veldrid;

namespace UAlbion.Core.Visual
{
    public class DebugGuiRenderer : Component, IRenderer, IRenderable
    {
        static readonly IList<Handler> Handlers = new Handler[]
        {
            new Handler<DebugGuiRenderer, RenderEvent>((x, e) => e.Add(x)),
            new Handler<DebugGuiRenderer, InputEvent>((x,e) => x._imguiRenderer.Update((float)e.DeltaSeconds, e.Snapshot)), 
            new Handler<DebugGuiRenderer, WindowResizedEvent>((x,e) => x._imguiRenderer.WindowResized(e.Width, e.Height))
        };

        ImGuiRenderer _imguiRenderer;
        readonly int _width;
        readonly int _height;

        public DebugGuiRenderer(int width, int height) : base(Handlers)
        {
            _width = width;
            _height = height;
        }

        public RenderPasses RenderPasses => RenderPasses.Overlay;
        public int RenderOrder => 99;
        public Type Renderer => typeof(DebugGuiRenderer);

        public void CreateDeviceObjects(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
            if (_imguiRenderer == null)
            {
                _imguiRenderer = new ImGuiRenderer(gd, sc.MainSceneFramebuffer.OutputDescription, _width, _height, ColorSpaceHandling.Linear);
            }
            else
            {
                _imguiRenderer.CreateDeviceResources(gd, sc.MainSceneFramebuffer.OutputDescription, ColorSpaceHandling.Linear);
            }
        }

        public IEnumerable<IRenderable> UpdatePerFrameResources(GraphicsDevice gd, CommandList cl, SceneContext sc, IEnumerable<IRenderable> renderables) => renderables;

        public void DestroyDeviceObjects()
        {
            _imguiRenderer.Dispose();
        }

        public void Render(GraphicsDevice gd, CommandList cl, SceneContext sc, RenderPasses renderPass, IRenderable r)
        {
            Debug.Assert(renderPass == RenderPasses.Overlay);
            _imguiRenderer.Render(gd, cl);
        }

        public void UpdatePerFrameResources(GraphicsDevice gd, CommandList cl, SceneContext sc, IRenderable r) { }

        public void Dispose() { DestroyDeviceObjects(); }
    }
}
