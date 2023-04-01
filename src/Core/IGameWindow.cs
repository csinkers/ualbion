using System;
using System.Numerics;

namespace UAlbion.Core;

public interface IGameWindow
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
    Vector2 PixelToUi(Vector2 pos);
    Rectangle UiToPixel(Rectangle rect);
}

public static class WindowManagerExtensions
{
    public static Vector2 UiToNorm(this IGameWindow wm, float x, float y)
    {
        if (wm == null) throw new ArgumentNullException(nameof(wm));
        return wm.UiToNorm(new Vector2(x, y));
    }

    public static Vector2 NormToUi(this IGameWindow wm, float x, float y)
    {
        if (wm == null) throw new ArgumentNullException(nameof(wm));
        return wm.NormToUi(new Vector2(x, y));
    }

    public static Vector2 NormToPixel(this IGameWindow wm, float x, float y)
    {
        if (wm == null) throw new ArgumentNullException(nameof(wm));
        return wm.NormToPixel(new Vector2(x, y));
    }

    public static Vector2 PixelToNorm(this IGameWindow wm, float x, float y)
    {
        if (wm == null) throw new ArgumentNullException(nameof(wm));
        return wm.PixelToNorm(new Vector2(x, y));
    }

    public static Vector2 UiToNormRelative(this IGameWindow wm, float x, float y)
    {
        if (wm == null) throw new ArgumentNullException(nameof(wm));
        return wm.UiToNormRelative(new Vector2(x, y));
    }

    public static Vector2 NormToUiRelative(this IGameWindow wm, float x, float y)
    {
        if (wm == null) throw new ArgumentNullException(nameof(wm));
        return wm.NormToUiRelative(new Vector2(x, y));
    }

    public static Vector2 NormToPixelRelative(this IGameWindow wm, float x, float y)
    {
        if (wm == null) throw new ArgumentNullException(nameof(wm));
        return wm.NormToPixelRelative(new Vector2(x, y));
    }

    public static Vector2 PixelToNormRelative(this IGameWindow wm, float x, float y)
    {
        if (wm == null) throw new ArgumentNullException(nameof(wm));
        return wm.PixelToNormRelative(new Vector2(x, y));
    }
}