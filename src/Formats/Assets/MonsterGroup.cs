using System;
using System.Text.Json.Serialization;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets.Save;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.Assets;

public class MonsterGroup
{
    // 36 bytes = 2 bytes/slot, 6x3 combat grid.
    [JsonInclude]
    public MonsterId[] Grid { get; private set; } = new MonsterId[SavedGame.CombatColumns * SavedGame.CombatRowsForMobs];

    public static MonsterGroup Serdes(int _, MonsterGroup m, AssetMapping mapping, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        m ??= new MonsterGroup();
        for (int i = 0; i < m.Grid.Length; i++)
        {
            m.Grid[i] = MonsterId.SerdesU8(
                i.ToString(),
                m.Grid[i],
                mapping, s);
            s.UInt8(null, 0);
        }
        return m;
    }
}
