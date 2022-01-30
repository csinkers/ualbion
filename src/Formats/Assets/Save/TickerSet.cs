using System;
using System.Collections.Generic;
using System.Globalization;
using SerdesNet;
using UAlbion.Config;

namespace UAlbion.Formats.Assets.Save;

public class TickerSet : Dictionary<TickerId, byte>
{
    const int Min = 100;
    const int Max = 255;

    public static TickerSet Serdes(int _, TickerSet d, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        d ??= new TickerSet();
        if (s.IsReading())
            d.Clear();

        // TODO: Proper extensible modding support
        for (int i = Min; i <= Max; i++)
        {
            var assetId = new TickerId(AssetType.Ticker, i);
            d[assetId] = d.TryGetValue(assetId, out var existing)
                ? s.UInt8(i.ToString(CultureInfo.InvariantCulture), existing) 
                : s.UInt8(i.ToString(CultureInfo.InvariantCulture), 0);
        }
        return d;
    }
}