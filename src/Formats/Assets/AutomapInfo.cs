using System;
using SerdesNet;

namespace UAlbion.Formats.Assets;

public class AutomapInfo
{
    public const int MaxNameLength = 15;
    public byte X { get; set; }
    public byte Y { get; set; }
    public byte Unk2 { get; set; }
    public byte Unk3 { get; set; }
    public string Name { get; set; } // name length = 15

    public static AutomapInfo Serdes(int _, AutomapInfo existing, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        var info = existing ?? new AutomapInfo();
        info.X = s.UInt8(nameof(X), info.X); // 0
        info.Y = s.UInt8(nameof(Y), info.Y); // 1
        info.Unk2 = s.UInt8(nameof(Unk2), info.Unk2); // 2
        info.Unk3 = s.UInt8(nameof(Unk3), info.Unk3); // 3
        info.Name = s.FixedLengthString(nameof(Name), info.Name, 15); // 4
        return info;
    }
}