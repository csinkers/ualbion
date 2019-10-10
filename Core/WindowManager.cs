using System;
using System.Numerics;
using Veldrid.Sdl2;

namespace UAlbion.Core
{
    public class WindowManager : IWindowManager
    {
        internal Sdl2Window Window { get; set; }

        public int PixelWidth => Window.Width;
        public int PixelHeight => Window.Height;
        public int UiWidth => 360;
        public int UiHeight => 240;
        public Vector2 Size => new Vector2(Window.Width, Window.Height);
        Vector2 UiSize => new Vector2(UiWidth, UiHeight);

        public int GuiScale
        {
            get
            {
                float widthRatio = (float)Window.Width / UiWidth;
                float heightRatio = (float)Window.Height / UiHeight;
                int scale = (int)Math.Min(widthRatio, heightRatio);
                return scale == 0 ? 1 : scale;
            }
        }

        // UI Coordinates:
        // Top left corner in original game = (0,0)
        // Bottom right corner in original game = (360, 240)
        // + bottom 48 pixels reserved for status bar, so viewport is 192 high.
        // Scaled up to nearest whole multiple that will fit
        // w/ letterboxing to compensate for aspect ratio differences.
        public Vector2 UiToNorm(Vector2 pos) => UiToNormRelative(pos - UiSize / 2.0f);
        public Vector2 NormToUi(Vector2 pos) => NormToUiRelative(pos) + UiSize / 2.0f;
            /*
            new Vector2(
                (int)(pos.X * PixelWidth / (2*GuiScale)) + UiWidth / 2,
                UiHeight / 2 - (int)(pos.Y * PixelWidth / (2 * GuiScale)));
            */
        public Vector2 NormToPixel(Vector2 pos) => NormToPixelRelative(new Vector2(pos.X + 1.0f, pos.Y - 1.0f));
            /*
            new Vector2(
                (pos.X + 1.0f) * Size.X / 2,
                (-pos.Y + 1.0f) * Size.Y / 2);
            */

        public Vector2 PixelToNorm(Vector2 pos) => PixelToNormRelative(pos) - Vector2.One;
            /*
            new Vector2(
                2 * pos.X / Size.X - 1.0f,
                1.0f - 2 * pos.Y / Size.Y);
            */

        public Vector2 UiToNormRelative(Vector2 pos) =>
            new Vector2(
                2.0f * pos.X * GuiScale / PixelWidth,
                -2.0f * pos.Y * GuiScale / PixelHeight);

        public Vector2 NormToUiRelative(Vector2 pos) =>
            new Vector2(
                (int)(pos.X * PixelWidth / (2.0f * GuiScale)),
                -(int)(pos.Y * PixelWidth / (2.0f * GuiScale)));

        public Vector2 NormToPixelRelative(Vector2 pos) =>
            new Vector2(
                pos.X * Size.X / 2.0f,
                -pos.Y * Size.Y / 2.0f);

        public Vector2 PixelToNormRelative(Vector2 pos) =>
            new Vector2(
                2.0f * pos.X / Size.X,
                -2.0f * pos.Y / Size.Y);
    }
}