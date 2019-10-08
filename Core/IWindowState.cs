using System.Numerics;

namespace UAlbion.Core
{
    public interface IWindowState
    {
        int Width { get; }
        int Height { get; }
        int UiWidth { get; }
        int UiHeight { get; }
        Vector2 Size { get; }
        int GuiScale { get; }
        Vector2 UiToScreen(int x, int y);
        Vector2 UiToScreenRelative(int x, int y);
    }
}