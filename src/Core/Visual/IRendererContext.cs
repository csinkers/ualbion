using UAlbion.Core.Textures;

namespace UAlbion.Core.Visual
{
    public interface IRendererContext
    {
        ICoreFactory Factory { get; }
        void UpdatePerFrameResources(ICamera camera);
        void SetClearColor(float red, float green, float blue);
        void SetCurrentPalette(PaletteTexture newPalette, int version);
        void StartSwapchainPass();
    }
}
