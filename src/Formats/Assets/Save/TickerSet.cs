using System;
using System.Collections.Generic;
using System.Globalization;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.Assets.Save;

public class TickerSet : Dictionary<TickerId, byte>
{
    const int Max = 255;

    public static TickerSet Serdes(string _, TickerSet d, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        d ??= new TickerSet();
        if (s.IsReading())
            d.Clear();

        // TODO: Proper extensible modding support
        for (int i = 0; i <= Max; i++)
        {
            var assetId = new TickerId(AssetType.Ticker, i);
            d[assetId] = d.TryGetValue(assetId, out var existing)
                ? s.UInt8(i.ToString(CultureInfo.InvariantCulture), existing) 
                : s.UInt8(i.ToString(CultureInfo.InvariantCulture), 0);
        }
        return d;
    }
}