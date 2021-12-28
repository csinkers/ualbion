using System;
using System.Collections.Generic;
using System.IO;

namespace UAlbion.PaletteBuilder;

class CommandLine
{
    public int PaletteSize { get; }
    public int Offset { get; }
    public string[] Directories { get; }
    public string OutPath { get; }
    public string BitmapPath { get; }
    public string BasePath { get; }
    public int BaseOffset { get; }
    public bool ExportImages { get; }
    public bool Trim { get; }

    public CommandLine(string[] args)
    {
        PaletteSize = 255;
        Offset = 1;

        var directories = new List<string>();
        for (int i = 0; i < args.Length; i++)
        {
            var path = args[i];
            if (path.Equals("--size", StringComparison.OrdinalIgnoreCase))
            {
                if (args.Length == i) { Console.WriteLine("Missing size parameter"); return; } 
                i++;
                if (!uint.TryParse(args[i], out var temp)) { Console.WriteLine("Could not parse size parameter"); return; }
                PaletteSize = (int)temp;
            }
            else if (path.Equals("--offset", StringComparison.OrdinalIgnoreCase))
            {
                if (args.Length == i) { Console.WriteLine("Missing offset parameter"); return; } 
                i++;
                if (!uint.TryParse(args[i], out var temp)) { Console.WriteLine("Could not parse offset parameter"); return; }
                Offset = (int)temp;
            }
            else if (path.Equals("--out", StringComparison.OrdinalIgnoreCase))
            {
                if (args.Length == i) { Console.WriteLine("Missing output path parameter"); return; } 
                i++;
                OutPath = args[i];
            }
            else if (path.Equals("--outbmp", StringComparison.OrdinalIgnoreCase))
            {
                if (args.Length == i) { Console.WriteLine("Missing output bitmap path parameter"); return; } 
                i++;
                BitmapPath = args[i];
            }
            else if (path.Equals("--base", StringComparison.OrdinalIgnoreCase))
            {
                if (args.Length == i) { Console.WriteLine("Missing base path parameter"); return; } 
                i++;
                BasePath = args[i];
            }
            else if (path.Equals("--baseoffset", StringComparison.OrdinalIgnoreCase))
            {
                if (args.Length == i) { Console.WriteLine("Missing base offset parameter"); return; } 
                i++;
                if (!uint.TryParse(args[i], out var temp)) { Console.WriteLine("Could not parse base offset parameter"); return; }
                BaseOffset = (int)temp;
            }
            else if (path.Equals("--images", StringComparison.OrdinalIgnoreCase))
            {
                ExportImages = true;
            }
            else if (path.Equals("--trim", StringComparison.OrdinalIgnoreCase))
            {
                Trim = true;
            }
            else
            {
                if (!Directory.Exists(path))
                {
                    Console.WriteLine($"Directory {path} could not be found.");
                    return;
                }

                directories.Add(path);
            }
        }

        Directories = directories.ToArray();
        if (Offset + PaletteSize > 256)
            throw new ArgumentOutOfRangeException($"The offset of {Offset} and size of {PaletteSize} would result in a palette index greater than 255");
    }

    public static void PrintUsage()
    {
        Console.WriteLine("Usage: [options] [Directories]");
        Console.WriteLine("Reads all PNG files from the given directories and generates");
        Console.WriteLine("a palette to match the set of colours they contain.");
        Console.WriteLine("Note: Index 0 is always reserved for transparency");
        Console.WriteLine();
        Console.WriteLine("Options:"); 
        Console.WriteLine(" --out        Set the output file for the raw palette colours");
        Console.WriteLine(" --outbmp     Set the output file for a bitmap of the palette colours");
        Console.WriteLine(" --size       Set the palette size (defaults to 256)");
        Console.WriteLine(" --offset     Start the palette at the given index, padding all prior entries with zeroes");
        Console.WriteLine(" --base       Use the given palette file as a starting point, only the zeroed out entries will be calculated for the new palette");
        Console.WriteLine(" --baseoffset Start the base palette at the given index");
        Console.WriteLine(" --images     Re-export the source images as 8-bit bitmaps using the generated palette");
        Console.WriteLine(" --trim       When writing output file, skip directly to the non-transparent / non-skipped colours");
    }
}