namespace UAlbion.Core.Visual
{
    public interface IRendererContext
    {
        ICoreFactory Factory { get; }
        void UpdatePerFrameResources();
        void SetClearColor(float red, float green, float blue);
        void StartSwapchainPass();
    }
}
