using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Veldrid;

namespace UAlbion.Core
{
    public class ImGuiRenderable : Component, IRenderer, IRenderable
    {
        static readonly IList<Handler> Handlers = new Handler[]
        {
            new Handler<ImGuiRenderable, RenderEvent>((x, e) => e.Add(x)),
            new Handler<ImGuiRenderable, EngineUpdateEvent>((x,e) => x._imguiRenderer.Update(e.DeltaSeconds, InputTracker.FrameSnapshot)), 
            new Handler<ImGuiRenderable, WindowResizedEvent>((x,e) => x._imguiRenderer.WindowResized(e.Width, e.Height))
        };

        ImGuiRenderer _imguiRenderer;
        readonly int _width;
        readonly int _height;

        public ImGuiRenderable(int width, int height) : base(Handlers)
        {
            _width = width;
            _height = height;
        }

        public RenderPasses RenderPasses => RenderPasses.Overlay;
        public int RenderOrder => 99;
        public Type Renderer => typeof(ImGuiRenderable);

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
