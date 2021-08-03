using System;
using System.Collections.Generic;
using UAlbion.Config;

namespace UAlbion.Formats.Assets.Save
{
    public class FlagDictionary
    {
        // TODO: Proper AssetId support + a way of storing extra flags in save files.
        public const int OriginalSaveGameMax = 600;
        public static int PackedSize(int min, int max) => (max-min + 7) / 8;

        readonly HashSet<SwitchId> _set = new();

        public bool GetFlag(SwitchId flag) => _set.Contains(flag);
        public void SetFlag(SwitchId flag, bool value)
        {
            if (value) _set.Add(flag);
            else _set.Remove(flag);
        }

        public byte[] GetPacked(int from, int to, AssetMapping mapping)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            var packed = new byte[PackedSize(from, to)];
            for (int i = 0; i < to-from; i++)
            {
                var diskId = new SwitchId(AssetType.Switch, i + from);
                var globalId = AssetMapping.Global.EnumToId(mapping.IdToEnum(diskId));
                packed[i / 8] |= (byte)((_set.Contains(globalId) ? 1 : 0) << (i % 8));
            }

            return packed;
        }

        public void SetPacked(int offset, byte[] packed, AssetMapping mapping)
        {
            if (mapping == null)
                throw new ArgumentNullException(nameof(mapping));

            _set.Clear();
            if (packed == null)
                return;

            for (int i = 0; i < packed.Length; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    bool flagValue = (packed[i] & (1 << j)) != 0;
                    int diskId = i * 8 + j + offset;
                    SwitchId id = AssetMapping.Global.EnumToId(mapping.IdToEnum(new SwitchId(AssetType.Switch, diskId)));
                    if (flagValue)
                        _set.Add(id);
                }
            }
        }
    }
}
