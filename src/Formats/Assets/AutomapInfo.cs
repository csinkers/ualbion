using System;
using SerdesNet;

namespace UAlbion.Formats.Assets;

public class AutomapInfo // Total size 0x13 = 19 bytes
{
    public const int MaxNameLength = 15;
    public byte X { get; set; }
    public byte Y { get; set; }
    public byte Unk2 { get; set; }
    public byte MarkerId { get; set; }
    /// <summary>
    /// This is only for debugging / map editing purposes. The real name is pulled from item 0 in the map's MapText file: the string
    /// is broken up with {BLOK###} directives, BLOK000 matches the first AutomapInfo in the map, BLOK001 the second etc.
    /// </summary>
    public string Name { get; set; } // name length = 15

    public static AutomapInfo Serdes(int _, AutomapInfo existing, ISerializer s)
    {
        ArgumentNullException.ThrowIfNull(s);
        var info = existing ?? new AutomapInfo();
        info.X = s.UInt8(nameof(X), info.X); // 0
        info.Y = s.UInt8(nameof(Y), info.Y); // 1
        info.Unk2 = s.UInt8(nameof(Unk2), info.Unk2); // 2
        info.MarkerId = s.UInt8(nameof(MarkerId), info.MarkerId); // 3
        info.Name = s.FixedLengthString(nameof(Name), info.Name, 15); // 4
        return info;
    }
}