using System;
using System.Collections.Generic;
using System.Numerics;
using SixLabors.ImageSharp.PixelFormats;

namespace UAlbion.PaletteBuilder;

public class Palette
{
    public Palette(uint[] colours) => Colours = colours;
    public int Size => Colours.Length;
    public uint[] Colours { get; }
    public byte[] Convert(ReadOnlySpan<Rgba32> pixels)
    {
        var result = new byte[pixels.Length];
        var lookup = new Dictionary<Rgba32, byte>();

        for (int i = 0; i < pixels.Length; i++)
        {
            var pixel = pixels[i];
            if (lookup.TryGetValue(pixel, out var index))
            {
                result[i] = index;
                continue;
            }

            if (pixel.A == 0)
            {
                lookup[pixel] = 0;
                result[i] = 0;
                continue;
            }

            byte min = 0;
            float best = float.MaxValue;
            for (int j = 0; j < Colours.Length; j++)
            {
                var (r, g, b) = Colour.Unpack(Colours[j]);
                var colourVector = new Vector3(r, g, b);
                var pixelVector = new Vector3(pixel.R, pixel.G, pixel.B);

                var distance2 = (pixelVector - colourVector).LengthSquared();
                if (distance2 < best)
                {
                    best = distance2;
                    min = (byte)j;
                }
            }

            lookup[pixel] = min;
            result[i] = min;
        }

        return result;
    }
}