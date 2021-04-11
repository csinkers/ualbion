using UAlbion.Core.Visual;
using Veldrid;

namespace UAlbion.Core.Veldrid
{
    public class VeldridRendererContext : IRendererContext
    {
        RgbaFloat _clearColour;

        public VeldridRendererContext(GraphicsDevice graphicsDevice, CommandList commandList, SceneContext sceneContext, ICoreFactory factory)
        {
            GraphicsDevice = graphicsDevice;
            CommandList = commandList;
            SceneContext = sceneContext;
            Factory = factory;
        }

        public GraphicsDevice GraphicsDevice { get; }
        public CommandList CommandList { get; }
        public SceneContext SceneContext { get; }
        public ICoreFactory Factory { get; }
        public void SetClearColor(float red, float green, float blue) => _clearColour = new RgbaFloat(red, green, blue, 1.0f);
        public void UpdatePerFrameResources()
        {
            SceneContext.UpdatePerFrameResources(GraphicsDevice, CommandList);
        }

        public void StartSwapchainPass()
        {
            CommandList.SetFramebuffer(GraphicsDevice.SwapchainFramebuffer);
            CommandList.SetFullViewports();
            CommandList.SetFullScissorRects();
            CommandList.ClearColorTarget(0, _clearColour);
            CommandList.ClearDepthStencil(GraphicsDevice.IsDepthRangeZeroToOne ? 1f : 0f);
        }
    }
}
