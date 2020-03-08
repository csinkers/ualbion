using UAlbion.Api;
using UAlbion.Core.Textures;
using UAlbion.Core.Veldrid.Textures;
using Veldrid;

namespace UAlbion.Core.Veldrid
{
    public class VeldridRendererContext : IRendererContext
    {
        VeldridPaletteTexture _paletteTexture;
        int _lastPaletteVersion = -1;
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
        public void SetCurrentScene(Scene scene) => SceneContext.SetCurrentScene(scene);
        public void SetClearColor(float red, float green, float blue) => _clearColour = new RgbaFloat(red, green, blue, 1.0f);
        public void SetCurrentPalette(PaletteTexture newPalette, int newVersion)
        {
            if (SceneContext.PaletteView != null && _paletteTexture == newPalette && _lastPaletteVersion == newVersion)
                return;

            SceneContext.PaletteView?.Dispose();
            SceneContext.PaletteTexture?.Dispose();
            CoreTrace.Log.Info("Scene", "Disposed palette device texture");
            _paletteTexture = (VeldridPaletteTexture)newPalette;
            _lastPaletteVersion = newVersion;
            SceneContext.PaletteTexture = _paletteTexture.CreateDeviceTexture(GraphicsDevice, GraphicsDevice.ResourceFactory, TextureUsage.Sampled);
            SceneContext.PaletteView = GraphicsDevice.ResourceFactory.CreateTextureView(SceneContext.PaletteTexture);
        }

        public void UpdatePerFrameResources()
        {
            SceneContext.UpdatePerFrameResources(GraphicsDevice, CommandList);
        }

        public void StartMainPass()
        {
            CommandList.SetFramebuffer(SceneContext.MainSceneFramebuffer);
            var fbWidth = SceneContext.MainSceneFramebuffer.Width;
            var fbHeight = SceneContext.MainSceneFramebuffer.Height;
            CommandList.SetViewport(0, new Viewport(0, 0, fbWidth, fbHeight, 0, 1));
            CommandList.SetFullViewports();
            CommandList.SetFullScissorRects();
            CommandList.ClearColorTarget(0, _clearColour);
            CommandList.ClearDepthStencil(GraphicsDevice.IsDepthRangeZeroToOne ? 1f : 0f);
        }
        public void StartOverlayPass() { }

        public void StartDuplicatorPass()
        {
            if (SceneContext.MainSceneColorTexture.SampleCount != TextureSampleCount.Count1)
                CommandList.ResolveTexture(SceneContext.MainSceneColorTexture, SceneContext.MainSceneResolvedColorTexture);

            CommandList.SetFramebuffer(SceneContext.DuplicatorFramebuffer);
            CommandList.SetFullViewports();
        }

        public void StartSwapchainPass()
        {
            CommandList.SetFramebuffer(GraphicsDevice.SwapchainFramebuffer);
            CommandList.SetFullViewports();
        }
    }
}