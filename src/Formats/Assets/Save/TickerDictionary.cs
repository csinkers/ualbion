using System;
using System.Collections.Generic;
using System.Globalization;
using SerdesNet;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.Assets.Save
{
    public class TickerDictionary : Dictionary<TickerId, byte>
    {
        const int Min = 100;
        const int Max = 255;

        public void Serdes(ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            s.Begin();
            if (s.Mode == SerializerMode.Reading)
                Clear();

            for (int i = Min; i <= Max; i++)
            {
                this[(TickerId)i] = TryGetValue((TickerId)i, out var existing) 
                    ? s.UInt8(i.ToString(CultureInfo.InvariantCulture), existing) 
                    : s.UInt8(i.ToString(CultureInfo.InvariantCulture), 0);
            }
            s.End();
        }
    }
}
