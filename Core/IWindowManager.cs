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
        Rectangle UiToPixel(Rectangle rect);
    }

    public static class WindowManagerExtensions
    {
        public static Vector2 UiToNorm(this IWindowManager wm, float x, float y) => wm.UiToNorm(new Vector2(x, y));
        public static Vector2 NormToUi(this IWindowManager wm, float x, float y) => wm.NormToUi(new Vector2(x, y));
        public static Vector2 NormToPixel(this IWindowManager wm, float x, float y) => wm.NormToPixel(new Vector2(x, y));
        public static Vector2 PixelToNorm(this IWindowManager wm, float x, float y) => wm.PixelToNorm(new Vector2(x, y));
        public static Vector2 UiToNormRelative(this IWindowManager wm, float x, float y) => wm.UiToNormRelative(new Vector2(x, y));
        public static Vector2 NormToUiRelative(this IWindowManager wm, float x, float y) => wm.NormToUiRelative(new Vector2(x, y));
        public static Vector2 NormToPixelRelative(this IWindowManager wm, float x, float y) => wm.NormToPixelRelative(new Vector2(x, y));
        public static Vector2 PixelToNormRelative(this IWindowManager wm, float x, float y) => wm.PixelToNormRelative(new Vector2(x, y));
    }
}
