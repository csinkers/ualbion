using System;
using System.Numerics;
using UAlbion.Api.Eventing;

namespace UAlbion.Core;

public class GameWindow : ServiceComponent<IGameWindow>, IGameWindow
{
    int _width;
    int _height;

    Vector2 _uiOffset;
    Vector2 _uiToNorm;
    Vector2 _normToUi;
    Vector2 _normToPixel;
    Vector2 _pixelToNorm;

    public GameWindow(int width, int height)
    {
        _width = width;
        _height = height;
    }

    public void Resize(int width, int height)
    {
        _width = width;
        _height = height;
        Recalculate();
    }

    public int GuiScale { get; private set; }
    public int UiWidth => UiConstants.UiExtents.Width;
    public int UiHeight => UiConstants.UiExtents.Height;
    public int PixelWidth => _width;
    public int PixelHeight => _height;
    public Vector2 Size => new(_width, _height);
    static Vector2 UiSize => new(UiConstants.UiExtents.Width, UiConstants.UiExtents.Height);

    // UI Coordinates:
    // Top left corner in original game = (0,0)
    // Bottom right corner in original game = (360, 240)
    // + bottom 48 pixels reserved for status bar, so viewport is 192 high.
    // Scaled up to nearest whole multiple that will fit
    // w/ letterboxing to compensate for aspect ratio differences.
    public Vector2 UiToNorm(Vector2 pos) => UiToNormRelative(pos) - _uiOffset;
    public Vector2 NormToUi(Vector2 pos) => NormToUiRelative(pos + _uiOffset);
    public Vector2 NormToPixel(Vector2 pos) => NormToPixelRelative(new Vector2(pos.X + 1.0f, pos.Y - 1.0f));
    public Vector2 PixelToNorm(Vector2 pos) => PixelToNormRelative(pos - Size / 2.0f);
    public Vector2 UiToNormRelative(Vector2 pos) => pos * _uiToNorm;
    public Vector2 NormToUiRelative(Vector2 pos) => pos * _normToUi;
    public Vector2 NormToPixelRelative(Vector2 pos) => pos * _normToPixel;
    public Vector2 PixelToNormRelative(Vector2 pos) => pos * _pixelToNorm;
    public Vector2 PixelToUi(Vector2 pos) => NormToUi(PixelToNorm(pos));

    public Rectangle UiToPixel(Rectangle rect)
    {
        var pos = NormToPixel(UiToNorm(rect.Position));
        var size = NormToPixelRelative(UiToNormRelative(rect.Size));
        return new Rectangle(
            (int)pos.X,
            (int)pos.Y,
            (int)size.X,
            (int)size.Y);
    }

    void Recalculate()
    {
        float widthRatio = (float)_width / UiWidth;
        float heightRatio = (float)_height / UiHeight;
        int scale = (int)Math.Min(widthRatio, heightRatio);
        GuiScale = scale == 0 ? 1 : scale;

        _uiToNorm = new Vector2(
            2.0f * GuiScale / _width,
            -2.0f * GuiScale / _height);

        _normToUi = new Vector2(
            _width / (2.0f * GuiScale),
            -_height / (2.0f * GuiScale));

        _normToPixel = new Vector2(
            _width / 2.0f,
            -_height / 2.0f);

        _pixelToNorm = new Vector2(
            2.0f / _width,
            -2.0f / _height);

        _uiOffset = new Vector2(
            UiSize.X * GuiScale / _width,
            1.0f - 2 * GuiScale * UiSize.Y / _height);

        // Snap to the nearest pixel
        var uiPos = NormToUiRelative(_uiOffset);
        _uiOffset = UiToNormRelative(new Vector2((int)uiPos.X, (int)uiPos.Y));
    }
}