using System;
using System.Collections.Generic;

namespace UAlbion.Formats.Assets.Save
{
    public class FlagDictionary : Dictionary<SwitchId, bool>
    {
        // TODO: Proper AssetId support + a way of storing extra flags in save files.
        const int _max = 600;
        public const int PackedSize = _max / 8;

        public bool GetFlag(SwitchId flag) => TryGetValue(flag, out var value) && value;
        public void SetFlag(SwitchId flag, bool value)
        {
            if (flag.Id > _max)
                throw new InvalidOperationException($"Tried to set out of range flag {flag} (greater than max: {_max})");
            this[flag] = value;
        }

        public byte[] GetPacked()
        {

            var packed = new byte[(_max + 7) / 8];
            for (int i = 0; i < _max; i++)
                packed[i / 8] |= (byte)((TryGetValue((SwitchId)i, out var value) && value ? 1 : 0) << (i % 8));
            return packed;
        }

        public void SetPacked(byte[] packed)
        {
            Clear();
            if (packed == null) return;
            for (int i = 0; i < packed.Length; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    bool flagValue = (packed[i] & (1 << j)) != 0;
                    if (flagValue)
                        this[(SwitchId)(i * 8 + j)] = true;
                }
            }
        }
    }
}
