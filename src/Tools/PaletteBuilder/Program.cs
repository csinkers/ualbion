using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.ML;
using SerdesNet;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using UAlbion.Formats.Assets;

namespace UAlbion.PaletteBuilder;

static class Program
{
    static void Main(string[] args)
    {
        if (!(args?.Length > 0))
        {
            CommandLine.PrintUsage();
            return;
        }

        var options = new CommandLine(args);
        if(options.Directories?.Any() != true)
        {
            Console.WriteLine("No directories supplied.");
            return;
        }

        var palette = BuildPalette(options);

        if (palette == null)
        {
            Console.WriteLine("Could not construct palette.");
            return;
        }

        SavePalette(options, palette);

        if (options.ExportImages)
            ConvertAll(options.Directories, palette);
    }

    static Palette BuildPalette(CommandLine options)
    {
        Palette basePalette = null;
        if (!string.IsNullOrEmpty(options.BasePath))
            basePalette = LoadPalette(options.BasePath);

        var context = new MLContext();
        var builder = new PaletteBuilder(context);
        Console.Write("Loading");
        foreach (var directory in options.Directories)
        {
            foreach(var file in Directory.EnumerateFiles(directory, "*.png", SearchOption.AllDirectories))
            {
                using var stream = File.OpenRead(file);
                var image = Image.Load<Rgba32>(stream);
                if (!image.DangerousTryGetSinglePixelMemory(out var pixelMemory))
                {
                    Console.WriteLine($"Could not read pixels from {file}");
                    continue;
                }

                builder.Add(pixelMemory.Span);
                Console.Write('.');
            }
        }
        Console.WriteLine();
        Console.WriteLine();

        if (basePalette != null)
            builder.RemoveBaseColours(basePalette.Colours, 10.0f / 255);

        var palette = builder.Build(options.PaletteSize, options.Offset);
        Console.WriteLine();

        if (basePalette != null)
            for (int i = 0, j = options.BaseOffset; i < basePalette.Size; i++, j++)
                palette.Colours[j] = basePalette.Colours[i];

        return palette;
    }

    static void ConvertAll(IEnumerable<string> directories, Palette palette)
    {
        Console.Write("Exporting");
        // Convert and re-emit for testing
        foreach (var directory in directories)
        {
            foreach (var file in Directory.EnumerateFiles(directory, "*.png", SearchOption.AllDirectories))
            {
                using var stream = File.OpenRead(file);
                var image = Image.Load<Rgba32>(stream);
                if (!image.DangerousTryGetSinglePixelMemory(out var pixelMemory))
                    continue;

                var pixels = palette.Convert(pixelMemory.Span);
                WriteBitmap(Path.ChangeExtension(file, "bmp"), palette.Colours, pixels, image.Width);
                Console.Write('.');
            }
        }
        Console.WriteLine();
    }

    static void WriteBitmap(string path, uint[] palette, byte[] pixels, int width)
    {
        using var stream = File.Open(path, FileMode.Create, FileAccess.Write);
        using var bw = new BinaryWriter(stream);
        using var s = new GenericBinaryWriter(bw, Encoding.ASCII.GetBytes);
        Bitmap8Bit.Serdes(new Bitmap8Bit((ushort)width, palette, pixels), s);
    }

    static Palette LoadPalette(string path)
    {
        var bytes = File.ReadAllBytes(path);
        int count = bytes.Length / 3;
        var colours = new uint[count];
        for (int i = 0, j = 0; i < count; i++)
        {
            var r = bytes[j++];
            var g = bytes[j++];
            var b = bytes[j++];
            colours[i] = Colour.Pack(r, g, b);
        }

        return new Palette(colours);
    }

    static void SavePalette(CommandLine options, Palette palette)
    {
        int start = options.Trim ? options.Offset : 0;
        if (!string.IsNullOrEmpty(options.OutPath))
        {
            var paletteBytes = new byte[options.Trim ? options.PaletteSize * 3 : palette.Size * 3];
            for (int i = start, index = 0; i < palette.Size; i++)
            {
                var colour = palette.Colours[i];
                var (r, g, b) = Colour.Unpack(colour);
                paletteBytes[index++] = r;
                paletteBytes[index++] = g;
                paletteBytes[index++] = b;
            }

            File.WriteAllBytes(options.OutPath, paletteBytes);
        }

        if (!string.IsNullOrEmpty(options.BitmapPath))
        {
            var pixels = Enumerable.Range(start, palette.Colours.Length - start).Select(x => (byte) x).ToArray();
            WriteBitmap(options.BitmapPath, palette.Colours, pixels, 16);
        }
    }
}