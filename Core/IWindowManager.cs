using System.Numerics;

namespace UAlbion.Core
{
    public interface IWindowManager
    {
        int PixelWidth { get; }
        int PixelHeight { get; }
        int UiWidth { get; }
        int UiHeight { get; }
        Vector2 Size { get; }
        int GuiScale { get; }
        Vector2 UiToNorm(Vector2 pos);
        Vector2 NormToUi(Vector2 pos);
        Vector2 NormToPixel(Vector2 pos);
        Vector2 PixelToNorm(Vector2 pos);
        Vector2 UiToNormRelative(Vector2 pos);
        Vector2 NormToUiRelative(Vector2 pos);
        Vector2 NormToPixelRelative(Vector2 pos);
        Vector2 PixelToNormRelative(Vector2 pos);
    }
}
