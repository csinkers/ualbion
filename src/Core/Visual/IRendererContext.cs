namespace UAlbion.Core.Visual
{
    public interface IRendererContext
    {
        ICoreFactory Factory { get; }
        IFramebufferSource Framebuffer { get; }
        void UpdatePerFrameResources();
        void SetClearColor(float red, float green, float blue, float alpha);
        void StartSwapchainPass();
    }
}
