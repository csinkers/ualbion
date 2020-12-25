using System;
using SixLabors.ImageSharp.PixelFormats;
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace UAlbion.PaletteBuilder
{
    public static class Colour
    {
        public static (byte, byte, byte) Unpack(uint n)
        {
            var r = (byte)(n & 0xff);
            var g = (byte)((n & 0xff00) >> 8);
            var b = (byte)((n & 0xff0000) >> 16);
            return (r, g, b);
        }

        public static uint Pack(byte r, byte g, byte b)
        {
            return (uint)(r | (g << 8) | (b << 16));
        }

        public static Rgba32 ToRgba32(uint n)
        {
            var (r, g, b) = Unpack(n);
            return new Rgba32(r,g,b);
        }

        public static (float, float, float) ToHsv(uint u)
        {
            var (r, g, b) = Unpack(u);

            float min = Math.Min(Math.Min(r, g), b);
            float max = Math.Max(Math.Max(r, g), b);

            float delta = max - min;

            if (max == 0)
                return (-1, 0, max); // HSV

            float h;
            if (r == max)
                h = (g - b) / delta; // between yellow & magenta
            else if (g == max)
                h = 2 + (b - r) / delta; // between cyan & yellow
            else
                h = 4 + (r - g) / delta; // between magenta & cyan

            h *= 60; // degrees
            if (h < 0)
                h += 360;

            return (h, delta / max, max);
        }
    }
}