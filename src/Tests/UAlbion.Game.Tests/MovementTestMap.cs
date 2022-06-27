using System;
using System.Linq;
using UAlbion.Formats;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Game.Tests;

class MovementTestMap
{
    public MovementTestMap(string map, char startDir)
    {
        var lines = map
                .Split('\n', '\r', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .ToArray();

        Width = lines.Max(x => x.Length);
        Height = lines.Length;
        Tiles = new Passability[Width * Height];

        for (int j = 0; j < Height; j++)
        {
            var line = lines[j];
            for (int i = 0; i < Width; i++)
            {
                var c = line.Length <= i ? '.' : line[i];

                if (c == '@')
                {
                    StartPos = (i, j);
                    continue;
                }

                Tiles[j * Width + i] = c switch
                {
                    >= '0' and <= '9' => (Passability)(c - '0'),
                    >= 'a' and <= 'f' => (Passability)(10 + c - 'a'),
                    >= 'A' and <= 'F' => (Passability)(10 + c - 'A'),
                    '#' => Passability.Solid,
                    ' ' or '.' => Passability.Open,
                    _ => throw new FormatException($"Unexpected character '{c}' in map at ({i},{j})")
                };

            }
        }

        StartDir = char.ToUpperInvariant(startDir) switch
        {
            'N' => Direction.North,
            'S' => Direction.South,
            'E' => Direction.East,
            'W' => Direction.West,
            _ => throw new FormatException($"Unexpcted starting direction '{startDir}'")
        };
    }

    public Passability[] Tiles { get; }
    public int Width { get; }
    public int Height { get; }
    public (int X, int Y) StartPos { get; }
    public Direction StartDir { get; }
}