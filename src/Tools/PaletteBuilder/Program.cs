using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.ML;
using SerdesNet;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using UAlbion.Formats.Assets;

namespace UAlbion.PaletteBuilder
{
    static class Program
    {
        static void Main(string[] args)
        {
            using var stream = File.OpenRead(@"C:\Depot\bb\ualbion\mods\UATestDev\Assets\3DBCKGR0\1.bmp");
            using var sr = new BinaryReader(stream);
            using var s = new GenericBinaryReader(sr, stream.Length, Encoding.ASCII.GetString);
            var bitmap = Bitmap8Bit.Serdes(null, s);

            if (!(args?.Length > 0))
            {
                Console.WriteLine("Usage: <PaletteSize> [--transparent #] [Directories]");
                Console.WriteLine("Reads all PNG files from the given directories, then outputs");
                Console.WriteLine("the unsigned hex RGBA32 values corresponding to the best matching palette found.");
                Console.WriteLine("Options: transparent - will reserve the given palette index for transparency (i.e. black with opacity 0)");
                return;
            }

            var options = new CommandLine(args);
            var palette = BuildPalette(options);

            if (palette == null)
            {
                Console.WriteLine("Could not construct palette.");
                return;
            }

            foreach (var colour in palette.Colours)
                Console.WriteLine($"{colour:x8}");

            ConvertAll(options.Directories, palette);
        }

        static Palette BuildPalette(CommandLine options)
        {
            var context = new MLContext();
            var builder = new PaletteBuilder(context);
            Console.Write("Loading");
            foreach (var directory in options.Directories)
            {
                foreach(var file in Directory.EnumerateFiles(directory, "*.png", SearchOption.AllDirectories))
                {
                    using var stream = File.OpenRead(file);
                    var image = Image.Load<Rgba32>(stream);
                    if (!image.TryGetSinglePixelSpan(out var pixelSpan))
                    {
                        Console.WriteLine($"Could not read pixels from {file}");
                        continue;
                    }

                    builder.Add(pixelSpan);
                    Console.Write('.');
                }
            }
            Console.WriteLine();
            Console.WriteLine();

            Console.Write("Building");
            var palette = builder.Build(options.PaletteSize, options.TransparentIndex);
            Console.WriteLine();

            return palette;
        }

        static void ConvertAll(IEnumerable<string> directories, Palette palette)
        {
            // Convert and re-emit for testing
            foreach (var directory in directories)
            {
                foreach (var file in Directory.EnumerateFiles(directory, "*.png", SearchOption.AllDirectories))
                {
                    using var stream = File.OpenRead(file);
                    var image = Image.Load<Rgba32>(stream);
                    if (!image.TryGetSinglePixelSpan(out var pixelSpan))
                        continue;

                    var pixels = palette.Convert(pixelSpan);
                    WriteBitmap(Path.ChangeExtension(file, "bmp"), palette.Colours, pixels, image.Width);
                }
            }
        }

        static void WriteBitmap(string path, uint[] palette, byte[] pixels, int width)
        {
            using var stream = File.Open(path, FileMode.Create, FileAccess.Write);
            using var bw = new BinaryWriter(stream);
            using var s = new GenericBinaryWriter(bw, Encoding.ASCII.GetBytes);
            Bitmap8Bit.Serdes(new Bitmap8Bit((ushort)width, palette, pixels), s);
        }
    }
}
