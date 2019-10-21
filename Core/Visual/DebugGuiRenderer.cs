using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core.Events;
using Veldrid;
using Veldrid.Utilities;

namespace UAlbion.Core.Visual
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
        public RenderPasses RenderPasses => RenderPasses.Standard;
        public int RenderOrder => (int)DrawLayer.Debug;
        public Type Renderer => typeof(DebugGuiRenderer);
        public BoundingBox? Extents => null;
        public Matrix4x4 Transform => Matrix4x4.Identity;
        public event EventHandler ExtentsChanged;

        public void CreateDeviceObjects(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
            if (_imguiRenderer == null)
            {
                var window = Resolve<IWindowManager>();
                _imguiRenderer = new ImGuiRenderer(gd, sc.MainSceneFramebuffer.OutputDescription, window.PixelWidth, window.PixelHeight, ColorSpaceHandling.Linear);
            }
            else
            {
                _imguiRenderer.CreateDeviceResources(gd, sc.MainSceneFramebuffer.OutputDescription, ColorSpaceHandling.Linear);
            }
        }

        public IEnumerable<IRenderable> UpdatePerFrameResources(GraphicsDevice gd, CommandList cl, SceneContext sc, IEnumerable<IRenderable> renderables) => renderables;

        public void DestroyDeviceObjects()
        {
            _imguiRenderer?.Dispose();
        }

        public void Render(GraphicsDevice gd, CommandList cl, SceneContext sc, RenderPasses renderPass, IRenderable r)
        {
            Debug.Assert(renderPass == RenderPasses.Standard);
            _imguiRenderer.Render(gd, cl);
        }

        public void UpdatePerFrameResources(GraphicsDevice gd, CommandList cl, SceneContext sc, IRenderable r) { }

        public void Dispose() { DestroyDeviceObjects(); }
    }
}
