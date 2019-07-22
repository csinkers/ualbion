using System.Diagnostics;
using System.Numerics;
using Veldrid;

namespace UAlbion.Core
{
    public class ImGuiRenderable : Renderable, IComponent
    {
        ImGuiRenderer _imguiRenderer;
        readonly int _width;
        readonly int _height;

        public ImGuiRenderable(int width, int height)
        {
            _width = width;
            _height = height;
        }

        public void Attach(EventExchange exchange)
        {
            exchange.Subscribe<EngineUpdateEvent>(this);
            exchange.Subscribe<WindowResizedEvent>(this);
        }

        public void Receive(IEvent @event, object sender)
        {
            switch (@event)
            {
                case EngineUpdateEvent e:
                    _imguiRenderer.Update(e.DeltaSeconds, InputTracker.FrameSnapshot);
                    break;
                case WindowResizedEvent e:
                    _imguiRenderer.WindowResized(e.Width, e.Height);
                    break;
            }
        }

        public override void CreateDeviceObjects(GraphicsDevice gd, CommandList cl, SceneContext sc)
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

        public override void DestroyDeviceObjects()
        {
            _imguiRenderer.Dispose();
        }

        public override RenderOrderKey GetRenderOrderKey(Vector3 cameraPosition)
        {
            return new RenderOrderKey(ulong.MaxValue);
        }

        public override void Render(GraphicsDevice gd, CommandList cl, SceneContext sc, RenderPasses renderPass)
        {
            Debug.Assert(renderPass == RenderPasses.Overlay);
            _imguiRenderer.Render(gd, cl);
        }

        public override void UpdatePerFrameResources(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
        }

        public override RenderPasses RenderPasses => RenderPasses.Overlay;

    }
}
