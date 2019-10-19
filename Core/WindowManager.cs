using System;
using System.Numerics;
using UAlbion.Core.Events;
using Veldrid.Sdl2;

namespace UAlbion.Core
{
    public class WindowManager : Component, IWindowManager
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<WindowManager, WindowResizedEvent>((x, _) => x.Recalculate())
        );

        public WindowManager() : base(Handlers) { }

        internal Sdl2Window Window
        {
            get => _window;
            set
            {
                _window = value;
                Recalculate();
            }
        }

        public int UiWidth => 360;
        public int UiHeight => 240;
        const int StatusBarHeight = 48;
        public int PixelWidth => Window.Width;
        public int PixelHeight => Window.Height;
        public Vector2 Size => new Vector2(Window.Width, Window.Height);
        Vector2 UiSize => new Vector2(UiWidth, UiHeight);

        void Recalculate()
        {
            float widthRatio = (float)Window.Width / UiWidth;
            float heightRatio = (float)Window.Height / UiHeight;
            int scale = (int)Math.Min(widthRatio, heightRatio);
            GuiScale = scale == 0 ? 1 : scale;

            _uiToNormX =  2.0f * GuiScale / Window.Width;
            _uiToNormY = -2.0f * GuiScale / Window.Height;

            _normToUiX =  Window.Width  / (2.0f * GuiScale);
            _normToUiY = -Window.Height / (2.0f * GuiScale);

            _normToPixelX =  Window.Width  / 2.0f;
            _normToPixelY = -Window.Height / 2.0f;

            _pixelToNormX =  2.0f / Window.Width;
            _pixelToNormY = -2.0f / Window.Height;
        }

        public int GuiScale { get; private set; }
        float _uiToNormX;
        float _uiToNormY;
        float _normToUiX;
        float _normToUiY;
        float _normToPixelX;
        float _normToPixelY;
        float _pixelToNormX;
        float _pixelToNormY;
        Sdl2Window _window;

        // UI Coordinates:
        // Top left corner in original game = (0,0)
        // Bottom right corner in original game = (360, 240)
        // + bottom 48 pixels reserved for status bar, so viewport is 192 high.
        // Scaled up to nearest whole multiple that will fit
        // w/ letterboxing to compensate for aspect ratio differences.
        public Vector2 UiToNorm(Vector2 pos) => UiToNormRelative(pos - UiSize / 2.0f);
        public Vector2 NormToUi(Vector2 pos) => NormToUiRelative(pos) + UiSize / 2.0f;
        public Vector2 NormToPixel(Vector2 pos) => NormToPixelRelative(new Vector2(pos.X + 1.0f, pos.Y - 1.0f));
        public Vector2 PixelToNorm(Vector2 pos) => PixelToNormRelative(pos - Size / 2.0f);
        public Vector2 UiToNormRelative(Vector2 pos) => new Vector2(_uiToNormX * pos.X, _uiToNormY * pos.Y);
        public Vector2 NormToUiRelative(Vector2 pos) => new Vector2(pos.X * _normToUiX, pos.Y * _normToUiY);
        public Vector2 NormToPixelRelative(Vector2 pos) => new Vector2(pos.X * _normToPixelX, pos.Y * _normToPixelY);
        public Vector2 PixelToNormRelative(Vector2 pos) => new Vector2(pos.X * _pixelToNormX, pos.Y * _pixelToNormY);
    }
}