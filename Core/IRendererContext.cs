using UAlbion.Core.Textures;

namespace UAlbion.Core
{
    public interface IRendererContext
    {
        ICoreFactory Factory { get; }
        void UpdatePerFrameResources();
        void SetCurrentScene(Scene scene);
        void SetClearColor(float red, float green, float blue);
        void SetCurrentPalette(PaletteTexture newPalette, int version);
        void StartMainPass();
        void StartOverlayPass();
        void StartDuplicatorPass();
        void StartSwapchainPass();
    }
}