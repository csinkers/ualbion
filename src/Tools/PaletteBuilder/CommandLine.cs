using System;
using System.Collections.Generic;
using System.IO;

namespace UAlbion.PaletteBuilder
{
    class CommandLine
    {
        public int? TransparentIndex { get; }
        public int PaletteSize { get; }
        public string[] Directories { get; }

        public CommandLine(string[] args)
        {
            if (!int.TryParse(args[0], out var paletteSize))
            {
                Console.WriteLine("Palette Size invalid.");
                return;
            }

            PaletteSize = paletteSize;

            var directories = new List<string>();
            for (int i = 1; i < args.Length; i++)
            {
                var path = args[i];
                if (path.Equals("--transparent", StringComparison.OrdinalIgnoreCase))
                {
                    if (args.Length == i) { Console.WriteLine("Missing transparency parameter"); return; } 
                    i++;
                    if (!uint.TryParse(args[i], out var temp)) { Console.WriteLine("Could not parse transparency parameter"); return; }
                    TransparentIndex = (int)temp;
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
        }
    }
}