using UAlbion.Core.Veldrid.Visual;
using UAlbion.Core.Visual;
using Veldrid;

namespace UAlbion.Core.Veldrid
{
    public class VeldridRendererContext : IRendererContext
    {
        readonly FramebufferSource _framebuffer;
        RgbaFloat _clearColour;

        public VeldridRendererContext(GraphicsDevice graphicsDevice, CommandList commandList, SceneContext sceneContext, ICoreFactory factory, FramebufferSource framebuffer)
        {
            GraphicsDevice = graphicsDevice;
            CommandList = commandList;
            SceneContext = sceneContext;
            Factory = factory;
            _framebuffer = framebuffer;
        }

        public GraphicsDevice GraphicsDevice { get; }
        public CommandList CommandList { get; }
        public SceneContext SceneContext { get; }
        public ICoreFactory Factory { get; }
        public IFramebufferSource Framebuffer => _framebuffer;

        public void SetClearColor(float red, float green, float blue, float alpha) => _clearColour = new RgbaFloat(red, green, blue, alpha);
        public void UpdatePerFrameResources()
        {
            SceneContext.UpdatePerFrameResources(GraphicsDevice, CommandList, Framebuffer);
        }

        public void StartSwapchainPass()
        {
            if (_framebuffer != null)
            {
                _framebuffer.CreateDeviceObjects(this);
                CommandList.SetFramebuffer(_framebuffer.Framebuffer);
            }
            else
                CommandList.SetFramebuffer(GraphicsDevice.SwapchainFramebuffer);
            CommandList.SetFullViewports();
            CommandList.SetFullScissorRects();
            CommandList.ClearColorTarget(0, _clearColour);
            CommandList.ClearDepthStencil(GraphicsDevice.IsDepthRangeZeroToOne ? 1f : 0f);
        }
    }
}
