using System;
using System.Numerics;
using UAlbion.Core.Events;

namespace UAlbion.Core
{
    public class WindowManager : ServiceComponent<IWindowManager>, IWindowManager
    {
        public WindowManager()
        {
            On<WindowResizedEvent>(_ => Recalculate());
        }

        public IWindow Window
        {
            get => _window;
            set
            {
                _window = value;
                Recalculate();
            }
        }

        public int UiWidth => UiConstants.UiExtents.Width;
        public int UiHeight => UiConstants.UiExtents.Height;
        public int PixelWidth => Window.Width;
        public int PixelHeight => Window.Height;
        public Vector2 Size => new Vector2(Window.Width, Window.Height);
        Vector2 UiSize => new Vector2(UiConstants.UiExtents.Width, UiConstants.UiExtents.Height);

        void Recalculate()
        {
            float widthRatio = (float)Window.Width / UiWidth;
            float heightRatio = (float)Window.Height / UiHeight;
            int scale = (int)Math.Min(widthRatio, heightRatio);
            GuiScale = scale == 0 ? 1 : scale;

            _uiToNorm = new Vector2(
                 2.0f * GuiScale / Window.Width,
                -2.0f * GuiScale / Window.Height);

            _normToUi = new Vector2(
                 Window.Width / (2.0f * GuiScale),
                -Window.Height / (2.0f * GuiScale));

            _normToPixel = new Vector2(
                 Window.Width / 2.0f,
                -Window.Height / 2.0f);

            _pixelToNorm = new Vector2(
                 2.0f / Window.Width,
                -2.0f / Window.Height);

            _uiOffset = new Vector2(
                UiSize.X * GuiScale / Window.Width,
                1.0f - 2 * GuiScale * UiSize.Y / Window.Height);

            // Snap to the nearest pixel
            var uiPos = NormToUiRelative(_uiOffset);
            _uiOffset = UiToNormRelative(new Vector2((int)uiPos.X, (int)uiPos.Y));
        }

        public int GuiScale { get; private set; }
        Vector2 _uiOffset;
        Vector2 _uiToNorm;
        Vector2 _normToUi;
        Vector2 _normToPixel;
        Vector2 _pixelToNorm;
        IWindow _window;

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
    }
}
