using System;
using System.Globalization;
using SerdesNet;
using UAlbion.Config;

namespace UAlbion.Formats.Assets
{
    public class MonsterGroup
    {
        // 36 bytes = 2 bytes/slot, 6x3 combat grid.
        public MonsterId[] Grid { get; private set; } = new MonsterId[6 * 3]; // setter required for JSON

        public static MonsterGroup Serdes(int _, MonsterGroup m, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            m ??= new MonsterGroup();
            for(int i = 0; i < m.Grid.Length; i++)
            {
                m.Grid[i] = MonsterId.SerdesU8(
                    i.ToString(CultureInfo.InvariantCulture),
                    m.Grid[i],
                    mapping, s);
                s.UInt8(null, 0);
            }
            return m;
        }
    }
}
