using System;
using System.Collections.Generic;

namespace UAlbion.Formats.Assets.Save
{
    public class FlagSet : Dictionary<int, bool>
    {
        const int _max = 600;
        public const int PackedSize = _max / 8;

        public bool Get(int flag) => TryGetValue(flag, out var value) && value;
        public void Set(int flag, bool value)
        {
            if (flag > _max)
                throw new InvalidOperationException($"Tried to set out of range flag {flag} (greate than max: {_max})");
            this[flag] = value;
        }

        public byte[] Packed
        {
            get
            {
                var packed = new byte[(_max + 7) / 8];
                for (int i = 0; i < _max; i++)
                    packed[i / 8] |= (byte)((TryGetValue(i, out var value) && value ? 1 : 0) << (i % 8));
                return packed;
            }

            set
            {
                Clear();
                for (int i = 0; i < value.Length; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        bool flagValue = (value[i] & (1 << j)) != 0;
                        if (flagValue)
                            this[i * 8 + j] = true;
                    }
                }
            }
        }
    }
}