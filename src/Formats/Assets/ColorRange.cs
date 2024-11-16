using System;
using SerdesNet;

namespace UAlbion.Formats.Assets;

/// <summary>
/// Maybe useful for palette swapping?
/// </summary>
public class ColorRange
{
    ushort _pad1;

    /// <summary>
    /// Animation rate.
    /// </summary>
    public ushort Rate { get; set; }

    /// <summary>
    /// Range flags.
    /// </summary>
    public ColorRangeFlags Flags { get; set; }

    /// <summary>
    /// Low color index.
    /// </summary>
    public byte Low { get; set; }

    /// <summary>
    /// High color index.
    /// </summary>
    public byte High { get; set; }

    /// <summary>
    /// Reads range from IFF stream.
    /// </summary>
    public static ColorRange Serdes(int _, ColorRange c, ISerdes s)
    {
        ArgumentNullException.ThrowIfNull(s);
        c ??= new ColorRange();
        c._pad1 = s.UInt16BE(nameof(_pad1), c._pad1);
        c.Rate = s.UInt16BE(nameof(Rate), c.Rate);
        c.Flags = s.EnumU16BE(nameof(Flags), c.Flags);
        c.Low = s.UInt8(nameof(Low), c.Low);
        c.High = s.UInt8(nameof(High), c.High);
        return c;
    }
}