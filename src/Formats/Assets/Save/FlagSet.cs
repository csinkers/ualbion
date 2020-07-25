using System;
using System.Collections.Generic;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.Assets.Save
{
    public class FlagSet : Dictionary<SwitchId, bool>
    {
        const SwitchId _max = (SwitchId)600;
        public const int PackedSize = (int)_max / 8;

        public bool Get(SwitchId flag) => TryGetValue(flag, out var value) && value;
        public void Set(SwitchId flag, bool value)
        {
            if (flag > _max)
                throw new InvalidOperationException($"Tried to set out of range flag {flag} (greater than max: {(int)_max})");
            this[flag] = value;
        }

        public byte[] Packed
        {
            get
            {
                var packed = new byte[((int)_max + 7) / 8];
                for (int i = 0; i < (int)_max; i++)
                    packed[i / 8] |= (byte)((TryGetValue((SwitchId)i, out var value) && value ? 1 : 0) << (i % 8));
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
                            this[(SwitchId)(i * 8 + j)] = true;
                    }
                }
            }
        }
    }
}