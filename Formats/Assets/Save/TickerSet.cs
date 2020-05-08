using System.Collections.Generic;
using SerdesNet;

namespace UAlbion.Formats.Assets.Save
{
    public class TickerSet : Dictionary<int, byte>
    {
        const int Min = 100;
        const int Max = 255;

        public void Serdes(ISerializer s)
        {
            if (s.Mode == SerializerMode.Reading)
                Clear();

            for (int i = Min; i <= Max; i++)
            {
                this[i] = TryGetValue(i, out var existing) 
                    ? s.UInt8(i.ToString(), existing) 
                    : s.UInt8(i.ToString(), 0);
            }
        }
    }
}