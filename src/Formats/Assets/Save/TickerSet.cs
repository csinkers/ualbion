using System.Collections.Generic;
using SerdesNet;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.Assets.Save
{
    public class TickerSet : Dictionary<TickerId, byte>
    {
        const int Min = 100;
        const int Max = 255;

        public void Serdes(ISerializer s)
        {
            s.Begin();
            if (s.Mode == SerializerMode.Reading)
                Clear();

            for (int i = Min; i <= Max; i++)
            {
                this[(TickerId)i] = TryGetValue((TickerId)i, out var existing) 
                    ? s.UInt8(i.ToString(), existing) 
                    : s.UInt8(i.ToString(), 0);
            }
            s.End();
        }
    }
}
